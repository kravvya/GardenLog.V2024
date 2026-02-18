# MCP Improvement Tracker

Purpose: Track MCP improvements in one place so we can implement in order and check items off.

## Status Legend

- [ ] Not started
- [~] In progress
- [x] Completed

---

## 1) Fix Existing Bugs

### A. `get_plant_harvest_cycles` returns empty for valid `plantId`

- [x] Reproduce with known onion plant ID (`75579cc4-6889-4e30-9702-b10a38f7f15c`) and user context.
- [x] Verify API contract alignment (`plantId` vs `plantHarvestCycleId` vs related filters).
- [x] Inspect PlantHarvest search route mapping and query handler filtering.
- [x] Add/adjust integration tests for expected non-empty results.
- [x] Fix implementation and validate via local build.
- [x] Enforce API-first behavior in MCP (`plantId` required, no in-memory fallback).
- [x] Validate via deployed MCP tool call.
- [ ] Confirm backward compatibility for existing parameters.
- Note: After Harvest API redeploy, deployed MCP returns onion records correctly and required `plantId` enforcement remains active.

### B. `get_worklog_history` broad queries return empty unexpectedly

- [x] Reproduce with only `limit` and with broad date ranges.
- [x] Verify whether current dataset is in WorkLog domain objects or only in bed/cycle records.
- [x] Validate API route behavior and user-scoping predicates.
- [x] Fix query logic and/or projection mapping.
- [x] Add server-side `plantId` filter to WorkLog search.
- [x] Make `plantId` required end-to-end for WorkLog search path.
- [x] Add test coverage for broad and filtered queries.
- [x] Validate from deployed MCP end-to-end.
- Note: After Harvest API redeploy, deployed MCP returns onion worklogs correctly and required `plantId` enforcement remains active.

### C. `get_garden_details` initial unscoped call fails with unclear error

- [x] Reproduce unscoped behavior (`gardenId` and `gardenName` omitted).
- [x] Decide expected behavior: deterministic validation error vs default garden fallback.
- [x] Implement explicit, friendly validation response.
- [x] Implement single-garden auto-resolution for unscoped calls.
- [ ] Add tests for error semantics and success path.
- [ ] Validate deployed MCP caller sees actionable error text.
- Note: Deployed MCP unscoped call now succeeds (single garden auto-resolution observed). Error-message path still needs explicit multi/zero-garden validation.

### D. Oversized payload friction

- [ ] Identify heavy responses (`get_garden_details` with beds, large bed history windows).
- [ ] Add bounded defaults and/or reduced projections where possible.
- [ ] Confirm tool callers can request lightweight mode.
- [ ] Validate agent can answer common questions without parsing large dumps.

---

## 2) Improve Tool Descriptions

Goal: Make tools self-explanatory, reduce misuse, and make parameter intent explicit.

### Description quality checklist (apply to each tool)

- [ ] State primary purpose in one sentence.
- [ ] State expected inputs and required combinations.
- [ ] Clarify common pitfalls (e.g., when name lookup is needed first).
- [ ] State output shape at a high level.
- [ ] Include at least one practical example query pattern.

### Tools to revise first

- [ ] `get_worklog_history`
  - [ ] Document actual supported filters and default behavior.
  - [ ] Clarify if plant-based filtering is supported directly.
- [ ] `get_plant_harvest_cycles`
  - [ ] Clarify semantics of `plantId` and other filters.
  - [ ] Clarify expected metrics and when empty result is normal.
- [ ] `search_harvest_cycles`
  - [ ] Clarify this is cycle-focused, not plant-search focused.
- [ ] `get_garden_details`
  - [ ] Warn about large payload when including beds.
  - [ ] Document efficient usage for weather-date questions.
- [ ] `get_garden_bed_history`
  - [ ] Clarify ideal lookback strategy and limits for rotation analysis.
- [ ] `get_plant_details`
  - [ ] Highlight it includes grow instructions (timing anchors).

---

## 3) Concrete Agent Plan for Answering Questions

This is the default plan the agent should follow using exposed tools.

> Note (User Feedback): User disagrees with the current Agent Plan approach. We will debate and revise this plan **after bug fixes** and **before** tool-description updates or adding new tools.

### Step 0: Identify question type

- [ ] Plant timing question (e.g., “When should I seed onions?”)
- [ ] Crop rotation question
- [ ] Historical performance/outcome question
- [ ] General garden context question

### Step 1: Resolve context once

- [ ] Get garden context with minimal scope first (`get_garden_details` without heavy extras when possible).
- [ ] Extract weather anchors (`lastFrostDate`, `firstFrostDate`, `warmSoilDate`) and garden ID.

### Step 2: Resolve plant identity

- [ ] If plant ID is known, proceed.
- [ ] If only plant name is known, resolve via existing pathway (currently indirect via history/details) until dedicated tool exists.
- [ ] Validate canonical plant record via `get_plant_details`.

### Step 3: Pull direct planning signal first

- [ ] Use `get_plant_details` grow instructions as baseline timing.
- [ ] Convert relative timing to calendar dates using garden weather anchors.

### Step 4: Pull historical signal (if needed)

- [ ] Query `get_worklog_history` for sow/transplant/harvest events (if reliable after bug fix).
- [ ] Query `get_plant_harvest_cycles` for outcome metrics and date patterns (if reliable after bug fix).
- [ ] Use `get_garden_bed_history` only for targeted confirmation (avoid broad brute-force scans).

### Step 5: Compose answer with confidence

- [ ] Return direct date window + rationale from grow instructions.
- [ ] Add history-based adjustment only if data exists.
- [ ] State confidence level: High (instruction + history), Medium (instruction only), Low (partial data).

### Step 6: Escalation path when data is missing

- [ ] If worklog/cycle tools return empty unexpectedly, flag as MCP bug and fallback to grow instruction + weather anchors.
- [ ] Keep user answer unblocked while documenting gap for fix.

---

## 4) Missing Tools (to add only if gaps remain after fixes)

### High-priority candidates

- [x] `search_plants`
  - Input: plant name text
  - Output: canonical `plantId`, `plantName`, aliases/tags
  - Why: eliminate indirect plant-ID discovery.
  - Status: Implemented in MCP using existing PlantCatalog routes (`GetIdByPlantName` exact-first + `GetAllPlantNames` fallback contains matching).

- [ ] `get_planting_window`
  - Input: `plantId`, `gardenId`, optional year and planting method
  - Output: computed date windows (seed indoors, transplant, direct sow)
  - Why: answer timing questions in one call.

- [ ] `get_plant_history_summary`
  - Input: `plantId`, `gardenId`, optional lookback years
  - Output: compact season summary of planned vs actual + outcomes
  - Why: avoid large multi-call orchestration for recommendation context.

### Optional (use-case driven)

- [x] `get_current_harvest_cycle`
  - Input: optional `gardenId`, optional `asOfDate`
  - Output: single current harvest cycle (or null)
  - Rule: `StartDate <= asOfDate` and `EndDate == null`, newest `StartDate` wins.
  - Why: simplify seasonal context resolution for planning questions.

- [ ] `get_rotation_risk`
  - Input: `gardenId`, `gardenBedId`, `plantId`, lookback years
  - Output: risk level, evidence, suggested alternatives
  - Why: directly supports internal rotation advisory use case.

---

## 5) Execution Order

- [x] Phase 1: Fix bug A (`get_plant_harvest_cycles`) - code changes complete
- [x] Phase 2: Fix bug B (`get_worklog_history`) - code changes complete
- [x] Phase 3: Fix bug C (`get_garden_details` error semantics) - code changes complete
- [x] Targeted local integration tests for Phase 1/2 passed
- [ ] Phase 4: Debate and revise Agent Plan with user (gate)
- [ ] Phase 5: Validate revised agent plan on 3 real user questions
- [ ] Phase 6: Description updates across all existing tools
- [ ] Phase 7: Decide and implement missing tools based on remaining pain points

---

## 6) Definition of Done

- [ ] The onion timing question can be answered in <= 3 MCP tool calls.
- [ ] Tool errors are deterministic and actionable.
- [ ] Tool descriptions prevent common misuse.
- [ ] Agent plan works for timing, rotation, and history questions.
- [ ] Remaining missing tools are explicitly justified with evidence.
