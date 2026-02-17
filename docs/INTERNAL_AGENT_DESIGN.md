# Internal AI Agent Design

## Overview

This document describes planned AI agent capabilities that will run **within** the GardenLog application to provide intelligent recommendations, warnings, and optimizations based on historical data and best practices. Unlike the MCP agent (which provides external AI assistants with read access to garden data), this internal agent will actively analyze user data and suggest improvements.

## Purpose

The date calculation system works well with the current flow:
1. **Plant GrowInstructions** define relative timing (e.g., "4 weeks before last frost")
2. **Garden weather dates** provide climate benchmarks (LastFrostDate, FirstFrostDate, WarmSoilDate)
3. **PlantCalendar schedules** are generated from templates + benchmarks
4. **PlantTask records** are created from schedules
5. **WorkLog entries** capture actual completion dates and outcomes

However, this system uses **fixed regional averages** for weather dates and **generic grow instructions** from seed catalogs. Over multiple growing seasons, **actual experience** reveals that adjusting these dates produces better results for specific gardens and microclimates.

The internal agent will bridge the gap between **planned dates** (template-driven) and **optimal dates** (experience-driven).

## Use Cases

### Use Case 1: Optimize Planting Dates Based on Historical Success

**Problem:**
- Plant grow instructions use generic timing rules (e.g., "start tomatoes indoors 6 weeks before last frost")
- Garden weather dates are regional averages that may not reflect microclimate conditions
- Through experience, gardeners discover that adjusting dates produces better outcomes
- Current system requires manual calculation and adjustment of each plant's schedule

**Current Data Flow:**
```
GrowInstruction Template → Garden Weather Dates → PlantCalendar (Planned Dates)
                                ↓
                         PlantTask Records
                                ↓
                         WorkLog (Actual Dates + Outcomes)
```

**Gap:**
No feedback loop from WorkLog back to PlantCalendar for future seasons. Successful adjustments are lost unless manually remembered.

**Agent Solution:**
The agent will analyze historical data to recommend date adjustments:

1. **Analyze Past Success:**
   - Query WorkLog entries filtered by plant type and Reason (SowIndoors, TransplantOutside, etc.)
   - Identify harvests with positive outcomes (high yields, good quality, successful germination)
   - Compare WorkLog.EventDateTime (actual dates) vs PlantCalendar scheduled dates
   - Calculate patterns: "When we started Peppers indoors 2 weeks earlier than planned, germination rate was 95% vs 70%"

2. **Generate Recommendations:**
   - When user creates new HarvestCycle and adds plants, agent analyzes:
     - PlantId + PlantGrowthInstructionId being used
     - Historical WorkLog for this plant in this garden
     - Planned dates generated from GrowInstruction + Garden weather dates
   - Agent suggests: "Based on your past 3 seasons, consider starting Peppers 2 weeks earlier (March 15 instead of March 29)"
   - Provides reasoning: "In 2023 and 2024, you started on March 12 and March 18 with 95% germination, vs 2022 on March 30 with 70% germination"

3. **Learn From Outcomes:**
   - Track PlantHarvestCycle metrics: GerminationRate, TotalWeightInPounds, TotalItems
   - Correlate with timing deviations from planned schedule
   - Build garden-specific adjustment recommendations over time

**Data Sources:**
- **PlantHarvestCycle:** Metrics (GerminationRate, yield data), PlantCalendar (planned dates)
- **WorkLog:** EventDateTime (actual dates), RelatedEntities linking to PlantHarvestCycle
- **Plant/GrowInstructions:** Original templates for comparison
- **Garden:** Weather dates and location for microclimate patterns

**Example Recommendation:**
```
🤖 Agent Suggestion: Optimize Tomato Planting

Analysis of your past 3 seasons shows:
- Template: Start indoors 6 weeks before last frost (April 3)
- Your successful plantings: March 20 (2024), March 18 (2023), March 25 (2022)
- Average: 11 days earlier than template

Recommendation: Start tomatoes indoors on March 23 this year
Confidence: High (based on 3 consistent seasons)

Historical outcomes when planted earlier:
- 2024 (March 20): 92% germination, 45 lbs harvested
- 2023 (March 18): 88% germination, 52 lbs harvested
- 2022 (March 25): 90% germination, 48 lbs harvested

Do you want to adjust the schedule? [Yes] [Not this year] [Never for this plant]
```

**Benefits:**
- **Personalized schedules** based on actual garden microclimate and practices
- **Continuous improvement** as more data accumulates
- **Retain knowledge** across seasons without manual tracking
- **Quantified confidence** based on number of historical examples
- **Explainable recommendations** showing the data that drives suggestions

---

### Use Case 2: Crop Rotation Validation and Warnings

**Problem:**
- Planting same plant families in same beds year after year depletes soil nutrients and increases pest/disease pressure
- Best practice: Rotate crops so same family not in same bed for 3-4 years
- Current system allows users to place any plant in any bed without warnings
- No visibility into what was planted in a bed historically when planning new season

**Plant Family Relationships:**
- Brassicas (cabbage family): Broccoli, Cauliflower, Cabbage, Brussels Sprouts, Kale, Mustard, Turnip, Rutabaga
- Solanaceae (nightshade family): Tomatoes, Peppers, Eggplant, Potatoes
- Cucurbits: Cucumbers, Squash, Melons, Pumpkins
- Legumes: Beans, Peas
- Alliums: Onions, Garlic, Leeks, Shallots

**Current Data Flow:**
```
User selects: HarvestCycle + PlantHarvestCycle + GardenBed assignment
                                ↓
                    GardenBedPlantHarvestCycle record created
                    (no validation against historical placements)
```

**Agent Solution:**
The agent will validate crop rotation and warn about violations:

1. **Historical Bed Analysis:**
   - When user assigns plant to garden bed, query GardenBedPlantHarvestCycle collection:
     - Filter by GardenBedId + UserProfileId
     - Look back 3 years (or 3 most recent HarvestCycles)
     - Get PlantIds planted in this bed historically
   - Check plant family relationships (stored in Plant catalog)
   - Identify rotation violations

2. **Warning Levels:**
   - **Critical (Red):** Same plant in same bed within 3 years
     - "⚠️ Rotation Warning: Tomatoes were grown in 'In Ground Right 1' in 2023 and 2024. Growing again in 2025 increases disease risk."
   - **Warning (Yellow):** Same family in same bed within 3 years
     - "⚠️ Rotation Advisory: Peppers (nightshade family) in 'In Ground Right 1'. This bed had Tomatoes (same family) in 2024."
   - **Info (Blue):** Good rotation detected
     - "✓ Good rotation: Beans (legumes) will replenish nitrogen after last year's Tomatoes."

3. **Alternative Bed Suggestions:**
   - Agent analyzes all available beds and suggests better placements
   - Example: "Consider 'In Ground Left 2' instead - last used for Onions in 2024 (different family, good rotation)"
   - Rank bed recommendations by rotation benefit

4. **Rotation Planning Assistant:**
   - When starting new HarvestCycle, agent can suggest optimal bed assignments for all plants
   - Takes into account:
     - Previous 3 years of bed usage
     - Plant family relationships
     - Companion planting benefits (e.g., legumes before heavy feeders)
     - Bed size constraints (plant spacing requirements)

**Data Sources:**
- **GardenBedPlantHarvestCycle:** Historical bed assignments (GardenBedId, PlantHarvestCycleId, start/end dates)
- **PlantHarvestCycle:** PlantId, PlantVarietyId, spacing/layout
- **Plant:** Plant family/type classification (needs enhancement - may need to add PlantFamily enum)
- **HarvestCycle:** Year/season context for 3-year lookback
- **Garden.GardenBeds:** Available beds and their dimensions

**Example Warning:**
```
⚠️ Crop Rotation Violation Detected

You're planning to grow Cauliflower in "In Ground Right 1"

History for this bed:
- 2024: Broccoli (Brassica family) ← SAME FAMILY
- 2023: Cabbage (Brassica family) ← SAME FAMILY  
- 2022: Tomatoes (Nightshade family)

Risk: Brassicas in same bed 3 years in a row
Problems:
- Depleted calcium and other nutrients
- Clubroot disease buildup in soil
- Increased flea beetle and cabbage moth pressure

Better bed options:
✓ "In Ground Left 2" - Last: Beans (2024), Squash (2023)
✓ "Raised Bed 3" - Last: Lettuce (2024), Carrots (2023)

[Change Bed] [Plant Anyway] [Learn More]
```

**Benefits:**
- **Prevent mistakes** before they impact harvest quality
- **Education** about plant families and rotation principles
- **Actionable suggestions** with specific alternative bed recommendations
- **Historical context** showing exactly what was planted where and when
- **Long-term soil health** by enforcing proven rotation practices

---

## Implementation Considerations

### Data Requirements

**Enhancements Needed:**
1. **Plant Family Classification:** Add PlantFamily enum or property to Plant catalog (Brassica, Solanaceae, Cucurbit, Legume, Allium, etc.)
2. **Outcome Metrics:** Ensure PlantHarvestCycle consistently captures GerminationRate, yield data, quality ratings
3. **Success Indicators:** Define what constitutes "successful" harvest (thresholds for metrics)

### Agent Architecture

**Options:**
1. **Rule-based expert system:** Start with hardcoded rules (rotation = 3 years, deviation > 7 days = significant)
2. **Machine learning model:** Train on historical data to predict optimal dates and rotation patterns (future enhancement)
3. **Hybrid approach:** Rules for rotation validation, ML for date optimization based on outcomes

**Timing:**
- **Real-time validation:** Run at point of data entry (when assigning beds, creating schedules)
- **Batch recommendations:** Generate suggestions when user creates new HarvestCycle
- **Background analysis:** Periodic jobs to pre-compute recommendations for dashboard

### User Experience

**Integration Points:**
1. **HarvestCycle Creation:** "Generate Optimized Schedule" button uses agent to analyze historical data
2. **Bed Assignment UI:** Live warnings when dragging plants to beds with rotation violations
3. **Dashboard:** "Agent Insights" panel showing recommendations for current season
4. **Schedule Editing:** Inline suggestions when adjusting PlantCalendar dates

**User Control:**
- Users can accept/reject recommendations
- Track acceptance rate to measure agent value
- Allow users to override with notes (e.g., "Rotating beds this year due to construction")

---

## Next Steps

1. **Prototype Use Case 1 (Date Optimization):**
   - Build query to analyze WorkLog vs PlantCalendar date deviations
   - Correlate with GerminationRate and yield outcomes
   - Create recommendation engine with confidence scoring

2. **Prototype Use Case 2 (Rotation Validation):**
   - Enhance Plant catalog with family classification
   - Build 3-year bed history query
   - Create warning system with severity levels

3. **User Testing:**
   - Beta test with experienced users who have 3+ years of data
   - Validate recommendation accuracy against their manual adjustments
   - Gather feedback on warning thresholds and UI integration

4. **Expand Use Cases:**
   - Companion planting suggestions
   - Weather-based task timing adjustments
   - Seed inventory management (reorder alerts)
   - Harvest prediction and succession planting
