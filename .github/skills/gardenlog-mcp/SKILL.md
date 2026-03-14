---
name: gardenlog-mcp
description: 'Answer gardening questions using GardenLog MCP: when to seed/plant specific plants, full cycle seed planning, crop rotation advice, garden bed history.'
---

# GardenLog MCP Skill

This skill enables answering gardening questions by calling GardenLog MCP tools and analyzing historical data with quality signals.

## Question Types & Workflows

Identify which workflow to use based on the user's question:

| User Question Type | Use This Workflow |
|-------------------|------------------|
| "When should I seed [plant]?" | **Single Plant Seeding Analysis** |
| "When should I seed everything?" | **Full Cycle Seed Plan** |
| "Can I plant [plant] in this bed?" | **Crop Rotation Check** |

---

## Workflow 1: Single Plant Seeding Analysis

**Purpose:** Recommend optimal seeding date for a specific plant by analyzing baseline timing, planned dates, historical actuals, and outcome quality.

### Tool Call Sequence

1. **`search_plants`** → Get `plantId` from plant name
2. **`get_current_harvest_cycle`** → Get active `harvestCycleId`
3. **`get_plant_details`** → Get baseline grow instructions (e.g., "6 weeks before warm soil")
4. **`get_plant_schedule`** → Get current cycle's planned schedule for the specific plant
   - Returns ALL schedules (one per grow instruction)
   - For plants with multiple schedules (e.g., Radishes: spring + fall), filter by current date to pick relevant one
   - Check `plantGrowthInstructionName` to understand which season/method
   - Note: Does not include bed assignments. Use `get_harvest_cycle_plants_summary(includeVarieties=true, includeBeds=true)` if you need bed info
5. **`get_plant_harvest_cycles`** → Get historical cycle data (notes with quality signals, dates, germination rates) for the plant
6. **`get_worklog_history`** → Get historical actual seeding dates (use `reason: 'SowIndoors'` or `'SowOutside'`)

### Critical: Evaluate Outcome Quality Signals

From the `notes` field in `get_plant_harvest_cycles` results, classify each historical season:

**✅ Positive Signals** (prioritize these dates):
- Strong/high/excellent germination
- Good/large size, strong yield
- Positive outcome language

**❌ Negative Signals** (avoid these dates):
- Poor/low germination
- Small/weak size
- Context issues (shade, pests, bed conditions)

**⚠️ Neutral:**
- No notes or generic notes
- Lower confidence

### Recommendation Logic

- **With quality notes:** Weight toward dates with positive outcomes, avoid negative
- **Multiple positive results:** Use median/cluster center of successful dates
- **Sparse history:** Fallback to planned date
- **No active cycle:** Show baseline timing + suggest planning

### Confidence Levels

- **High:** 3+ seasons, consistent dates, positive outcomes
- **Medium:** 2 seasons OR mixed outcomes OR some negative signals
- **Low:** 1 season only OR inconsistent OR negative outcomes dominate

### Output Format

```
**Planned Date:** [Date from plantCalendar]

**Historical Actual Dates:**
- [Year]: [Date] ([Days difference from planned])
- [Year]: [Date] ([Days difference from planned])

**Outcome Quality Analysis:**
- [Year] ([Date]): [Outcome summary with ✅/❌/⚠️]

**Recommended Date:** [Calculated recommendation]
**Confidence:** [High/Medium/Low]

**Rationale:** [1-2 sentences: baseline timing, historical pattern, quality signals, warnings]
```

---

## Workflow 2: Full Cycle Seed Plan

**Purpose:** Analyze all plants in the current cycle, compare planned vs. recommended dates, flag discrepancies.

### Tool Call Sequence

1. **`get_current_harvest_cycle`** → Get active `harvestCycleId`
2. **`get_harvest_cycle_plants_summary`** → Enumerate all plants in cycle (lightweight list with optional varieties/beds)
   - Use `includeVarieties=true` if you need variety-level recommendations
   - Use `includeBeds=true` AND `includeVarieties=true` to see bed assignments (beds are associated with varieties)
   - Use `includeBeds=false` if you only need plant/variety list
3. **For each plant** (or selected plants):
   - **`get_plant_schedule`** → Get planned seeding schedule for current cycle (use `plantName` parameter)
   - **`get_plant_harvest_cycles`** → Get historical cycle data with quality signals (2-3 years)
   - **`get_worklog_history`** → Get historical actual seeding dates (2-3 years)
   - Evaluate quality signals from notes
   - Calculate recommended date
   - Compare: `delta = recommended - planned`

### Comparison Classification

- **Match:** `|delta| ≤ 2 days` ✅
- **Different:** `|delta| > 2 days` ⚠️
  - Earlier: negative delta (e.g., -5 days)
  - Later: positive delta (e.g., +3 days)

### Output Format

```
## Seed Plan Analysis for [Cycle Name]

**Summary:**
- Total plants: [N]
- Matches (±2 days): [N]
- Need adjustment: [N]

| Plant | Planned | Recommended | Difference | Status | Rationale |
|-------|---------|-------------|------------|--------|-----------|
| Tomatoes | Mar 15 | Mar 20 | +5 days | Different | 3yrs @ Mar 18-22, 92% germ |
| Onions | Feb 27 | Feb 25 | -2 days | Match | 3yrs consistent, 100% germ |

**Plants Needing Attention:**
[Detailed recommendations for plants with |delta| > 2 days]
```

---

## Workflow 3: Crop Rotation Check

**Purpose:** Validate that a plant isn't being placed in a bed where the same plant/family was grown within 3 years.

### Plant Family Reference

- **Brassicas:** Broccoli, Cauliflower, Cabbage, Kale, Radish, Turnip
- **Solanaceae:** Tomatoes, Peppers, Eggplant, Potatoes, Tomatillos  
- **Cucurbits:** Cucumbers, Squash, Zucchini, Melons, Pumpkins
- **Legumes:** Beans, Peas, Lentils
- **Alliums:** Onions, Garlic, Leeks, Shallots, Chives
- **Apiaceae:** Carrots, Parsnips, Celery, Parsley, Cilantro
- **Chenopodiaceae:** Beets, Chard, Spinach
- **Asteraceae:** Lettuce, Endive, Artichoke

### Tool Call Sequence

1. **`search_plants`** → Get `plantId`, infer plant family from name
2. **`get_current_harvest_cycle`** → Get active `harvestCycleId` and `gardenId`
3. **`get_plant_schedule`** → Get plant's current schedule to find assigned `gardenBedId` from bed placements
   - **If no bed assigned:** Stop - inform user to assign bed first
4. **`get_garden_bed_history`** → Get 3-year history for the bed (use `startDate` = 3 years ago)
5. **Classify plant families** for all historical plants + target plant
6. **Detect violations:**
   - ❌ **Critical:** Same exact plant in bed within 3 years
   - ⚠️ **Warning:** Same family in bed within 3 years
   - ✅ **Good:** No family overlap in 3 years

### Output Format

```
## Crop Rotation Analysis: [Plant] in [Bed Name]

**Plant:** [Name] ([Family])
**Bed:** [Bed Name]
**Current Cycle:** [Year]

**Bed History (3 years):**
- [Plant] ([Family], [Year]) ← [SAME PLANT/SAME FAMILY/Different]
- [Plant] ([Family], [Year])

**Risk:** [Description if violation detected]

**Problems:**
- [Disease/pest risks]
- [Nutrient depletion concerns]

**Recommendations:**
1. [Specific actions: amend soil, monitor, consider different bed]
```

---

## Available MCP Tools

### Core Discovery Tools

| Tool | Purpose | Key Parameters | Returns |
|------|---------|----------------|---------|
| `search_plants` | Find plant by name/description | `searchText` (required) | Plant matches with `plantId`, `plantName`, `description` |
| `get_plant_details` | Get baseline grow instructions | `plantId` (required) | Timing guidelines, spacing, germination, botanical info |
| `get_current_harvest_cycle` | Get active planning cycle | None | Current `harvestCycleId`, cycle name, start/end dates, garden info |

### Current Cycle Tools (Lightweight)

| Tool | Purpose | Key Parameters | Returns |
|------|---------|----------------|---------|
| `get_harvest_cycle_plants_summary` | List all plants in cycle | `harvestCycleId` (required), `includeVarieties` (bool), `includeBeds` (bool) | Lightweight plant list (~5-10KB). Note: Bed data requires BOTH `includeVarieties=true` AND `includeBeds=true` (beds are variety-specific). |
| `get_plant_schedule` | Get ALL schedules for ONE plant | `plantName` OR `plantHarvestCycleId`, `harvestCycleId` (required) | ALL planned schedules (one per grow instruction). For plants with multiple schedules (spring/fall), all returned so AI can pick relevant one by current date. Does not include bed assignments. |

### Historical Analysis Tools

| Tool | Purpose | Key Parameters | Returns |
|------|---------|----------------|---------|
| `get_plant_harvest_cycles` | Historical cycles for ONE plant | `plantName` (required), `startDate` (optional), `endDate` (optional) | Past cycles with quality notes, germination, simplified bed info |
| `get_worklog_history` | Get actual completed tasks | `reason` (e.g., "SowIndoors"), `startDate`, `endDate` | Historical actual dates |

### Garden & Crop Rotation Tools

| Tool | Purpose | Key Parameters | Returns |
|------|---------|----------------|---------|
| `get_garden_details` | Garden and bed info | `gardenId` (required) | Garden metadata, bed list (NO coordinates/layout) |
| `get_garden_bed_history` | Bed planting history | `gardenBedId` (required), `startDate` (optional) | What was grown in bed (NO variety/layout details) |

---

## Edge Cases

### No Current Cycle
- Show baseline timing from grow instructions
- Suggest: "For your next cycle, consider..."

### No Historical Data
- Use planned date from current cycle
- Note: "First time growing - follow planned schedule"
- Confidence: Low

### Conflicting Data
- Call out explicitly: "Early seeding worked in sunny beds but not in shade"

### Multiple Varieties
- If different seeding dates: show range or breakdown by variety
- If same dates: group as single recommendation

### Plants with Multiple Growing Seasons
- **Example:** Radishes have spring and fall schedules
- **Solution:** `get_plant_schedule` returns ALL schedules (one per grow instruction)
- **AI Action:** Filter by current date:
  - If asking "what to seed next" in March → Use spring schedule (past dates in fall schedule should be ignored)
  - If asking "what to seed next" in August → Use fall schedule (past dates in spring schedule should be ignored)
- **Pattern:** Check `plantGrowthInstructionName` (e.g., "Direct seed in early spring" vs "Direct seed in late summer")

---

## Authentication

GardenLog MCP requires Auth0 JWT authentication (automatically configured). All tools return user-scoped data.
