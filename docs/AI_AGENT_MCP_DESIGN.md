# GardenLog MCP Design (API-First)

## Purpose

This document defines the MCP architecture for GardenLog with a strict API-first boundary:

- MCP tools call domain APIs only.
- MCP does not read domain databases directly.
- Domain teams evolve query flexibility in their APIs.
- MCP remains a thin, agent-oriented orchestration layer.

This approach supports independent service deployment and avoids domain leakage into `GardenLog.Mcp`.

---

## Current Direction

### Architecture Summary

- **MCP host:** `src/GardenLog.Mcp/GardenLog.Mcp.Api`
- **Tool layer:** `src/GardenLog.Mcp/GardenLog.Mcp.Application/Tools`
- **API client layer:** `src/GardenLog.Mcp/GardenLog.Mcp.Infrastructure/ApiClients`
- **Domain APIs:** PlantCatalog, PlantHarvest, UserManagement, GrowConditions

### Core Rules

1. **No direct datastore access from MCP**
2. **All user-scoped filtering enforced by downstream APIs**
3. **MCP forwards authenticated user context/token to APIs**
4. **Tool responses are agent-friendly and structured where supported**

---

## Implemented Baseline

### `get_plant_details`

Status: ✅ Working

- Uses API wrapper calls to PlantCatalog endpoints
- Fetches plant details and grow instructions
- Uses parallel retrieval where applicable
- Caches only complete composed results
- Fails fast if required grow instruction data is missing
- Uses `UseStructuredContent = true` for better MCP client handling

### `get_worklog_history`

Status: ✅ Implemented as API wrapper

- MCP calls PlantHarvest API search route (no direct DB query)
- Supports flexible filtering for historical work log analysis
- Returns user-scoped historical records via API contracts

---

## PlantHarvest API Additions for Independent Deploy

The following API-first additions support MCP analytical use cases while keeping domain logic inside PlantHarvest.

### WorkLog Search

- Route constant in `PlantHarvest.Contract/HarvestRoutes.cs`:
  - `SearchWorkLogs`
- Query contract in `PlantHarvest.Contract/Query/WorkLogQuery.cs`:
  - `WorkLogSearch`
- Repository contract and implementation:
  - `SearchWorkLogsForUser(...)`
- API query handler support:
  - `WorkLogQueryHandler.SearchWorkLogs(...)`
- Controller endpoint:
  - `WorkLogController` exposes `GET /v1/api/WorkLog/search`

### PlantHarvestCycle Search

- Route constant in `PlantHarvest.Contract/HarvestRoutes.cs`:
  - `SearchPlantHarvestCycles`
- Query contract in `PlantHarvest.Contract/Query/PlantHarvestCycleQuery.cs`:
  - `PlantHarvestCycleSearch`
- Repository contract and implementation:
  - `SearchPlantHarvestCyclesForUser(...)`
- API query handler support:
  - `HarvestQueryHandler.SearchPlantHarvestCycles(...)`
- Controller endpoint:
  - `HarvestCycleController` exposes `GET /v1/api/HarvestCycle/search`

### Deployment Boundary

These routes allow PlantHarvest to deploy independently while serving MCP query needs through stable HTTP contracts.

---

## MCP Tool Catalog (API-First)

### 1) `get_worklog_history`

**Purpose:** Query completed activities with flexible filters.

**Parameters:**

- `startDate` (optional)
- `endDate` (optional)
- `reason` (optional)
- `plantId` (optional)
- `harvestCycleId` (optional)
- `plantName` (optional)
- `limit` (optional, bounded)

**Implementation approach:**

1. Resolve `plantName` to `plantId` through PlantCatalog API when needed.
2. Call PlantHarvest `SearchWorkLogs` route.
3. Return API contract data to MCP client.

---

### 2) `get_plant_harvest_cycles`

**Purpose:** Flexible cycle search with optional outcome metrics.

**Parameters:**

- `plantId` (optional)
- `plantName` (optional)
- `harvestCycleId` (optional)
- `startDate` (optional)
- `endDate` (optional)
- `minGerminationRate` (optional)
- `includeMetrics` (default true)
- `includePlantCalendar` (default false)
- `includeGardenBeds` (default false)
- `limit` (optional, bounded)

**Implementation approach:**

1. Resolve names to IDs through PlantCatalog API as needed.
2. Call PlantHarvest `SearchPlantHarvestCycles` route.
3. Return API contract data to MCP client.

---

### 3) `get_garden_bed_history`

**Purpose:** Historical bed occupancy for crop-rotation reasoning.

**Implementation approach:**

- Call existing PlantHarvest `GetGardenBedUsageHistory` API route.
- Apply optional date filtering at API level when available, else post-filter in MCP.

---

### 4) `get_garden_details`

**Purpose:** Garden context including frost dates and bed layout.

**Implementation approach:**

- Call UserManagement `GetGarden` / `GetGardenByName` APIs.

---

### 5) `search_harvest_cycles`

**Purpose:** Locate seasons by year/garden/date constraints.

**Implementation approach:**

- Use PlantHarvest list/search routes.
- Prefer server-side filters; only post-filter in MCP when API capability lags.

---

### 6) `get_planting_data_for_analysis`

**Purpose:** Return raw cross-domain data for agent reasoning (no hardcoded recommendation logic).

**Implementation approach:**

1. Plant + grow instructions from PlantCatalog API
2. Garden weather context from UserManagement API
3. Historical actions from PlantHarvest `SearchWorkLogs`
4. Historical outcomes from PlantHarvest `SearchPlantHarvestCycles`
5. MCP composes and returns raw data model for the agent

---

## API Capability Gaps and Ownership

When MCP needs new filters/fields:

- **Do not add datastore logic to MCP.**
- Add or extend query routes in the owning domain API.
- Keep filtering, authorization, and projections in domain service boundaries.

This maintains clean ownership and avoids duplicating persistence models in MCP.

---

## Security Model

### Authentication

- MCP validates JWT (Auth0).
- MCP forwards user token/context to downstream APIs.

### Authorization

- Domain APIs enforce user scoping (`UserProfileId`) and resource-level rules.
- MCP does not bypass domain authorization.

### Error Handling

- MCP returns agent-friendly validation and not-found messages.
- Internal exception details are logged server-side, not leaked to tool callers.

---

## Performance and Reliability

### MCP-side patterns

- Parallelize independent API calls where safe.
- Cache only complete composed objects.
- Use bounded limits and explicit defaults.
- Fail fast when required dependencies return incomplete data.

### API-side expectations

- Search routes should support server-side filtering/sorting/paging.
- Queries should avoid full scans and return only required fields.
- Route contracts should remain backward compatible where possible.

---

## Testing Checklist

### Tool-level

- [ ] `tools/list` includes expected tools
- [ ] Structured content appears for tools configured with `UseStructuredContent`
- [ ] Validation errors are readable and actionable

### Security

- [ ] Invalid JWT rejected
- [ ] Cross-user access denied by domain APIs

### Query behavior

- [ ] WorkLog search respects all filters and bounds
- [ ] PlantHarvestCycle search respects all filters and bounds
- [ ] Plant-name lookup resolves correctly before search calls

### Integration

- [ ] MCP tool calls route correctly to downstream APIs
- [ ] Partial downstream failures produce deterministic MCP errors

---

## Rollout Plan

### Phase 1 (Now)

- ✅ Stabilize `get_plant_details`
- ✅ Deliver API-wrapper `get_worklog_history`
- ✅ Add PlantHarvest API search routes required for MCP

### Phase 2

- Implement MCP `get_plant_harvest_cycles` on top of `SearchPlantHarvestCycles`
- Add/extend API filters for any missing use cases discovered in testing
- Implement `get_planting_data_for_analysis` composition tool

### Phase 3

- Add additional API-wrapper tools (`get_garden_details`, `get_garden_bed_history`, `search_harvest_cycles`)
- Harden observability and rate limits for production traffic

---

## Decision Log

### Decision 1: Centralized MCP Server

**Status:** Active

MCP remains centralized in `GardenLog.Mcp` for a single agent endpoint, but it orchestrates through domain APIs.

### Decision 2: API-First Data Access

**Status:** Active (supersedes prior hybrid/direct-db approach)

- MCP tools consume domain APIs only.
- Analytical flexibility is delivered by expanding domain query endpoints.
- This protects bounded contexts and independent service deployability.

### Decision 3: Read-first Scope

**Status:** Active

Phase 1 focuses on read/query tools. Write tools are deferred until safety and UX are proven.

---

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| API gaps slow MCP feature delivery | Medium | Add targeted search/query routes in owning APIs |
| Over-fetching from list endpoints | Medium | Prefer dedicated search endpoints with server-side filters |
| Inconsistent tool error quality | Medium | Standardize friendly error mapping in tool layer |
| Contract drift across services | High | Version routes/contracts and validate in CI |

---

## Document Control

**Version:** 1.1  
**Last Updated:** 2026-02-18  
**Owner:** GardenLog Development Team

### Change History

| Date | Version | Changes |
|------|---------|---------|
| 2026-02-17 | 1.0 | Initial design draft |
| 2026-02-18 | 1.1 | API-first rewrite; removed MCP direct-datastore guidance; aligned with PlantHarvest search routes |
