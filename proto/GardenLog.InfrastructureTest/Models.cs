using PlantHarvest.Contract.Enum;

namespace GardenLog.InfrastructureTest;

public abstract class BaseEntity
{
    public string Id { get; set; } = string.Empty;

}

public class HarvestCycle : BaseEntity
{
    public string HarvestCycleName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string UserProfileId { get; set; } = string.Empty;
    public string GardenId { get; set; } = string.Empty;
    public List<PlantHarvestCycle> Plants = new();
}

public class PlantHarvestCycle : BaseEntity
{
    public string HarvestCycleId { get; set; } = string.Empty;

    public string PlantId { get; set; } = string.Empty;
    public string PlantName { get; set; } = string.Empty;

    public string? PlantVarietyId { get; set; }
    public string? PlantVarietyName { get; set; }

    public string? PlantGrowthInstructionId { get; set; }
    public string? PlantGrowthInstructionName { get; set; }
    public PlantingMethodEnum PlantingMethod { get; set; }

    public int? NumberOfSeeds { get; set; }
    public string? SeedCompanyId { get; set; }
    public string? SeedCompanyName { get; set; }

    public DateTime? SeedingDate { get; set; }

    public DateTime? GerminationDate { get; set; }
    public decimal? GerminationRate { get; set; }

    public int? NumberOfTransplants { get; set; }
    public DateTime? TransplantDate { get; set; }

    public DateTime? FirstHarvestDate { get; set; }
    public DateTime? LastHarvestDate { get; set; }

    public decimal? TotalWeightInPounds { get; set; }
    public int? TotalItems { get; set; }

    public string Notes { get; set; } = string.Empty;
    public int? DesiredNumberOfPlants { get; set; }
    public int? SpacingInInches { get; set; }
    public double? PlantsPerFoot { get; set; }

    public List<PlantSchedule> PlantCalendar = new();

    public List<GardenBedPlantHarvestCycle> GardenBedLayout = new();

}

public class PlantSchedule : BaseEntity
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public WorkLogReasonEnum TaskType { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsSystemGenerated { get; set; }
}


public class GardenBedPlantHarvestCycle : BaseEntity
{
    public string HarvestCycleId { get; set; } = string.Empty;
    public string PlantHarvestCycleId { get;  set; } = string.Empty;
    public string PlantId { get;  set; } = string.Empty;
    public string PlantName { get;  set; } = string.Empty;
    public string PlantVarietyId { get;  set; } = string.Empty;
    public string PlantVarietyName { get;  set; } = string.Empty;
    public string GardenId { get;  set; } = string.Empty;
    public string GardenBedId { get;  set; } = string.Empty;
    public int NumberOfPlants { get;  set; }
    public double PlantsPerFoot { get;  set; }

    public double X { get;  set; }
    public double Y { get;  set; }
    public double Length { get;  set; }
    public double Width { get;  set; }

    public double PatternWidth { get;  set; }
    public double PatternLength { get;  set; }
    public DateTime? StartDate { get;  set; }
    public DateTime? EndDate { get; set; }
}

public class HarvestCycleDocument : BaseEntity
{
    public string HarvestCycleName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string UserProfileId { get; set; } = string.Empty;
    public string GardenId { get; set; } = string.Empty;
}

public class PlantHarvestCycleDocument : BaseEntity
{
    public string HarvestCycleId { get; set; } = string.Empty;

    public string PlantId { get; set; } = string.Empty;
    public string PlantName { get; set; } = string.Empty;

    public string? PlantVarietyId { get; set; }
    public string? PlantVarietyName { get; set; }

    public string? PlantGrowthInstructionId { get; set; }
    public string? PlantGrowthInstructionName { get; set; }
    public PlantingMethodEnum PlantingMethod { get; set; }

    public int? NumberOfSeeds { get; set; }
    public string? SeedCompanyId { get; set; }
    public string? SeedCompanyName { get; set; }

    public DateTime? SeedingDate { get; set; }

    public DateTime? GerminationDate { get; set; }
    public decimal? GerminationRate { get; set; }

    public int? NumberOfTransplants { get; set; }
    public DateTime? TransplantDate { get; set; }

    public DateTime? FirstHarvestDate { get; set; }
    public DateTime? LastHarvestDate { get; set; }

    public decimal? TotalWeightInPounds { get; set; }
    public int? TotalItems { get; set; }

    public string Notes { get; set; } = string.Empty;
    public int? DesiredNumberOfPlants { get; set; }
    public int? SpacingInInches { get; set; }
    public double? PlantsPerFoot { get; set; }

    public List<PlantSchedule> PlantCalendar = new();


}