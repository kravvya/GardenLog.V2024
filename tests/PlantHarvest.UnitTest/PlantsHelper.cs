using PlantCatalog.Contract.ViewModels;

namespace PlantHarvest.UnitTest;

internal class PlantsHelper
{

    public static string PLANT_ID = "TestPlantId";
    public static string PLANT_VARIETY_ID = "TestPlantVarietyId";
    public static string GROW_INSTRUCTION_ID = "GrowInstructionId";

    public static string GetGrowInstruction(PlantCatalog.Contract.Enum.PlantingMethodEnum plantingMethod)
    {
        var grow = new PlantGrowInstructionViewModel()
        {
            PlantingMethod = plantingMethod,

            PlantGrowInstructionId = GROW_INSTRUCTION_ID,
            Name = "Start Seed Indoors",
            PlantId = PLANT_ID,

            StartSeedInstructions = "Start Seeding Instrunctions",
            StartSeedWeeksAheadOfWeatherCondition = 15,
            StartSeedWeeksRange = 1,
            StartSeedAheadOfWeatherCondition = WeatherConditionEnum.BeforeLastFrost,
            FertilizerFrequencyForSeedlingsInWeeks = 2,
            FertilizerForSeedlings = FertilizerEnum.HalfBalanced,

            DaysToSproutMin = 7,
            DaysToSproutMax = 14,

            TransplantAheadOfWeatherCondition = WeatherConditionEnum.EarlySpring,
            TransplantWeeksAheadOfWeatherCondition = 0,
            TransplantWeeksRange = 2,
            TransplantInstructions = "Transplant Instructions",
            FertilizerAtPlanting = FertilizerEnum.Starter,

            FertilizeFrequencyInWeeks = 6,
            Fertilizer = FertilizerEnum.Balanced,

            GrowingInstructions = "Grow Instructions",

            HarvestInstructions = "Harvest Instructions",

            HarvestSeason = HarvestSeasonEnum.Summer,
            PlantingDepthInInches = PlantingDepthEnum.Depth1,

            SpacingInInches = 10
        };

        return JsonSerializer.Serialize(grow, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static string GetPlantVariety()
    {
        var variety = new PlantVarietyViewModel()
        {
            Name = "Test Variety",
            Title = "Test Title",
            PlantVarietyId = PLANT_VARIETY_ID,
            Colors = new() { "Black" },
            DaysToMaturityMax = 110,
            DaysToMaturityMin = 120,
            Description = "Description",
            GrowTolerance = GrowToleranceEnum.LightFrost,
            HeightInInches = 12,
            IsHeirloom = true,
            LightRequirement = LightRequirementEnum.FullShade,
            MoistureRequirement = MoistureRequirementEnum.InchPerWeek,
            PlantId = PLANT_ID,
            Sources = new() { "Book" },
            Tags = new() { "Tag" }
        };

        return JsonSerializer.Serialize(variety, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }


    public static string GetPlant()
    {
        var plant = new PlantViewModel()
        {
            Name = "Test Variety",
            DaysToMaturityMax = 110,
            DaysToMaturityMin = 120,
            Description = "Description",
            GrowTolerance = GrowToleranceEnum.LightFrost,
           LightRequirement = LightRequirementEnum.FullShade,
            MoistureRequirement = MoistureRequirementEnum.InchPerWeek,
            PlantId = PLANT_ID,
            Tags = new() { "Tag" }
        };

        return JsonSerializer.Serialize(plant, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}
