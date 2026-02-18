# GardenLog MCP Server

Model Context Protocol server for GardenLog AI agent integration.

## Quick Start

### 1. Run the MCP Server

```bash
cd src/GardenLog.Mcp/GardenLog.Mcp.Api
dotnet run
```

Server will start on `http://localhost:5100`

### 2. Test with curl

**Tool Discovery:**
```bash
curl -X POST http://localhost:5100/mcp \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_AUTH0_JWT_TOKEN" \
  -d '{
    "jsonrpc": "2.0",
    "method": "tools/list",
    "id": 1
  }'
```

**Call get_plant_details:**
```bash
curl -X POST http://localhost:5100/mcp \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_AUTH0_JWT_TOKEN" \
  -d '{
    "jsonrpc": "2.0",
    "method": "tools/call",
    "params": {
      "name": "get_plant_details",
      "arguments": {
        "plantId": "5f9a7b3c1c9d440000a1b2c3"
      }
    },
    "id": 2
  }'
```

### 3. Get Your Auth0 JWT Token

1. Open GardenLogWeb in browser
2. Navigate to Preferences → Generate Access Token
3. Copy the token
4. Use it in the `Authorization: Bearer <token>` header

### 4. Configure GitHub Copilot (Optional)

Add to `.vscode/mcp-settings.json` or Copilot settings:

```json
{
  "mcpServers": {
    "gardenlog": {
      "command": "http",
      "args": ["POST", "http://localhost:5100/mcp"],
      "headers": {
        "Authorization": "Bearer YOUR_TOKEN_HERE",
        "Content-Type": "application/json"
      }
    }
  }
}
```

## Architecture

### Token Flow

1. User authenticates with Auth0 → gets JWT
2. User provides JWT to agent (GitHub Copilot)
3. Agent calls MCP server `/mcp` endpoint with `Authorization: Bearer <jwt>`
4. MCP server:
   - Validates JWT
   - Extracts UserProfileId from claims
   - Forwards JWT to downstream APIs via `UserAuthenticationHandler`
5. Downstream APIs validate JWT and filter by UserProfileId

This ensures user-level data isolation throughout the chain.

### Current Tools

1. **get_plant_details** - Retrieves plant information from PlantCatalog API
   - Parameter: `plantId` (required)
  - Returns: Combined payload with `plant` metadata and `growInstructions` list
2. **get_worklog_history** - Searches user-scoped work logs from PlantHarvest API
  - Parameters: `startDate`, `endDate`, `reason`, `limit`
  - Returns: WorkLog history ordered by event date
3. **get_plant_harvest_cycles** - Searches user-scoped plant harvest cycles from PlantHarvest API
  - Parameters: `plantId`, `harvestCycleId`, `startDate`, `endDate`, `minGerminationRate`, `limit`
  - Returns: Plant harvest cycle records with optional calendar/layout data

## Authentication Pattern

### Why UserAuthenticationHandler (Not Auth0AuthenticationHandler)?

The MCP server uses **UserAuthenticationHandler** to forward the user's JWT token, which is different from how other backend APIs work:

**PlantHarvest API → UserManagement API:**
- Uses `Auth0AuthenticationHandler` (from SharedInfrastructure)
- Gets a NEW service token from Auth0 using client credentials
- Service-to-service authentication

**MCP Server → Downstream APIs:**
- Uses `UserAuthenticationHandler` (custom in Infrastructure layer)
- FORWARDS the existing user JWT from the agent
- Maintains user context through the entire chain

**Why this matters:**
1. The AI agent already has the user's JWT (from GardenLogWeb)
2. We want to preserve the UserProfileId claim throughout the call chain
3. Downstream APIs filter data by UserProfileId for security
4. MCP server acts as a pass-through, not as a service principal

**Auth0 Registration:**
Yes, the MCP server still needs to be registered with Auth0 to **validate** the incoming JWT, even though it doesn't request new tokens. The downstream APIs also validate the same JWT.

### Health Checks

- **Liveness**: `http://localhost:5100/healthz`
- **Readiness**: `http://localhost:5100/ready`

## Development

### API URLs Configuration

API URLs default to production Azure Container Apps. To test with local APIs, uncomment the local URLs in Program.cs:

```csharp
// Production URLs (default)
string plantCatalogUrl = "https://plantcatalogapi-containerapp...";

// Uncomment to use local APIs
// plantCatalogUrl = "https://localhost:5051";
```

### Authentication

The MCP server **always** requires a valid Auth0 JWT token, even in development. There is no "development mode" that bypasses authentication.

To test locally:
1. Get a valid JWT from GardenLogWeb (Preferences → Generate Access Token)
2. Use it in the Authorization header
3. The token is validated by Auth0 and forwarded to downstream APIs

### Adding New Tools

1. Create tool class in `GardenLog.Mcp.Application/Tools/`
2. Mark with `[McpServerToolType]`
3. Add method with `[McpServerTool(Name = "tool_name")]`
4. Add `[Description]` attributes for parameters
5. Tools are auto-discovered at startup

**API-Calling Tool Example:**
```csharp
[McpServerToolType]
public class MyTool
{
    private readonly IPlantCatalogApiClient _apiClient;
    private readonly ILogger<MyTool> _logger;
    
    public MyTool(IPlantCatalogApiClient apiClient, ILogger<MyTool> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }
    
    [McpServerTool(Name = "my_tool")]
    [Description("Tool description")]
    public async Task<ResultViewModel> ExecuteAsync(
        [Description("Param description")] string param,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("my_tool called: {Param}", param);
        
        // JWT token automatically forwarded by UserAuthenticationHandler
        var result = await _apiClient.SomeMethod(param);
        
        return result;
    }
}
```

**Remember:**
- Only add ApiClient methods you actually need (avoid GetAll if not required)
- JWT token is automatically forwarded via UserAuthenticationHandler
- Keep tool parameters simple - let the AI agent do the reasoning
- Return raw data, not pre-calculated recommendations

## Next Steps

- [ ] Test with real Auth0 JWT token
- [ ] Add more API-wrapper tools (get_garden_details, search_harvest_cycles)
- [ ] Add error handling and retry policies
- [ ] Add audit logging
- [ ] Deploy to Azure Container Apps

## Troubleshooting

**"401 Unauthorized" from downstream API:**
- Check that your JWT token is valid
- Verify token has not expired
- Ensure token has correct audience ("https://api.GardenLog")

**"Tool not found":**
- Check tool is marked with `[McpServerToolType]` and `[McpServerTool]`
- Verify tool class is public
- Check logs for tool discovery at startup

**"Cannot resolve service":**
- Check HttpClient is registered with correct name
- Verify UserAuthenticationHandler is registered as transient
- Check IUserContextAccessor is registered as scoped
