# GardenLog AI Agent System - Technical Design Document

## Document Information
- **Created:** February 17, 2026
- **Status:** Approved Design
- **Purpose:** Technical specification for AI agent integration via MCP server

## Executive Summary

This document describes the design and implementation plan for adding AI agent capabilities to GardenLog. The system will enable external AI agents (GitHub Copilot, Claude Desktop, custom agents) to answer natural language questions about historical garden data and perform operations on behalf of users.

**Key Features:**
- Query historical planting data: "When do I typically plant onions?"
- Analyze garden bed usage: "What was grown in Garden Bed 3 last year?"
- Search harvest records: "Show me all failed plantings in 2024"
- Get aggregated statistics: "What's my average tomato yield?"
- Future: Create harvest cycles, log activities via natural language

## Architecture Overview

### Design Decision: Centralized MCP Server

After evaluating embedded (MCP in each API) vs centralized approaches, **centralized architecture** was selected:

**Single `GardenLog.Mcp` microservice that:**
- Exposes MCP protocol endpoint for external agents
- Reads directly from MongoDB for optimized analytical queries
- Calls production APIs for write operations (Phase 2)
- Maintains security and data isolation
- Provides unified tool namespace for agents

### Architecture Diagram

```
┌─────────────────────────────────────────────────────┐
│         External AI Agents                          │
│  (GitHub Copilot, Claude Desktop, Custom Agents)    │
└─────────────────┬───────────────────────────────────┘
                  │ MCP Protocol (JSON-RPC)
                  │ Auth: JWT Bearer Token
                  ↓
┌─────────────────────────────────────────────────────┐
│           GardenLog.Mcp Service                     │
│  ┌───────────────────────────────────────────────┐  │
│  │  MCP Endpoint (POST /mcp)                     │  │
│  │  - Tool Discovery                             │  │
│  │  - Tool Invocation                            │  │
│  │  - Auth Validation (Auth0 JWT)                │  │
│  └───────────────┬───────────────────────────────┘  │
│                  │                                   │
│  ┌───────────────┴───────────────────────────────┐  │
│  │  MCP Tools Layer                              │  │
│  │  - get_plant_history                          │  │
│  │  - get_garden_bed_timeline                    │  │
│  │  - search_harvests                            │  │
│  │  - get_plant_statistics                       │  │
│  │  - get_plant_info, search_plants              │  │
│  │  - get_garden_info, get_weather_history       │  │
│  └───────┬─────────────────────────┬─────────────┘  │
│          │                         │                 │
│  ┌───────┴──────────┐    ┌────────┴──────────────┐  │
│  │ Query Services   │    │ API Clients (Phase 2) │  │
│  │ - PlantHarvest   │    │ - PlantHarvest API    │  │
│  │ - PlantCatalog   │    │ - PlantCatalog API    │  │
│  │ - Garden/Weather │    │ - UserManagement API  │  │
│  └────────┬─────────┘    └───────────────────────┘  │
└───────────┼─────────────────────────────────────────┘
            │                         │
            ↓                         ↓
    ┌──────────────┐       ┌──────────────────┐
    │   MongoDB    │       │  Production APIs  │
    │  (Read-Only) │       │  (Write + Logic)  │
    └──────────────┘       └──────────────────┘
```

### Why Centralized vs Embedded?

| Aspect | Embedded (in each API) | Centralized (GardenLog.Mcp) |
|--------|------------------------|------------------------------|
| Code location | MCP tools in every API project | Single dedicated service |
| Deployment | Change 1 tool = deploy 4+ services | Single deployment |
| Production APIs | Bloated with agent code | Stay clean and focused |
| Query optimization | Limited to domain boundaries | Cross-domain analytics |
| Agent UX | Multiple MCP connections | Single connection, unified namespace |
| Data access | Via API only | Direct MongoDB reads for analytics |
| **Decision** | ❌ Not selected | ✅ **Selected** |

## Data Model & Collections

### MongoDB Collections Accessed

| Collection | Domain | Purpose | User Filtered |
|------------|--------|---------|---------------|
| `HarvestCycle-Collection` | PlantHarvest | Harvest season records | Yes |
| `PlantHarvestCycle-Collection` | PlantHarvest | Per-plant tracking in harvest | Yes |
| `GardenBedUsage-Collection` | PlantHarvest | Garden bed allocation history | Yes |
| `WorkLog-Collection` | PlantHarvest | Activity logs and notes | Yes |
| `Plant-Collection` | PlantCatalog | Plant species catalog | No (public) |
| `PlantVariety-Collection` | PlantCatalog | Plant varieties | No (public) |
| `Garden-Collection` | UserManagement | User gardens and beds | Yes |
| `Weather-Collection` | GrowConditions | Historical weather data | Yes |
| `ImageCatalog-Collection` | ImageCatalog | Images linked to entities | Yes |

### Key Data Entities

**HarvestCycle** - Top-level aggregate for a growing season
- `Id`, `UserProfileId`, `HarvestYear`, `StartDate`, `EndDate`
- Related gardens, planting notes
- Contains multiple PlantHarvestCycles

**PlantHarvestCycle** - Individual plant in a harvest
- `PlantId`, `PlantVarietyId`, `PlantGrowthInstructionId`
- **Dates:** `SeedingDate`, `GerminationDate`, `TransplantDate`, `FirstHarvestDate`, `LastHarvestDate`
- **Metrics:** `GerminationRate`, `TotalWeightInPounds`, `TotalItems`, `NumberOfSeeds`, `NumberOfTransplants`
- Planting method, spacing, seed company
- GardenBedLayout (spatial data)
- `PlantCalendar` array with scheduled tasks (StartDate, EndDate, TaskType, CompletedDateTime)
  - **CRITICAL:** PlantCalendar is the source of truth for task schedules
  - **GENERATION:** PlantCalendar schedules are auto-generated from PlantGrowthInstruction templates + Garden weather dates
    - GrowInstructions define timing relative to weather: `BeforeLastFrost`, `AfterLastFrost`, `WarmSoil`, `MidSummer`
    - System looks up Garden-Collection for the HarvestCycle's garden to get `LastFrostDate`, `FirstFrostDate`, `WarmSoilDate`
    - Calculates actual dates: GrowInstruction says "4 weeks before last frost" + Garden.LastFrostDate (May 15) = StartDate (April 17)
    - See: `ScheduleBuilder.GeneratePlantCalendarBasedOnGrowInstruction()` in PlantHarvest.Api
  - Task generators create PlantTask records based on PlantCalendar schedules
  - When PlantCalendar schedules are updated (via UpdatePlantScheduleCommand), the system triggers `PlantScheduleUpdated` events
  - Task generators listen to these events and sync PlantTask records accordingly
  - See: `IndoorSawTaskGenerator`, `TransplantOutsideTaskGenerator`, etc. in PlantHarvest.Api/EventHandlers/Tasks/
  - The sync is bi-directional: updating task dates requires updating PlantCalendar schedules to maintain consistency

**PlantTask** - Planned tasks (derived from PlantCalendar)
- `Title`, `Type` (SowIndoors, TransplantOutside, Harvest, etc.)
- `TargetDateStart`, `TargetDateEnd`, `CompletedDateTime`
- Links to HarvestCycleId, PlantHarvestCycleId, PlantScheduleId
- `IsSystemGenerated` flag
- **NOTE:** PlantTask records are generated from and must stay synchronized with PlantCalendar schedules

**WorkLog** - Completed activities (AUTHORITATIVE SOURCE for actual dates)
- `Log` (text description), `EventDateTime` (when activity happened), `EnteredDateTime`
- `Reason` (SowIndoors, TransplantOutside, Harvest, Watering, Fertilizing, etc.)
- `RelatedEntities` array linking to HarvestCycle, PlantHarvestCycle, Plant, Garden
- **Primary source for "what actually happened when"**

**GardenBedPlantHarvestCycle** - Bed usage record
- Which plants were in which beds
- Start/end dates, location (X, Y, Length, Width)
- Number of plants

**Garden** - User's physical garden location
- Name, location (city, state, lat/long), dimensions
- **Weather Dates (CRITICAL for PlantCalendar generation):**
  - `LastFrostDate` - Average last spring frost date for the location
  - `FirstFrostDate` - Average first fall frost date for the location
  - `WarmSoilDate` - Date when soil reaches planting temperature
- `GardenBeds` array with bed layout (Name, Type, Length, Width, X, Y coordinates)
- **Purpose:** Provides climate context to calculate actual planting dates from GrowInstruction templates

**Plant** - Catalog plant type
- Name, description, lifecycle, type (vegetable/herb/flower)
- Environmental needs (MoistureRequirement, LightRequirement, GrowTolerance)
- Days to maturity, harvest seasons, seed viability years
- Contains PlantGrowInstructions and PlantVarieties

**PlantGrowthInstruction** - Growing method templates (embedded in Plant)
- `Name`, `PlantingMethod` (SeedIndoors, DirectSeed, Transplant)
- **Weather-Relative Timing:**
  - `StartSeedAheadOfWeatherCondition` (BeforeLastFrost, AfterLastFrost, WarmSoil, MidSummer)
  - `StartSeedWeeksAheadOfWeatherCondition` (offset in weeks)
  - `TransplantAheadOfWeatherCondition`, `TransplantWeeksAheadOfWeatherCondition`
- Detailed instructions: `StartSeedInstructions`, `TransplantInstructions`, `GrowingInstructions`, `HarvestInstructions`
- Fertilizer schedules, planting depth, spacing, days to sprout
- **Used to generate PlantCalendar schedules** when plant is added to harvest cycle

### Example MongoDB Documents

**HarvestCycle Example:**
```json
{
  "_id": "16edb239-d239-4bb5-a902-377a66364387",
  "HarvestCycleName": "Homestead 2023 Plan",
  "StartDate": { "$date": "2023-02-02T06:00:00.000Z" },
  "EndDate": { "$date": "2023-10-20T00:00:00.000Z" },
  "Notes": "Second year on homestead",
  "UserProfileId": "auth0|up1",
  "GardenId": "39131fc5-61f5-43e2-9243-a537a75487b1"
}
```

**PlantHarvestCycle Example (Leeks):**
```json
{
  "_id": "8df83545-8ae7-4de3-8493-7ff746fe3197",
  "PlantCalendar": [
    {
      "_id": "7e20f372-ac9f-4c26-8218-b9f4cb71d350",
      "StartDate": { "$date": "2023-01-30T00:00:00.000Z" },
      "EndDate": { "$date": "2023-02-06T00:00:00.000Z" },
      "TaskType": "SowIndoors",
      "Notes": "Desired number of plants: 15. Sow in flats 1/4\" apart, 1/4\" deep.",
      "IsSystemGenerated": true
    },
    {
      "_id": "1c174770-1068-47eb-a4c3-5b51d369779e",
      "StartDate": { "$date": "2023-04-17T00:00:00.000Z" },
      "TaskType": "TransplantOutside",
      "Notes": "Transplant when daytime temperatures are at least 45°F."
    },
    {
      "_id": "4c6a892a-3927-4d14-84c6-472ce8e07aca",
      "StartDate": { "$date": "2023-08-15T00:00:00.000Z" },
      "TaskType": "Harvest",
      "Notes": "When plants reach desired size, loosen with spading fork."
    }
  ],
  "HarvestCycleId": "16edb239-d239-4bb5-a902-377a66364387",
  "PlantId": "a461feac-a128-4b56-ab35-89ef71264107",
  "PlantName": "Leeks",
  "PlantVarietyId": "57c30df2-418c-4c6f-9bdd-4b73b1d7a2ac",
  "PlantVarietyName": "Bandit",
  "PlantGrowthInstructionId": "b295d016-35ed-4650-9e62-ab8b5791c4a9",
  "PlantGrowthInstructionName": "February Seeding for 120 days",
  "PlantingMethod": "SeedIndoors",
  "NumberOfSeeds": 108,
  "SeedingDate": { "$date": "2023-02-05T00:00:00.000Z" },
  "GerminationDate": { "$date": "2023-02-10T00:00:00.000Z" },
  "GerminationRate": "100",
  "NumberOfTransplants": 72,
  "TransplantDate": { "$date": "2023-04-27T23:59:48.520Z" },
  "FirstHarvestDate": { "$date": "2023-09-17T04:05:53.734Z" },
  "LastHarvestDate": { "$date": "2023-10-15T00:00:00.000Z" },
  "TotalWeightInPounds": "15",
  "Notes": "Lots of great looking leeks",
  "DesiredNumberOfPlants": 15,
  "SpacingInInches": 5
}
```

**PlantTask Example:**
```json
{
  "_id": "e9562189-d0e6-4632-bbd6-f98f9f5f4bb3",
  "Title": "Sow Seeds Indoors",
  "Type": "SowIndoors",
  "CreatedDateTime": { "$date": "2023-02-04T11:04:57.438Z" },
  "TargetDateStart": { "$date": "2023-01-30T00:00:00.000Z" },
  "TargetDateEnd": { "$date": "2023-02-06T00:00:00.000Z" },
  "CompletedDateTime": { "$date": "2023-02-05T00:00:00.000Z" },
  "HarvestCycleId": "16edb239-d239-4bb5-a902-377a66364387",
  "PlantHarvestCycleId": "2e3c9beb-8d42-4e3f-8051-2221ffaa881b",
  "PlantName": "Leeks - Bulgarian Giant Leek",
  "PlantScheduleId": "ec0d39e4-af75-46b4-98a7-97f59fdaa7eb",
  "Notes": "Sow in flats in Feb.-March, 1/4\" apart.",
  "IsSystemGenerated": true,
  "UserProfileId": "auth0|up1"
}
```

**Garden Example (showing weather dates used for PlantCalendar generation):**
```json
{
  "_id": "39131fc5-61f5-43e2-9243-a537a75487b1",
  "Name": "Steve's Garden",
  "City": "Minnetrista",
  "StateCode": "MN",
  "Latitude": "44.97096",
  "Longitude": "-93.66813",
  "Notes": "Homestead Garden",
  "UserProfileId": "auth0|up1",
  "LastFrostDate": { "$date": "2023-05-15T00:00:00.000Z" },
  "FirstFrostDate": { "$date": "2023-09-18T00:00:00.000Z" },
  "WarmSoilDate": { "$date": "2023-06-07T00:00:00.000Z" },
  "Length": 100,
  "Width": 100,
  "GardenBeds": [
    {
      "_id": "06662568-8957-493d-bb72-488a7db71cf0",
      "Name": "In Ground Right 1",
      "RowNumber": 1,
      "Length": 120,
      "Width": 30,
      "X": 234,
      "Y": 45,
      "Notes": "Connected to in-ground left drip irrigation",
      "Type": "InGroundBed",
      "Rotate": 0
    },
    {
      "_id": "319a1f16-da19-4e3f-9919-f283d52f6820",
      "Name": "In Ground Right 2",
      "RowNumber": 2,
      "Length": 300,
      "Width": 30,
      "Notes": "Connected to in-ground left drip line",
      "Type": "InGroundBed"
    }
  ]
}
```

**WorkLog Example (Completed Activity):**
```json
{
  "_id": "09971be8-df30-4556-b68f-7ed11376dd65",
  "Log": "108 seeds of Leeks were planted indoors on 02/05/2023",
  "EnteredDateTime": { "$date": "2023-02-06T06:25:14.740Z" },
  "EventDateTime": { "$date": "2023-02-05T00:00:00.000Z" },
  "Reason": "SowIndoors",
  "UserProfileId": "auth0|up1",
  "RelatedEntities": [
    {
      "EntityType": "HarvestCycle",
      "EntityId": "16edb239-d239-4bb5-a902-377a66364387",
      "EntityName": "Homestead 2023 Plan"
    },
    {
      "EntityType": "PlantHarvestCycle",
      "EntityId": "8df83545-8ae7-4de3-8493-7ff746fe3197",
      "EntityName": "Leeks - Bandit"
    }
  ]
}
```

**Plant with GrowInstructions Example (Cauliflower):**
```json
{
  "_id": "facc42f4-d20c-4f72-846f-dc7b2e812f0e",
  "Name": "Cauliflower",
  "Description": "Some varieties are good for spring planting, most types best as mid-summer planting for fall harvest.",
  "Color": "#eae9e1",
  "Lifecycle": "Cool",
  "Type": "Vegetable",
  "MoistureRequirement": "ConsistentMoisture",
  "LightRequirement": "PartShade",
  "HarvestSeason": ["EarlySummer"],
  "SeedViableForYears": 3,
  "GrowInstructions": [
    {
      "_id": "9839a4b7-f414-4e32-99d3-768d541a543b",
      "Name": "Start Indoors",
      "PlantingMethod": "SeedIndoors",
      "PlantingDepthInInches": "Depth4th",
      "SpacingInInches": 18,
      "StartSeedAheadOfWeatherCondition": "BeforeLastFrost",
      "StartSeedWeeksAheadOfWeatherCondition": 4,
      "TransplantAheadOfWeatherCondition": "BeforeLastFrost",
      "TransplantWeeksAheadOfWeatherCondition": 0,
      "HarvestSeason": "Summer",
      "DaysToSproutMin": 7,
      "DaysToSproutMax": 14,
      "FertilizerForSeedlings": "Starter",
      "FertilizerFrequencyForSeedlingsInWeeks": 2,
      "StartSeedInstructions": "For spring-planted cauliflower, start seeds indoors in April. Apply fertilizer beginning when first true leaf appears.",
      "TransplantInstructions": "Transplant outdoors when seedlings are 4–5 weeks old. Water in with liquid starter solution.",
      "HarvestInstructions": "Harvest when heads reach usable size, before flower buds open."
    },
    {
      "_id": "ea38fed0-84e9-428c-897b-7fa037578eb1",
      "Name": "Direct seed in July",
      "PlantingMethod": "DirectSeed",
      "StartSeedAheadOfWeatherCondition": "MidSummer",
      "StartSeedWeeksAheadOfWeatherCondition": 0,
      "HarvestSeason": "Fall",
      "DaysToSproutMin": 7,
      "DaysToSproutMax": 10,
      "FertilizerAtPlanting": "Compost",
      "Fertilizer": "AllPurpose",
      "FertilizeFrequencyInWeeks": 4
    }
  ]
}
```

**Note:** When a plant is added to a harvest cycle with a specific GrowInstructionId, the system:
1. Looks up the **GrowInstruction** template from the Plant catalog (e.g., "Start seeds 4 weeks before last frost")
2. Gets the **Garden's weather dates** from Garden-Collection (LastFrostDate, FirstFrostDate, WarmSoilDate)
3. Calculates actual calendar dates: `GrowInstruction.StartSeedWeeksAheadOfWeatherCondition` + `Garden.LastFrostDate` = StartDate
   - Example: 4 weeks before LastFrostDate (May 15) = April 17
4. Generates **PlantCalendar** schedules with TaskType, StartDate, EndDate on PlantHarvestCycle
5. Task generators then create **PlantTask** records from these PlantCalendar schedules
6. Users complete tasks and log actual activities in **WorkLog** with EventDateTime

This flow connects: **GrowInstruction (template)** → **Garden (climate dates)** → **PlantCalendar (calculated schedule)** → **PlantTask (task records)** → **WorkLog (actual completion)**

## Security Architecture

### Authentication Flow

```
1. User authenticates with Auth0 (existing web app login)
2. User obtains JWT access token
3. User configures agent (GitHub Copilot) with token
4. Agent includes token in MCP requests: Authorization: Bearer <token>
5. GardenLog.Mcp validates token with Auth0
6. Extract UserProfileId from token claims
7. Execute query with user context
```

### Development vs Production Authentication

**Development Mode** (appsettings.Development.json)
```json
{
  "Authentication": {
    "Mode": "Development",
    "DevToken": "dev-test-token-12345"
  }
}
```
- Simple bearer token or anonymous access
- Fast local iteration
- No external Auth0 dependency

**Production Mode** (appsettings.json)
```json
{
  "Authentication": {
    "Mode": "Production",
    "Auth0": {
      "Domain": "https://gardenlog.auth0.com",
      "Audience": "gardenlog-mcp-api"
    }
  }
}
```
- Full JWT validation with Auth0
- Token signature and expiration checks
- Proper claim extraction

### Authorization & Data Isolation

**Every query MUST be filtered by UserProfileId:**

```csharp
// Example from PlantHarvestQueryService
public async Task<PlantHistory> GetPlantHistoryAsync(
    string plantId, 
    string userId,  // ← Extracted from JWT
    DateRange? dateRange = null)
{
    var filter = Builders<PlantHarvestCycle>.Filter.And(
        Builders<PlantHarvestCycle>.Filter.Eq(p => p.PlantId, plantId),
        Builders<PlantHarvestCycle>.Filter.Eq(p => p.UserProfileId, userId) // ← Always enforced
    );
    
    if (dateRange != null)
    {
        filter = Builders<PlantHarvestCycle>.Filter.And(filter,
            Builders<PlantHarvestCycle>.Filter.Gte(p => p.SeedingDate, dateRange.Start),
            Builders<PlantHarvestCycle>.Filter.Lte(p => p.SeedingDate, dateRange.End)
        );
    }
    
    var results = await _collection.Find(filter).ToListAsync();
    // ... aggregate and return
}
```

### Personal Access Token (PAT) Flow - Phase 3

1. User navigates to Settings in GardenLogWeb
2. Clicks "Generate Agent Token"
3. System creates JWT with long expiration (30-90 days)
4. User copies token to agent configuration
5. Token stored securely in agent's config file
6. User can revoke token anytime

### Security Measures Summary

| Measure | Implementation |
|---------|----------------|
| Authentication | Auth0 JWT Bearer tokens |
| Dev Mode | Simple token or anonymous (local only) |
| Prod Mode | Full JWT validation |
| Authorization | UserProfileId filtering on all queries |
| Data Isolation | Row-level security via code enforcement |
| MongoDB Access | Read-only database user credentials |
| Audit Logging | Log all tool invocations (user, tool, params, timestamp) |
| Rate Limiting | Per-user limits to prevent abuse |
| Network | Private endpoints (Azure VNET) in production |
| Secrets | Azure Key Vault for connection strings |

## MCP Protocol Integration

### What is MCP?

Model Context Protocol (MCP) is a standard for connecting AI agents to external tools and data sources. It defines:
- JSON-RPC 2.0 based communication
- Tool discovery mechanism
- Structured tool invocation with typed parameters
- Standardized error handling

### ModelContextProtocol Package Architecture

We're using the official **ModelContextProtocol** C# SDK (v0.8.0-preview.1) which provides:

**Three main packages:**
1. **ModelContextProtocol** - Core hosting and dependency injection. Main package for most projects.
2. **ModelContextProtocol.AspNetCore** - HTTP transport for web-based MCP servers (what we need for external agents)
3. **ModelContextProtocol.Core** - Low-level APIs for minimal dependency scenarios

**How it works:**
- **Attribute-based tool registration**: Mark classes with `[McpServerToolType]` and methods with `[McpServerTool]`
- **Automatic protocol handling**: The framework handles all JSON-RPC communication, tool discovery, and invocation
- **Dependency injection**: Tools can inject services (IPlantHarvestQueryService, IUserContextAccessor, etc.) via constructor
- **Microsoft.Extensions.Hosting integration**: Uses the .NET Generic Host pattern
- **Transport abstraction**: Supports Stdio (for local CLI tools) and HTTP (via AspNetCore package) transports

**Key differences from manual implementation:**
- ❌ No manual JSON-RPC parsing/handling needed
- ❌ No custom controller or endpoint routing required
- ❌ No manual tool registration in DI container
- ✅ Automatic tool discovery from assemblies
- ✅ Strongly-typed tool parameters and return types
- ✅ Built-in error handling and MCP error codes
- ✅ Logging to stderr per MCP specification

**Example tool definition:**
```csharp
[McpServerToolType]
public class MyTool
{
    private readonly IMyService _service;
    
    public MyTool(IMyService service) => _service = service;
    
    [McpServerTool(Name = "my_tool")]
    [Description("Tool description shown to agents")]
    public async Task<MyResult> ExecuteAsync(
        [Description("Parameter description")] string param,
        CancellationToken cancellationToken)
    {
        return await _service.DoWorkAsync(param, cancellationToken);
    }
}
```

The framework automatically:
- Generates JSON schema from method signature
- Handles tool discovery requests (tools/list)
- Routes tool invocations (tools/call) to the correct method
- Serializes/deserializes parameters and results
- Returns proper MCP error responses for exceptions

### Tool Definition Structure (Auto-Generated)

The ModelContextProtocol package automatically generates tool definitions from method signatures. For example, this C# method:

```csharp
[McpServerTool(Name = "example_tool")]
[Description("Tool description that helps agent understand when to use this")]
public async Task<YourViewModel> ExecuteAsync(
    [Description("Required parameter description")] string requiredParam,
    [Description("Optional parameter description")] string? optionalParam = null,
    [Description("Optional date parameter")] DateTime? dateParam = null,
    CancellationToken cancellationToken = default)
```

Becomes this JSON tool definition (generated automatically):

```json
{
  "name": "example_tool",
  "description": "Tool description that helps agent understand when to use this",
  "inputSchema": {
    "type": "object",
    "properties": {
      "requiredParam": {
        "type": "string",
        "description": "Required parameter description"
      },
      "optionalParam": {
        "type": ["string", "null"],
        "description": "Optional parameter description"
      },
      "dateParam": {
        "type": ["string", "null"],
        "format": "date-time",
        "description": "Optional date parameter"
      }
    },
    "required": ["requiredParam"]
  }
}
```

**You don't write JSON schemas manually** - they're inferred from your C# method signatures!

### Example Tool Invocation Flow

1. **Agent interprets user query:** Example: "When do I typically plant tomatoes?"

2. **Agent discovers tools:** Sends MCP request with method "tools/list"
   - ModelContextProtocol framework automatically responds with all `[McpServerTool]` methods

3. **Agent selects appropriate tool** based on tool descriptions

4. **Agent calls tool:** Sends MCP request
```json
{
  "jsonrpc": "2.0",
  "method": "tools/call",
  "params": {
    "name": "your_tool_name",
    "arguments": {
      "parameter1": "value1",
      "parameter2": "value2"
    }
  },
  "id": 1
}
```

5. **ModelContextProtocol framework processes (automatically):**
   - Parses JSON-RPC request
   - Finds method with matching `[McpServerTool(Name = "your_tool_name")]`
   - Deserializes arguments to method parameters
   - Invokes method with dependency-injected services
   - Your tool code executes:
     - IUserContextAccessor extracts userId from authenticated request
     - Query service retrieves/processes data (via API or MongoDB)
     - Returns Contract ViewModel
   - Framework serializes return value

6. **Returns response (automatically formatted by framework):**
```json
{
  "jsonrpc": "2.0",
  "result": {
    // Your returned ViewModel serialized as JSON
  },
  "id": 1
}
```

7. **Agent formats for user** in natural language

**Key point:** Steps 2, 5, and 6 are handled entirely by the ModelContextProtocol framework. You only write the business logic in step 5.

## Phase 1: Implementation Plan (MVP - Read-Only Tools)

### Step 1: Project Structure Setup

Create new solution folder: `src/GardenLog.Mcp/`

```
src/GardenLog.Mcp/
├── GardenLog.Mcp.Api/
│   ├── Program.cs (MCP server setup with ModelContextProtocol package)
│   ├── GardenLog.Mcp.Api.csproj
│   ├── appsettings.json
│   └── appsettings.Development.json
├── GardenLog.Mcp.Application/
│   ├── GardenLog.Mcp.Application.csproj
│   ├── Services/
│   │   ├── IPlantHarvestQueryService.cs
│   │   ├── PlantHarvestQueryService.cs
│   │   ├── IPlantCatalogQueryService.cs
│   │   ├── PlantCatalogQueryService.cs
│   │   ├── IGardenQueryService.cs
│   │   ├── GardenQueryService.cs
│   │   ├── IWeatherQueryService.cs
│   │   ├── WeatherQueryService.cs
│   │   ├── IUserContextAccessor.cs
│   │   └── UserContextAccessor.cs
│   └── Tools/ (MCP tools using [McpServerTool] attributes)
│       ├── PlantHarvest/
│       │   ├── GetPlantHistoryTool.cs
│       │   ├── GetGardenBedTimelineTool.cs
│       │   ├── SearchHarvestCyclesTool.cs
│       │   └── GetPlantStatisticsTool.cs
│       ├── PlantCatalog/
│       │   ├── GetPlantInfoTool.cs
│       │   └── SearchPlantsTool.cs
│       ├── Garden/
│       │   ├── GetGardenInfoTool.cs
│       │   └── ListGardenBedsTool.cs
│       └── Weather/
│           └── GetWeatherHistoryTool.cs
└── GardenLog.Mcp.Infrastructure/
    ├── GardenLog.Mcp.Infrastructure.csproj
    ├── MongoDB/
    │   ├── MongoDbContext.cs
    │   ├── Collections/
    │   │   ├── HarvestCycleCollection.cs
    │   │   ├── PlantCollection.cs
    │   │   ├── GardenCollection.cs
    │   │   └── WeatherCollection.cs
    │   └── Configuration/
    │       └── MongoDbSettings.cs
    └── Authentication/
        ├── Auth0Settings.cs
        ├── DevAuthHandler.cs
        └── DevAuthOptions.cs
```

**Project References:**
- Reference existing `*.Contract` projects to reuse DTOs/ViewModels:
  - `PlantHarvest.Contract` - Reuse PlantHarvestCycleViewModel, HarvestCycleViewModel, etc.
  - `PlantCatalog.Contract` - Reuse PlantViewModel, PlantVarietyViewModel, etc.
  - `UserManagement.Contract` - Reuse GardenViewModel, GardenBedViewModel, etc.
  - `GrowConditions.Contract` - Reuse WeatherViewModel, etc.

**Architecture Decision: Reuse Existing Contract Models**

Instead of creating new models in GardenLog.Mcp, **reuse the existing Contract models** from API projects:

**Benefits:**
1. **Consistency**: Same data shape whether querying via API or directly from MongoDB
2. **No duplication**: Don't maintain two sets of similar models
3. **API compatibility**: When we call APIs in Phase 2, models already match
4. **Single source of truth**: Contract models are the canonical representation

**Implementation:**
- Query services map MongoDB entities → Contract ViewModels
- MCP tools return Contract ViewModels
- For simple queries that APIs already support → call API and return result
- For complex analytical queries → query MongoDB directly, map to Contract ViewModels
- Either path returns the same model type

**Dependencies:**
- MongoDB.Driver (for data access)
- Microsoft.AspNetCore.Authentication.JwtBearer (for Auth0 JWT validation)
- ModelContextProtocol (v0.8.0-preview.1) - Core MCP functionality
- ModelContextProtocol.AspNetCore (v0.8.0-preview.1) - HTTP transport for external agents
- Microsoft.Extensions.Hosting (for hosting infrastructure)

### Step 2: MongoDB Configuration

**appsettings.json:**
```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "GardenLogDb",
    "Collections": {
      "HarvestCycles": "HarvestCycle-Collection",
      "PlantHarvestCycles": "PlantHarvestCycle-Collection",
      "GardenBedUsage": "GardenBedUsage-Collection",
      "WorkLogs": "WorkLog-Collection",
      "Plants": "Plant-Collection",
      "PlantVarieties": "PlantVariety-Collection",
      "Gardens": "Garden-Collection",
      "Weather": "Weather-Collection"
    }
  }
}
```

**MongoDB User (Production):**
```javascript
db.createUser({
  user: "gardenlog_mcp_readonly",
  pwd: "<secure-password>",
  roles: [
    { role: "read", db: "GardenLogDb" }
  ]
})
```

**API URL Configuration:**

API URLs are configured in Program.cs based on environment:

- **Production**: Azure Container Apps URLs (default)
- **Development**: Also defaults to Azure URLs, but can be uncommented to use localhost URLs if all APIs are running locally

This matches the pattern in GardenLogWeb where production URLs are the default for reliability, with optional local override for full-stack integration testing.

### Step 3: MCP Server Setup with ModelContextProtocol Package

**Program.cs:**
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging to stderr (MCP requirement)
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Authentication
var authMode = builder.Configuration["Authentication:Mode"];
if (authMode == "Development")
{
    builder.Services.AddAuthentication("DevAuth")
        .AddScheme<DevAuthOptions, DevAuthHandler>("DevAuth", options => 
        {
            options.DevToken = builder.Configuration["Authentication:DevToken"];
        });
}
else
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["Authentication:Auth0:Domain"];
            options.Audience = builder.Configuration["Authentication:Auth0:Audience"];
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        });
}

builder.Services.AddAuthorization();

// Register MongoDB data services (for complex analytical tools)
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IPlantHarvestQueryService, PlantHarvestQueryService>();
builder.Services.AddScoped<IUserContextAccessor, UserContextAccessor>();

// Configure API URLs based on environment
string plantCatalogUrl;
string plantHarvestUrl;
string userManagementUrl;
string growConditionsUrl;

if (builder.Environment.IsProduction())
{
    // Production: Azure Container Apps
    plantCatalogUrl = "https://plantcatalogapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";
    plantHarvestUrl = "https://plantharvestapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";
    userManagementUrl = "https://usermanagementapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";
    growConditionsUrl = "https://growconditionsapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";
}
else
{
    // Development: Can override with local URLs for integration testing
    // plantCatalogUrl = "https://localhost:5051";
    // plantHarvestUrl = "http://localhost:5049";
    // userManagementUrl = "http://localhost:5212";
    // growConditionsUrl = "http://localhost:5XXX";
    
    // Default to production URLs even in development (same as GardenLogWeb pattern)
    plantCatalogUrl = "https://plantcatalogapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";
    plantHarvestUrl = "https://plantharvestapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";
    userManagementUrl = "https://usermanagementapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";
    growConditionsUrl = "https://growconditionsapi-containerapp.politecoast-efa2ff8d.eastus.azurecontainerapps.io";
}

// Register HTTP clients to existing APIs (for simple wrapper tools)
builder.Services.AddHttpClient("PlantCatalogApi", client =>
{
    client.BaseAddress = new Uri(plantCatalogUrl);
}).AddHttpMessageHandler<UserAuthenticationHandler>();

builder.Services.AddHttpClient("PlantHarvestApi", client =>
{
    client.BaseAddress = new Uri(plantHarvestUrl);
}).AddHttpMessageHandler<UserAuthenticationHandler>();

builder.Services.AddHttpClient("UserManagementApi", client =>
{
    client.BaseAddress = new Uri(userManagementUrl);
}).AddHttpMessageHandler<UserAuthenticationHandler>();

builder.Services.AddHttpClient("GrowConditionsApi", client =>
{
    client.BaseAddress = new Uri(growConditionsUrl);
}).AddHttpMessageHandler<UserAuthenticationHandler>();

// Configure MCP Server with HTTP transport
builder.Services
    .AddMcpServer()
    .WithHttpServerTransport() // Use HTTP transport for external agents
    .WithToolsFromAssembly(); // Auto-discover all [McpServerTool] attributed methods

await builder.Build().RunAsync();
```

**Note:** The ModelContextProtocol package handles all the MCP protocol details internally. Tools are registered via attributes rather than explicit registration.

**UserContextMiddleware.cs:**

**Note:** With ModelContextProtocol.AspNetCore, middleware for extracting user context is likely integrated by the framework. If needed, you can use the IUserContextAccessor service pattern shown earlier. The exact middleware implementation depends on how ModelContextProtocol.AspNetCore handles authentication.

### Step 4: Query Service Implementation

**General Pattern for Query Services:**

**Service Interface:**
```csharp
using YourDomain.Contract.ViewModels; // Reuse existing contract models

public interface IYourQueryService
{
    // Define methods that return existing Contract ViewModels
    Task<YourViewModel> GetDataAsync(string id, string userId, CancellationToken cancellationToken = default);
}
```

**Service Implementation:**
```csharp
using YourDomain.Contract.ViewModels;
using YourDomain.Domain.Aggregates;

public class YourQueryService : IYourQueryService
{
    private readonly IMongoCollection<YourEntity> _collection;
    private readonly IYourApiClient _apiClient; // For simple queries

    public async Task<YourViewModel> GetDataAsync(
        string id, 
        string userId, 
        CancellationToken cancellationToken = default)
    {
        // Option 1: If API already supports this query, use it
        // return await _apiClient.GetDataAsync(id, userId);
        
        // Option 2: For complex queries not in API, query MongoDB directly
        var filter = Builders<YourEntity>.Filter.And(
            Builders<YourEntity>.Filter.Eq(e => e.Id, id),
            Builders<YourEntity>.Filter.Eq(e => e.UserProfileId, userId)
        );

        var entity = await _collection
            .Find(filter)
            .FirstOrDefaultAsync(cancellationToken);

        // Map MongoDB entity to existing Contract ViewModel
        return entity != null ? MapToViewModel(entity) : null;
    }

    private YourViewModel MapToViewModel(YourEntity entity)
    {
        // Map to existing Contract ViewModel
        return new YourViewModel
        {
            // Map properties from domain entity to contract model
        };
    }
}
```

**Key Pattern: Hybrid Approach**
1. **Simple queries existing in API**: Call API and return Contract models
2. **Complex analytical queries not in API**: Query MongoDB, map to Contract models
3. **Either way**: Return same model types for consistency

**Services will be implemented based on specific tool requirements.**

### Step 5: MCP Tool Implementation with Attributes

**Tools are defined using the ModelContextProtocol attribute-based approach:**

**General Tool Pattern:**
```csharp
using System.ComponentModel;
using ModelContextProtocol.Server;
using GardenLog.Mcp.Application.Services;

namespace GardenLog.Mcp.Application.Tools;

[McpServerToolType]
public class ExampleTool
{
    private readonly IYourService _service;
    private readonly IUserContextAccessor _userContext;

    public ExampleTool(
        IYourService service,
        IUserContextAccessor userContext)
    {
        _service = service;
        _userContext = userContext;
    }

    [McpServerTool(Name = "tool_name")]
    [Description("Tool description that helps the agent understand when to use this tool.")]
    public async Task<YourContractViewModel> ExecuteAsync(
        [Description("Parameter description")] string requiredParam,
        [Description("Optional parameter description")] string? optionalParam = null,
        CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetUserId();
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User context not found");

        // Your business logic here
        var result = await _service.DoWorkAsync(requiredParam, userId, cancellationToken);
        
        return result; // Return Contract ViewModel
    }
}
```

**Key patterns with ModelContextProtocol package:**
- `[McpServerToolType]` marks the class as containing MCP tools
- `[McpServerTool(Name = "tool_name")]` marks the method as an MCP tool
- `[Description]` attributes provide descriptions for tools and parameters
- Parameters are strongly-typed (not Dictionary<string, object>)
- Dependencies are injected via constructor
- User context accessed via IUserContextAccessor service
- Return types are strongly-typed (not object)
- **Return existing Contract ViewModels** from API projects for consistency
- Exceptions are handled by the MCP framework

**Two Implementation Patterns:**

1. **MongoDB-Based Tools** (for complex analytics):
```csharp
[McpServerToolType]
public class GetWorkLogHistoryTool
{
    private readonly IMongoCollection<WorkLog> _collection;
    private readonly IUserContextAccessor _userContext;

    [McpServerTool(Name = "get_worklog_history")]
    [Description("Query completed activities with flexible filtering")]
    public async Task<IReadOnlyCollection<WorkLogViewModel>> ExecuteAsync(
        [Description("Start date filter")] DateTime? startDate = null,
        [Description("Reason filter")] string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetUserId();
        
        // Build MongoDB filter
        var filter = Builders<WorkLog>.Filter.Eq(w => w.UserProfileId, userId);
        if (startDate.HasValue)
            filter = Builders<WorkLog>.Filter.And(filter, 
                Builders<WorkLog>.Filter.Gte(w => w.EventDateTime, startDate.Value));
        
        var results = await _collection.Find(filter).ToListAsync(cancellationToken);
        return results.Select(MapToViewModel).ToList();
    }
}
```

2. **API-Calling Tools** (for simple wrappers):
```csharp
[McpServerToolType]
public class GetPlantDetailsTool
{
    private readonly HttpClient _httpClient; // IHttpClientFactory injected
    private readonly IUserContextAccessor _userContext;

    [McpServerTool(Name = "get_plant_details")]
    [Description("Get plant catalog information including grow instructions")]
    public async Task<PlantViewModel> ExecuteAsync(
        [Description("Plant ID")] string? plantId = null,
        [Description("Plant name alternative")] string? plantName = null,
        CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetUserId();
        
        // Add user context header
        using var request = new HttpRequestMessage(HttpMethod.Get, 
            $"/api/plant/{plantId}");
        request.Headers.Add("X-User-Id", userId);
        
        // Call existing PlantCatalog API
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        // Return Contract ViewModel directly from API
        return await response.Content.ReadFromJsonAsync<PlantViewModel>(cancellationToken);
    }
}
```

**Specific tools will be created based on actual use cases and questions.**

### Step 6: User Context Service (replaces MCP Protocol Handler)

**With the ModelContextProtocol package, you don't need a controller or protocol handler - the framework handles all MCP protocol communication automatically.**

Instead, create a service to provide user context to tools:

**IUserContextAccessor.cs:**
```csharp
namespace GardenLog.Mcp.Application.Services;

public interface IUserContextAccessor
{
    string? GetUserId();
    void SetUserId(string userId);
}

public class UserContextAccessor : IUserContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return httpContext.User.FindFirst("UserProfileId")?.Value
                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        return null;
    }

    public void SetUserId(string userId)
    {
        // Used for testing or manual context injection
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Items["UserProfileId"] = userId;
        }
    }
}
```

**Register in Program.cs:**
```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContextAccessor, UserContextAccessor>();
```

**How it works:**
1. ModelContextProtocol.AspNetCore integrates with ASP.NET Core middleware
2. Authentication middleware validates JWT tokens
3. IUserContextAccessor extracts user ID from HttpContext.User claims
4. Tools inject IUserContextAccessor to get current user
5. All MCP protocol handling (JSON-RPC, tool discovery, invocation) is automatic

### Step 7: MongoDB Indexes

Execute in MongoDB:

```javascript
// PlantHarvestCycle indexes
db.getCollection("PlantHarvestCycle-Collection").createIndex(
  { "UserProfileId": 1, "PlantId": 1, "SeedingDate": -1 }
);

db.getCollection("PlantHarvestCycle-Collection").createIndex(
  { "UserProfileId": 1, "SeedingDate": 1 }
);

// GardenBedUsage indexes
db.getCollection("GardenBedUsage-Collection").createIndex(
  { "UserProfileId": 1, "GardenBedId": 1, "StartDate": -1 }
);

// WorkLog indexes
db.getCollection("WorkLog-Collection").createIndex(
  { "UserProfileId": 1, "CreatedDateTime": -1 }
);

db.getCollection("WorkLog-Collection").createIndex(
  { "UserProfileId": 1, "RelatedEntities.EntityId": 1 }
);

// HarvestCycle indexes
db.getCollection("HarvestCycle-Collection").createIndex(
  { "UserProfileId": 1, "HarvestYear": -1 }
);
```

### Step 8: Local Development Configuration

**appsettings.Development.json:**
```json
{
  "Authentication": {
    "Mode": "Development",
    "DevToken": "dev-bearer-token-12345"
  },
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "GardenLogDb"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**GitHub Copilot MCP Configuration (.vscode/mcp-settings.json):**
```json
{
  "mcpServers": {
    "gardenlog": {
      "command": "http",
      "args": [
        "POST",
        "http://localhost:5100/mcp"
      ],
      "headers": {
        "Authorization": "Bearer dev-bearer-token-12345",
        "Content-Type": "application/json"
      }
    }
  }
}
```

**Local Testing:**
```bash
# Install packages
cd src/GardenLog.Mcp/GardenLog.Mcp.Api
dotnet add package ModelContextProtocol --prerelease
dotnet add package ModelContextProtocol.AspNetCore --prerelease

# Start MongoDB
docker run -d -p 27017:27017 --name mongodb mongo:latest

# Run GardenLog.Mcp
dotnet run

# The ModelContextProtocol package handles the MCP endpoint automatically
# Test using an MCP client (like GitHub Copilot) or the MCP inspector tool
# MCP protocol communication is handled by the framework

# For debugging, check the logs (logged to stderr per MCP spec)
# Tool calls and responses will be logged automatically
```

**Testing with MCP Inspector:**
```bash
# Install MCP Inspector (if available)
npm install -g @modelcontextprotocol/inspector

# Connect to your server (exact command depends on HTTP transport config)
mcp-inspector http://localhost:5100
```

## Phase 2: Write Operations via API Facade

### Goals
- Enable agents to perform write operations
- Maintain business logic in production APIs
- Provide transactional consistency

### Implementation Steps

1. **Add HTTP Clients for Production APIs**
```csharp
// Program.cs
builder.Services.AddHttpClient<IPlantHarvestApiClient, PlantHarvestApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:PlantHarvest"]);
})
.AddHttpMessageHandler<Auth0TokenHandler>();

builder.Services.AddHttpClient<IPlantCatalogApiClient, PlantCatalogApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:PlantCatalog"]);
})
.AddHttpMessageHandler<Auth0TokenHandler>();
```

2. **Create Write MCP Tools**

**CreateHarvestCycleTool.cs:**
```csharp
public class CreateHarvestCycleTool : IMcpTool
{
    private readonly IPlantHarvestApiClient _apiClient;

    public string Name => "create_harvest_cycle";
    
    public string Description => "Create a new harvest cycle (growing season) to track plants. Use when user wants to start planning or tracking a new garden season.";
    
    public object InputSchema => new
    {
        type = "object",
        properties = new
        {
            year = new { type = "integer", description = "Harvest year e.g. 2025" },
            gardenId = new { type = "string", description = "Garden ID for this cycle" },
            notes = new { type = "string", description = "Optional notes about the season" }
        },
        required = new[] { "year", "gardenId" }
    };

    public async Task<object> ExecuteAsync(Dictionary<string, object> arguments, UserContext userContext)
    {
        var year = Convert.ToInt32(arguments["year"]);
        var gardenId = arguments["gardenId"].ToString();
        var notes = arguments.GetValueOrDefault("notes")?.ToString();

        var request = new CreateHarvestCycleCommand
        {
            HarvestYear = year,
            GardenId = gardenId,
            Notes = notes,
            UserProfileId = userContext.UserId
        };

        // Call production API
        var result = await _apiClient.CreateHarvestCycleAsync(request, userContext.UserId);
        
        return new
        {
            success = true,
            harvestCycleId = result.HarvestCycleId,
            message = $"Created harvest cycle for {year}"
        };
    }
}
```

3. **Additional Write Tools**
- `add_plant_to_harvest`: Add plant to existing cycle
- `log_activity`: Create work log entry
- `update_harvest_metrics`: Record germination, transplant, harvest data
- `complete_harvest_cycle`: Mark season as complete

### API Client Implementation

```csharp
public interface IPlantHarvestApiClient
{
    Task<CreateHarvestCycleResponse> CreateHarvestCycleAsync(CreateHarvestCycleCommand command, string userId);
    Task<AddPlantResponse> AddPlantToHarvestAsync(string harvestId, AddPlantCommand command, string userId);
    Task LogActivityAsync(string harvestId, CreateWorkLogCommand command, string userId);
}

public class PlantHarvestApiClient : IPlantHarvestApiClient
{
    private readonly HttpClient _httpClient;

    public async Task<CreateHarvestCycleResponse> CreateHarvestCycleAsync(CreateHarvestCycleCommand command, string userId)
    {
        // Add user context header
        _httpClient.DefaultRequestHeaders.Add("X-User-Id", userId);
        
        var response = await _httpClient.PostAsJsonAsync("/api/HarvestCycle", command);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<CreateHarvestCycleResponse>();
    }
    
    // ... other methods
}
```

## Phase 3: Production Deployment

### Azure Container Apps Deployment

**Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["GardenLog.Mcp.Api/GardenLog.Mcp.Api.csproj", "GardenLog.Mcp.Api/"]
COPY ["GardenLog.Mcp.Application/GardenLog.Mcp.Application.csproj", "GardenLog.Mcp.Application/"]
COPY ["GardenLog.Mcp.Infrastructure/GardenLog.Mcp.Infrastructure.csproj", "GardenLog.Mcp.Infrastructure/"]
RUN dotnet restore "GardenLog.Mcp.Api/GardenLog.Mcp.Api.csproj"
COPY . .
WORKDIR "/src/GardenLog.Mcp.Api"
RUN dotnet build "GardenLog.Mcp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GardenLog.Mcp.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GardenLog.Mcp.Api.dll"]
```

**Azure Configuration:**
- Container Apps environment
- Managed identity for Key Vault access
- Private endpoint for MongoDB (Azure Cosmos DB for MongoDB)
- Application Insights integration
- Environment variables from Key Vault

### Personal Access Token Generation

Add to GardenLogWeb (Blazor app):

**Pages/Settings/AgentTokens.razor:**
```razor
@page "/settings/tokens"
@inject ITokenService TokenService
@inject IJSRuntime JS

<h3>AI Agent Access Tokens</h3>

<button class="btn btn-primary" @onclick="GenerateToken">Generate New Token</button>

@if (!string.IsNullOrEmpty(_newToken))
{
    <div class="alert alert-warning mt-3">
        <p><strong>Save this token - it won't be shown again!</strong></p>
        <div class="input-group">
            <input type="text" class="form-control" value="@_newToken" readonly />
            <button class="btn btn-secondary" @onclick="CopyTokenToClipboard">Copy</button>
        </div>
        @if (_copied)
        {
            <small class="text-success">✓ Copied to clipboard!</small>
        }
    </div>
}

<h4 class="mt-4">Active Tokens</h4>
<table class="table">
    <thead>
        <tr>
            <th>Name</th>
            <th>Created</th>
            <th>Last Used</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var token in _tokens)
        {
            <tr>
                <td>@token.Name</td>
                <td>@token.CreatedDate.ToShortDateString()</td>
                <td>@token.LastUsedDate?.ToShortDateString()</td>
                <td><button class="btn btn-sm btn-danger" @onclick="() => RevokeToken(token.Id)">Revoke</button></td>
            </tr>
        }
    </tbody>
</table>

@code {
    private string? _newToken;
    private List<TokenInfo> _tokens = new();
    private bool _copied = false;

    private async Task GenerateToken()
    {
        _newToken = await TokenService.GenerateAgentTokenAsync("GitHub Copilot Token");
        _copied = false;
        await LoadTokens();
    }

    private async Task RevokeToken(string tokenId)
    {
        await TokenService.RevokeTokenAsync(tokenId);
        _newToken = null;
        await LoadTokens();
    }

    private async Task CopyTokenToClipboard()
    {
        if (!string.IsNullOrEmpty(_newToken))
        {
            // Use Blazor JavaScript interop to copy to clipboard
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", _newToken);
            _copied = true;
            
            // Reset copied message after 3 seconds
            await Task.Delay(3000);
            _copied = false;
            StateHasChanged();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadTokens();
    }

    private async Task LoadTokens()
    {
        _tokens = await TokenService.GetUserTokensAsync();
    }
}
```

**Note:** Blazor WebAssembly can use `navigator.clipboard.writeText` via JavaScript interop. No custom JavaScript file needed - the Web Clipboard API is standard in modern browsers.

### Monitoring & Audit Logging

**AuditLogMiddleware.cs:**
```csharp
public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLogMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/mcp"))
        {
            var userContext = context.Items["UserContext"] as UserContext;
            var requestBody = await ReadRequestBodyAsync(context.Request);
            
            _logger.LogInformation(
                "MCP Tool Invocation: User={UserId}, Request={Request}",
                userContext?.UserId ?? "anonymous",
                requestBody
            );

            await _next(context);

            _logger.LogInformation(
                "MCP Tool Completed: User={UserId}, Status={StatusCode}",
                userContext?.UserId ?? "anonymous",
                context.Response.StatusCode
            );
        }
        else
        {
            await _next(context);
        }
    }
}
```

**Application Insights:**
- Track tool invocation frequency
- Monitor response times
- Alert on errors
- Analyze usage patterns

## Testing Strategy

### Unit Tests

**GetPlantHistoryToolTests.cs:**
```csharp
public class GetPlantHistoryToolTests
{
    [Fact]
    public async Task ExecuteAsync_WithPlantName_ReturnsHistory()
    {
        // Arrange
        var mockHarvestQuery = new Mock<IPlantHarvestQueryService>();
        var mockCatalogQuery = new Mock<IPlantCatalogQueryService>();
        
        mockCatalogQuery.Setup(x => x.FindPlantByNameAsync("tomato"))
            .ReturnsAsync(new Plant { Id = "plant_123", Name = "Tomato" });
        
        mockHarvestQuery.Setup(x => x.GetPlantHistoryAsync("plant_123", "user_1", null))
            .ReturnsAsync(new PlantHistory
            {
                PlantId = "plant_123",
                PlantName = "Tomato",
                TotalSeasons = 2
            });

        var tool = new GetPlantHistoryTool(mockHarvestQuery.Object, mockCatalogQuery.Object);
        var userContext = new UserContext { UserId = "user_1" };
        var arguments = new Dictionary<string, object> { ["plantName"] = "tomato" };

        // Act
        var result = await tool.ExecuteAsync(arguments, userContext);

        // Assert
        var history = result as PlantHistory;
        Assert.NotNull(history);
        Assert.Equal("Tomato", history.PlantName);
        Assert.Equal(2, history.TotalSeasons);
    }
}
```

### Integration Tests

**McpEndpointTests.cs:**
```csharp
public class McpEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    [Fact]
    public async Task Mcp_ToolsList_ReturnsAvailableTools()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "dev-bearer-token-12345");
        
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/list",
            id = 1
        };

        // Act
        var response = await client.PostAsJsonAsync("/mcp", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<McpResponse>();
        Assert.NotNull(content);
        Assert.NotEmpty(content.Result.Tools);
        Assert.Contains(content.Result.Tools, t => t.Name == "get_plant_history");
    }
}
```

### Manual Testing Checklist

- [ ] Tool discovery returns all expected tools
- [ ] Query with plant name resolves to correct plant ID
- [ ] Historical data filtered correctly by user
- [ ] Date range filtering works
- [ ] Statistics calculations are accurate
- [ ] Garden bed timeline shows correct occupancy
- [ ] Search harvest cycles with various filters
- [ ] Authentication rejects invalid tokens
- [ ] User cannot access other user's data
- [ ] MongoDB indexes improve query performance
- [ ] Error handling returns proper MCP error responses
- [ ] Audit logs capture tool invocations

## Tool Reference

### MCP Tool Design Principles

Based on analysis of existing APIs and internal agent requirements (see INTERNAL_AGENT_DESIGN.md), MCP tools should be:

1. **Flexible** - Support date ranges, multiple filters, various query patterns to avoid boxing in future use cases
2. **MongoDB-friendly** - Query MongoDB directly when existing APIs lack needed flexibility
3. **API-leveraging** - Use existing APIs where they provide sufficient functionality
4. **Agent-oriented** - Return rich data that enables AI reasoning (not just IDs)

### Existing API Analysis

**APIs with Good Coverage:**
- ✅ **GardenController.GetGarden(gardenId)** - Gets garden with beds and weather dates
- ✅ **HarvestController.GetGardenBedUsageHistory(gardenId, gardenBedId)** - Perfect for crop rotation validation
- ✅ **HarvestController.GetPlantHarvestCyclesByPlant(plantId)** - Historical plantings by plant (identity only)
- ✅ **PlantController.GetPlantById(id)** - Gets plant with GrowInstructions embedded

**API Gaps Requiring MongoDB Queries:**
- ❌ **WorkLogController.GetAllWorkLogs** - Only filters by single entity, no date range or reason filters
- ❌ No flexible PlantHarvestCycle search - can't filter by date range, metrics, multiple plants
- ❌ No PlantTask flexible search endpoint - no date range, reason, or completion status queries
- ❌ No cross-entity analytics - can't correlate WorkLog dates with PlantCalendar schedules

**Decision:** Build flexible MCP tools with MongoDB repositories to support agent reasoning, while allowing future APIs to replace them if enhanced.

---

## MCP Tools Specification

### Tool Categories

1. **Historical Data Query Tools** - Flexible queries for analyzing past seasons (MongoDB)
2. **Cross-Entity Correlation Tools** - Connect WorkLog, PlantCalendar, metrics for pattern detection (MongoDB)
3. **Garden Context Tools** - Gardens, beds, climate data (API calls)
4. **Plant Catalog Tools** - Plants, varieties, grow instructions (API calls)

**Implementation Pattern:**
- **Complex analytical tools** (requiring flexible filtering, cross-collection queries, or metrics not in APIs) query MongoDB directly
- **Simple catalog/context tools** (where existing APIs are sufficient) make HTTP calls to existing APIs
- All tools present a unified MCP interface to the agent - the agent doesn't need to know which backend is used

**Note:** Following the design philosophy of "raw data for agent reasoning", tools return comprehensive data rather than pre-calculated analysis. For example, crop rotation analysis is performed by the agent using raw bed history and plant family data, rather than a dedicated tool that calculates violations.

---

### 1. get_worklog_history

**Purpose:** Query completed activities with flexible filtering for date optimization analysis

**Parameters:**
- `startDate` (DateTime?, optional) - Filter activities from this date
- `endDate` (DateTime?, optional) - Filter activities up to this date
- `reason` (string?, optional) - WorkLogReason (SowIndoors, TransplantOutside, Harvest, etc.)
- `plantId` (string?, optional) - Filter by specific plant
- `harvestCycleId` (string?, optional) - Filter by specific harvest season
- `plantName` (string?, optional) - Alternative to plantId for natural language queries
- `limit` (int?, default: 100) - Max results to return

**Returns:** `IReadOnlyCollection<WorkLogViewModel>`
- Log text, EventDateTime, Reason, RelatedEntities

**Query Approach:**
- MongoDB query on WorkLog-Collection filtered by UserProfileId
- Apply date range, reason, and related entity filters
- If plantName provided, resolve to plantId first via PlantCatalog API
- Order by EventDateTime descending

**Enables:**
- "Show me all times we planted tomatoes indoors in the last 5 years"
- "When did we harvest peppers in 2023 vs 2024?"
- Date optimization analysis for internal agent

**Existing API:** ❌ Current WorkLogController.GetAllWorkLogs only filters by single entity

---

### 2. get_plant_harvest_cycles

**Purpose:** Flexible search across PlantHarvestCycle records with metrics for outcome analysis

**Parameters:**
- `plantId` (string?, optional) - Filter by plant
- `plantName` (string?, optional) - Alternative to plantId
- `harvestCycleId` (string?, optional) - Filter by harvest season
- `startDate` (DateTime?, optional) - Filter by SeedingDate or TransplantDate >= this date
- `endDate` (DateTime?, optional) - Filter by LastHarvestDate <= this date
- `minGerminationRate` (int?, optional) - Filter by GerminationRate >= this value
- `includeMetrics` (bool, default: true) - Include yield/germination data
- `includePlantCalendar` (bool, default: false) - Include embedded PlantCalendar schedules
- `includeGardenBeds` (bool, default: false) - Include bed assignments
- `limit` (int?, default: 50) - Max results

**Returns:** `IReadOnlyCollection<PlantHarvestCycleViewModel>`
- Plant name, variety, all dates (SeedingDate, GerminationDate, TransplantDate, HarvestDate)
- Metrics: GerminationRate, TotalWeightInPounds, TotalItems
- PlantCalendar array (if requested)
- GardenBedLayout (if requested)

**Query Approach:**
- MongoDB query on PlantHarvestCycle-Collection filtered by UserProfileId
- Apply all filters
- Use aggregation to join Plant data for name resolution
- Optionally join GardenBedPlantHarvestCycle for bed info

**Enables:**
- "Show me all tomato plantings with > 90% germination rate"
- "What was the yield for peppers planted in 2023?"
- Outcome correlation for date optimization agent

**Existing API:** ⚠️ Partial - GetPlantHarvestCyclesByPlant exists but returns identity-only, no metrics, no filters

---

### 3. get_garden_bed_history

**Purpose:** Historical bed usage for crop rotation validation

**Parameters:**
- `gardenBedId` (string) - Required: specific bed from Garden.GardenBeds
- `gardenId` (string) - Required: which garden owns this bed
- `startDate` (DateTime?, optional) - Filter from this date
- `endDate` (DateTime?, optional) - Filter to this date  
- `includeMetrics` (bool, default: false) - Include yield data

**Returns:** `IReadOnlyCollection<GardenBedPlantHarvestCycleViewModel>`
- Plant name, variety, start/end dates, location in bed
- Linked to PlantHarvestCycle for additional details

**Query Approach:**
- Call existing API via HTTP client: **HarvestController.GetGardenBedUsageHistory(gardenId, gardenBedId)**
- MCP tool makes authenticated HTTP request to PlantHarvest API
- If date filtering needed, apply post-query filtering in tool
- Returns Contract ViewModel directly from API

**Enables:**
- "What was planted in Bed 1 over the last 3 years?"
- Crop rotation validation for internal agent
- Succession planting analysis

**Existing API:** ✅ **Yes - HarvestController.GetGardenBedUsageHistory** already exists

---

### 4. get_planting_data_for_analysis

**Purpose:** Gather template dates, historical actual dates, and outcomes for agent to analyze and recommend optimal planting timing

**Parameters:**
- `plantId` (string) - Required: plant to analyze
- `plantName` (string?, optional) - Alternative to plantId
- `taskType` (string?, optional) - "SowIndoors", "TransplantOutside", etc.
- `gardenId` (string?, optional) - Specific garden context for weather dates
- `yearCount` (int, default: 3) - Look back this many years

**Returns:** Custom ViewModel with:
```json
{
  "plantName": "Tomato",
  "plantId": "plant_123",
  "taskType": "SowIndoors",
  "currentYear": 2026,
  "gardenContext": {
    "gardenName": "Steve's Garden",
    "lastFrostDate": "2026-05-15",
    "firstFrostDate": "2026-09-18",
    "warmSoilDate": "2026-06-07"
  },
  "growInstructionTemplate": {
    "name": "Start Indoors",
    "weatherCondition": "BeforeLastFrost",
    "weeksOffset": 6,
    "calculatedDateForThisYear": "2026-04-03",
    "instructions": "Start seeds indoors 6 weeks before last frost..."
  },
  "historicalActuals": [
    {
      "year": 2024,
      "eventDate": "2024-03-20",
      "source": "WorkLog",
      "daysBeforeTemplate": 14,
      "germinationRate": 92,
      "totalYieldPounds": 45,
      "harvestStartDate": "2024-07-15",
      "daysToFirstHarvest": 117
    },
    {
      "year": 2023,
      "eventDate": "2023-03-18",
      "source": "WorkLog",
      "daysBeforeTemplate": 16,
      "germinationRate": 88,
      "totalYieldPounds": 52,
      "harvestStartDate": "2023-07-10",
      "daysToFirstHarvest": 114
    },
    {
      "year": 2022,
      "eventDate": "2022-04-05",
      "source": "WorkLog",
      "daysBeforeTemplate": -2,
      "germinationRate": 70,
      "totalYieldPounds": 38,
      "harvestStartDate": "2022-07-28",
      "daysToFirstHarvest": 114
    }
  ]
}
```

**Query Approach:**
1. Get Plant + GrowInstructions via PlantCatalog API
2. Get Garden with weather dates via UserManagement API
3. Calculate template date from GrowInstruction + Garden weather dates for current year
4. Query WorkLog for historical actual dates (filter by plantId + reason + yearCount back)
5. Query PlantHarvestCycle for each historical planting to get outcomes (GerminationRate, yields, harvest dates)
6. Calculate deviation for each historical actual vs its year's template date
7. Return RAW DATA - no recommendations, no averages, no confidence scores
8. Let the agent analyze patterns, correlate dates with outcomes, and make recommendations

**Enables:**
- "When should I plant peppers this year based on my history?"
- Agent can reason about patterns: "You planted earlier in 2023/2024 with better results"
- Agent can consider context: weather forecasts, user preferences, risk tolerance
- Date optimization use case from internal agent design
- Agent makes nuanced decisions based on full context vs simple averaging

**Existing API:** ❌ No equivalent - requires cross-collection data gathering

**Why No Pre-Calculated Recommendations:**
- AI agents have reasoning capabilities to identify patterns humans might miss
- Agent can incorporate real-time context (current weather, user schedule, frost predictions)
- Different users may interpret "success" differently (yield vs early harvest vs disease resistance)
- Agent can explain its reasoning in natural language based on the data
- More flexible than hardcoded algorithms

---

### 5. get_garden_details

**Purpose:** Get garden context including weather dates and bed layout

**Parameters:**
- `gardenId` (string?, optional) - Specific garden, or user's default
- `gardenName` (string?, optional) - Alternative to gardenId

**Returns:** `GardenViewModel`
- Garden name, location, weather dates (LastFrostDate, FirstFrostDate, WarmSoilDate)
- GardenBeds array with layout

**Query Approach:**
- Call existing API via HTTP client: **UserManagement.GardenController.GetGarden(gardenId)**
- MCP tool makes authenticated HTTP request to UserManagement API
- If name provided, call GetGardenByName endpoint
- Returns Contract ViewModel directly from API

**Enables:**
- "What is my garden's last frost date?"
- Context for date calculations
- Bed selection for rotation suggestions

**Existing API:** ✅ **Yes - GardenController.GetGarden and GetGardenByName**

---

### 6. get_plant_details

**Purpose:** Get plant catalog information including grow instructions

**Parameters:**
- `plantId` (string?, optional) - Specific plant ID
- `plantName` (string?, optional) - Alternative to plantId
- `includeGrowInstructions` (bool, default: true) - Include embedded GrowInstructions
- `includeVarieties` (bool, default: false) - Include varieties

**Returns:** `PlantViewModel`
- Plant name, description, lifecycle, type
- GrowInstructions array (weather-relative timing rules)
- Varieties array (if requested)

**Query Approach:**
- Call existing API via HTTP client: **PlantCatalog.PlantController.GetPlantById(id)**
- MCP tool makes authenticated HTTP request to PlantCatalog API
- If name provided, call GetAllPlants and filter, or use search endpoint
- Returns Contract ViewModel directly from API

**Enables:**
- "What are the grow instructions for tomatoes?"
- Understanding template timing rules
- Date calculation context

**Existing API:** ✅ **Yes - PlantController.GetPlantById**

---

### 7. search_harvest_cycles

**Purpose:** Find harvest cycles with flexible filtering

**Parameters:**
- `year` (int?, optional) - Filter by year
- `gardenId` (string?, optional) - Filter by garden
- `startDate` (DateTime?, optional) - Filter by HarvestCycle.StartDate
- `endDate` (DateTime?, optional) - Filter by HarvestCycle.EndDate
- `includeArchived` (bool, default: false) - Include past seasons

**Returns:** `IReadOnlyCollection<HarvestCycleViewModel>`
- HarvestCycle name, start/end dates, garden reference

**Query Approach:**
- Call existing API via HTTP client: **HarvestController.GetAllHarvestCycles()**
- MCP tool makes authenticated HTTP request to PlantHarvest API
- Apply filters post-query if API doesn't support them
- Returns Contract ViewModels directly from API

**Enables:**
- "Show me all harvests from 2023"
- Context for multi-year analysis
- Season selection for historical queries

**Existing API:** ⚠️ Partial - GetAllHarvestCycles exists but no filters

---

## Tool Implementation Priority

### Phase 1: MongoDB-Based Analytical Tools (Complex Queries)
1. **get_worklog_history** - MongoDB query with flexible filtering
2. **get_plant_harvest_cycles** - MongoDB query with metrics and cross-collection joins
3. **get_planting_data_for_analysis** - MongoDB query gathering cross-collection data

### Phase 2: API-Based Context Tools (Simple Wrappers)
4. **get_garden_bed_history** - HTTP call to HarvestController.GetGardenBedUsageHistory
5. **get_garden_details** - HTTP call to GardenController.GetGarden
6. **get_plant_details** - HTTP call to PlantController.GetPlantById (includes PlantFamily)
7. **search_harvest_cycles** - HTTP call to HarvestController.GetAllHarvestCycles

**Implementation Note:** Tools #4-7 are simple HTTP client wrappers that authenticate and forward requests to existing APIs. They make the agent's interface consistent (everything is an MCP tool) while avoiding duplication of API logic.

**Crop Rotation Analysis:** Agent uses `get_garden_bed_history` + `get_plant_details` (PlantFamily classification) to reason about rotation quality, detect violations, and suggest alternatives - no dedicated pre-calculation tool needed.

---

## Data Requirements & Enhancements

### Required for Full Functionality:

1. **Plant Family Classification:**
   - Add `PlantFamily` enum to Plant catalog: Brassica, Solanaceae, Cucurbit, Legume, Allium, Root, Leafy, etc.
   - Needed for crop rotation analysis
   - Alternative: Build lookup table in MCP layer initially

2. **Success Indicators:**
   - Ensure PlantHarvestCycle.GerminationRate is consistently populated
   - Track quality ratings or success flags
   - Needed for outcome-based recommendations

3. **MongoDB Indexes:**
   - WorkLog: Compound index on (UserProfileId, EventDateTime, Reason)
   - PlantHarvestCycle: Compound index on (UserProfileId, PlantId, SeedingDate)
   - GardenBedPlantHarvestCycle: Compound index on (GardenBedId, StartDate)

4. **ViewModels:**
   - Create `PlantingDataViewModel` for get_planting_data_for_analysis (returns raw data for agent analysis)
   - Or return dynamic objects if MCP SDK supports it

---

## Summary: API vs MongoDB Decision Matrix

| Tool | Existing API | Decision | Rationale |
|------|-------------|----------|-----------|
| get_worklog_history | ❌ Too limited | **MongoDB** | API lacks date range + reason filtering |
| get_plant_harvest_cycles | ⚠️ Partial | **MongoDB** | API lacks metrics, dates, flexible filters |
| get_garden_bed_history | ✅ Good | **Call API** | GetGardenBedUsageHistory sufficient |
| get_planting_data_for_analysis | ❌ None | **MongoDB** | Cross-collection data gathering required |
| get_garden_details | ✅ Perfect | **Call API** | GetGarden is sufficient |
| get_plant_details | ✅ Perfect | **Call API** | GetPlantById is sufficient (includes PlantFamily) |
| search_harvest_cycles | ⚠️ Basic | **Call API** | GetAllHarvestCycles + post-filter acceptable |

**Conclusion:** All 7 tools exposed via MCP for unified agent interface. Tools #1, #2, #4 query MongoDB directly for complex analytics not available in APIs. Tools #3, #5, #6, #7 make HTTP calls to existing APIs via HttpClient, avoiding duplication while providing consistent MCP interface. Agent performs crop rotation analysis using raw bed history + plant family data rather than pre-calculated violation detection. This keeps us flexible for evolving agent requirements without boxing ourselves in.
  - Recommended planting window (start/end dates)
  - Based on: scheduled task target dates + historical actual planting dates
  - Statistics: average date, earliest, latest dates from history
- **Query Approach:**
  - **PRIMARY:** Query WorkLog collection filtered by Reason + UserProfileId
    - Get all EventDateTime values for actual completed activities
    - This is the authoritative source for "what actually happened"
  - **SECONDARY:** Query PlantHarvestCycle.PlantCalendar array for scheduled dates
    - PlantCalendar is the source of truth for planned schedules
    - Contains StartDate, EndDate, TaskType for each scheduled activity
    - Task generators create PlantTask records from these schedules
  - **TERTIARY:** Query PlantTask collection for TargetDateStart (derived from PlantCalendar)
  - **FALLBACK:** Query PlantHarvestCycle for SeedingDate/TransplantDate (summary fields, may be computed)
  - Calculate: average date across all years, typical range, standard deviation
  - Consider garden's LastFrostDate for outdoor planting recommendations
  - Return confidence level based on number of data points
  - **Note:** Distinguish between "when we planned" (PlantCalendar) vs "when we actually did it" (WorkLog)

**Additional Supporting Tools:**

`get_succession_planting_patterns`
- Analyze crop rotation: what follows what in garden beds
- Parameters: `gardenBedId` (optional), `minOccurrences` (int, default: 2)
- Returns: Common plant succession patterns (tomatoes → garlic → beans)

`get_plant_performance_summary`
- Plant success metrics over years
- Parameters: `plantName`, `gardenBedId` (optional)
- Returns: Year-over-year yields, germination rates, days to harvest, notes

**Important Note on Data Sources:**

When querying for "when did we actually do X" vs "when was X scheduled", understand the relationship:

**For Actual Completed Activities (what happened):**
1. **WorkLog.EventDateTime** (AUTHORITATIVE) - User-logged actual activities with definitive dates
2. **PlantTask.CompletedDateTime** - When tasks were marked complete in task system
3. **PlantHarvestCycle summary dates** (SeedingDate, TransplantDate, etc.) - Computed or manually entered

**For Planned/Scheduled Activities (what was planned):**
1. **PlantHarvestCycle.PlantCalendar[]** (PRIMARY SOURCE) - Embedded schedule array with StartDate, EndDate, TaskType
   - This is the master schedule that drives task generation
   - **Generated from:** PlantGrowthInstruction templates + Garden weather dates
     - GrowInstruction: "Start seeds 4 weeks before last frost"
     - Garden.LastFrostDate: May 15, 2023
     - Calculated PlantCalendar.StartDate: April 17, 2023
   - Task generators (IndoorSawTaskGenerator, etc.) read PlantCalendar to create PlantTask records
   - When schedules change, PlantScheduleUpdated events trigger task synchronization
2. **PlantTask.TargetDateStart/End** - Task records derived from PlantCalendar schedules
3. **PlantGrowthInstruction** (TEMPLATE) - Weather-relative timing rules (e.g., "4 weeks before last frost")
   - Used to generate PlantCalendar when plant added to harvest
   - Not specific to any year - defines relative timing pattern

**Critical Synchronization Rule:**
- PlantCalendar schedules and PlantTask records MUST stay synchronized
- Updating task dates requires updating the corresponding PlantCalendar schedule
- The system enforces this through domain events: `PlantScheduleUpdated` → Task generators update PlantTask records
- See code: `PlantHarvestCycle.UpdatePlantSchedule()` → `PlantSchedule.Update()` → raises `PlantScheduleUpdated` event

WorkLog is the single source of truth for actual activities because users log what they did when they did it. PlantCalendar is the source of truth for schedules/plans. GrowInstructions are templates that generate PlantCalendar schedules.

**Tools will be added to this section as they are implemented.**

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Data leakage between users** | High | Enforce UserProfileId filtering in all queries; comprehensive testing |
| **MongoDB performance impact** | Medium | Read-only connection, indexes, query optimization, separate connection pool |
| **MCP protocol changes** | Medium | Abstract protocol handling, version negotiation |
| **Auth0 token validation overhead** | Low | Token caching, efficient middleware |
| **Agent costs (external LLMs)** | Medium | Rate limiting, user quotas, tool result pagination |
| **Write operations data corruption** | High | Phase 2: Always call production APIs, never bypass validation |
| **Complex queries timeout** | Medium | Query timeouts, pagination, background jobs for heavy analytics |

## Success Metrics

- **Developer Experience:**
  - GitHub Copilot can successfully query my garden data during development
  - Tool invocation latency < 500ms (p95)
  - Zero user data leakage incidents

- **Functionality:**
  - All Phase 1 tools operational
  - 100% query success rate for valid requests
  - Accurate historical data retrieval

- **Adoption (Future):**
  - % of users who generate agent tokens
  - Number of tool invocations per user per month
  - User satisfaction with natural language queries

## Implementation Roadmap

### Phase 1: Core MCP Server (MVP - Read-Only) - 2-3 weeks

**Week 1: Foundation & Research**
- [ ] Research ModelContextProtocol.AspNetCore HTTP transport configuration
- [ ] Understand authentication integration with ASP.NET Core middleware
- [ ] Create GardenLog.Mcp project structure
- [ ] Configure MongoDB connections (all domains)
- [ ] Set up development authentication mode
- [ ] Configure MCP server with HTTP transport

**Week 2: Query Services & Tools**
- [ ] Implement IUserContextAccessor for user context
- [ ] Build query services as needed for specific tools
- [ ] Implement MCP tools based on actual questions/requirements with [McpServerTool] attributes
- [ ] Map domain entities to Contract ViewModels

**Week 3: Testing & Refinement**
- [ ] Add MongoDB indexes for query performance
- [ ] Unit tests for implemented tools and services
- [ ] Integration tests with MCP client or MCP Inspector
- [ ] Local testing with GitHub Copilot
- [ ] Performance optimization
- [ ] Refine tool descriptions based on agent behavior

**Research Items:**
- How does ModelContextProtocol.AspNetCore expose HTTP endpoints?
- What's the default URL/route for the MCP endpoint?
- How to integrate Auth0 JWT validation with MCP HTTP transport?
- Is there an MCP Inspector or test client available?
- Stdio vs HTTP transport configuration differences

### Phase 2: Write Operations - 1-2 weeks

**Week 4-5:**
- [ ] Add HTTP clients for production APIs
- [ ] Implement write tools (create, update, log)
- [ ] Test write operations end-to-end
- [ ] Update documentation

### Phase 3: Production Deployment - 1 week

**Week 6:**
- [ ] Create Dockerfile
- [ ] Azure deployment configuration
- [ ] Production authentication (PAT generation UI)
- [ ] Monitoring & audit logging
- [ ] Production testing
- [ ] Documentation & user guides

### Future Enhancements (Post-MVP)
- Advanced analytics tools
- Visualization capabilities
- Community data sharing
- Voice interface integration

## Future Enhancements (Post-Phase 3)

1. **Advanced Analytics Tools:**
   - `analyze_crop_rotation`: Recommend rotation based on history and best practices
   - `predict_planting_window`: ML-based optimal planting date recommendations
   - `compare_varieties`: Side-by-side variety performance analysis
   - `suggest_companion_plants`: Companion planting recommendations

2. **Visualization Tools:**
   - `generate_garden_map`: Visual garden layout with plant locations
   - `plot_yield_trends`: Time-series charts of yields
   - `visualize_bed_rotation`: Crop rotation visualization

3. **Notification & Automation:**
   - `subscribe_to_alerts`: Agent monitors conditions and notifies user
   - `auto_schedule_tasks`: Generate planting schedule based on climate

4. **Multi-user Features:**
   - `share_garden_data`: Share anonymized data for community insights
   - `compare_with_community`: "How does my tomato yield compare regionally?"

5. **Voice Interface:**
   - Integrate with voice assistants (Alexa, Google Assistant)
   - Natural language voice queries to garden data

## Appendix

### MCP Resources

- MCP Specification: https://spec.modelcontextprotocol.io/
- MCP SDK Documentation: [to be determined based on chosen implementation]
- GitHub Copilot MCP Integration: https://docs.github.com/copilot

### MongoDB Query Examples

**Complex Aggregation - Plant Statistics:**
```javascript
db.getCollection("PlantHarvestCycle-Collection").aggregate([
  {
    $match: {
      UserProfileId: "user_123",
      PlantId: "plant_tomato"
    }
  },
  {
    $group: {
      _id: "$PlantVarietyId",
      varietyName: { $first: "$PlantName" },
      totalSeasons: { $sum: 1 },
      avgYield: { $avg: "$TotalWeightInPounds" },
      successCount: {
        $sum: {
          $cond: [{ $gt: ["$TotalWeightInPounds", 0] }, 1, 0]
        }
      },
      avgDaysToHarvest: {
        $avg: {
          $dateDiff: {
            startDate: "$SeedingDate",
   odelContextProtocol NuGet Package: https://www.nuget.org/packages/ModelContextProtocol
- ModelContextProtocol.AspNetCore Package: https://www.nuget.org/packages/ModelContextProtocol.AspNetCore
- MCP C# SDK API Docs: https://modelcontextprotocol.github.io/csharp-sdk/api/
- MCP C# SDK GitHub: https://github.com/modelcontextprotocol/csharp-sdk
            unit: "day"
          }
        }
      }
    }
  },
  {
    $project: {
      varietyName: 1,
      totalSeasons: 1,
      avgYield: { $round: ["$avgYield", 2] },
      successRate: {
        $multiply: [
          { $divide: ["$successCount", "$totalSeasons"] },
          100
        ]
      },
      avgDaysToHarvest: { $round: ["$avgDaysToHarvest", 0] }
    }
  },
  {
    $sort: { avgYield: -1 }
  }
]);
```

### Configuration Reference

**Complete appsettings.json (Production):**
```json
{
  "Authentication": {
    "Mode": "Production",
    "Auth0": {
      "Domain": "https://gardenlog.auth0.com",
      "Audience": "gardenlog-mcp-api",
      "ClientId": "mcp-service-client-id",
      "ClientSecret": "${AUTH0_CLIENT_SECRET}"
    }
  },
  "MongoDB": {
    "ConnectionString": "${MONGODB_CONNECTION_STRING}",
    "DatabaseName": "GardenLogDb",
    "Collections": {
      "HarvestCycles": "HarvestCycle-Collection",
      "PlantHarvestCycles": "PlantHarvestCycle-Collection",
      "GardenBedUsage": "GardenBedUsage-Collection",
      "WorkLogs": "WorkLog-Collection",
      "Plants": "Plant-Collection",
      "PlantVarieties": "PlantVariety-Collection",
      "Gardens": "Garden-Collection",
      "Weather": "Weather-Collection"
    },
    "MaxConnectionPoolSize": 100,
    "ReadPreference": "SecondaryPreferred"
  },
  "ExternalApis": {
    "PlantHarvest": "https://plantharvest-api.gardenlog.com",
    "PlantCatalog": "https://plantcatalog-api.gardenlog.com",
    "UserManagement": "https://usermanagement-api.gardenlog.com"
  },
  "RateLimiting": {
    "EnableRateLimiting": true,
    "PermitLimit": 100,
    "Window": "00:01:00"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "GardenLog.Mcp": "Debug"
    },
    "ApplicationInsights": {
      "ConnectionString": "${APPINSIGHTS_CONNECTION_STRING}"
    }
  }
}
```

### Glossary

- **MCP (Model Context Protocol):** Standard protocol for connecting AI agents to external tools and data sources
- **Agent:** AI system (like GitHub Copilot, Claude) that can invoke tools on behalf of users
- **Tool:** Discrete function exposed via MCP that performs a specific action or query
- **Harvest Cycle:** Growing season where user tracks plants from seed to harvest
- **Plant Harvest Cycle:** Individual plant within a harvest cycle
- **Garden Bed Layout:** Spatial allocation of plants within garden beds
- **UserProfileId:** Unique identifier for user, used for data isolation
- **JWT:** JSON Web Token used for authentication
- **PAT:** Personal Access Token for agent authentication

## Key Decision Log

### Decision 1: Centralized vs Embedded MCP Architecture
**Date:** 2026-02-17  
**Decision:** Centralized MCP server (GardenLog.Mcp)  
**Rationale:** 
- Agents need analytical queries that production APIs don't support
- Single deployment/versioning for all agent tools
- Keeps production APIs focused and clean
- Better UX for agents (single connection point)

### Decision 2: Direct MongoDB Access for Reads (Hybrid Approach)
**Date:** 2026-02-17  
**Decision:** Hybrid approach - use APIs where sufficient, direct MongoDB for complex analytics  
**Rationale:**
- Simple queries: Use existing APIs (e.g., get harvest cycle by ID)
- Complex analytics: Query MongoDB directly (e.g., aggregated statistics across seasons)
- **Always return Contract models** - same ViewModels whether via API or MongoDB
- Enables complex aggregation pipelines for analytics not in APIs
- Better performance for analytical queries
- Bounded context less critical for reads
- Write operations still go through APIs

**Implementation Pattern:**
- Query services have both `IPlantHarvestApiClient` and `IMongoCollection<T>` injected
- Decide per-method whether to use API or direct MongoDB
- Map MongoDB entities to existing Contract ViewModels (PlantHarvestCycleViewModel, etc.)
- Result: Consistent data shape regardless of data source

### Decision 3: Auth0 for Security
**Date:** 2026-02-17  
**Decision:** Reuse existing Auth0 infrastructure with dev mode  
**Rationale:**
- Consistent with entire GardenLog ecosystem
- JWT tokens already used across all services
- Dev mode enables fast local iteration
- Production-ready security model

### Decision 4: Phase 1 Read-Only
**Date:** 2026-02-17  
**Decision:** Start with read-only tools, add writes in Phase 2  
**Rationale:**
- Lower risk MVP
- Validate agent usefulness with queries first
- Provides immediate value
- Time to refine security model before writes

### Decision 5: MCP Protocol for External Agents
**Date:** 2026-02-17  
**Decision:** Use MCP protocol standard via ModelContextProtocol NuGet package  
**Rationale:**
- Official C# SDK from Anthropic/Microsoft
- GitHub Copilot support is primary use case
- Standard protocol for agent integration
- Attribute-based tool registration (simpler than manual JSON-RPC handling)
- Automatic tool discovery and invocation
- Built-in hosting and dependency injection
- Future-proof for other agents (Claude, custom)
- HTTP transport via ModelContextProtocol.AspNetCore package

**Package details:**
- ModelContextProtocol v0.8.0-preview.1 (core functionality)
- ModelContextProtocol.AspNetCore v0.8.0-preview.1 (HTTP transport)
- Note: Package is in preview; breaking changes possible before release

**Implementation Notes:**
- HTTP transport configuration details need further investigation (ModelContextProtocol.AspNetCore docs)
- May need to research authentication integration with ASP.NET Core middleware
- Consider using MCP Inspector tool for testing during development

---

## Document Control

**Version:** 1.0  
**Last Updated:** February 17, 2026  
**Next Review:** After Phase 1 completion  
**Owner:** GardenLog Development Team  

**Change History:**
| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-02-17 | 1.0 | Initial design document created | Development Team |

---

**Questions or Feedback:** Contact the development team or update this document as implementation progresses.
