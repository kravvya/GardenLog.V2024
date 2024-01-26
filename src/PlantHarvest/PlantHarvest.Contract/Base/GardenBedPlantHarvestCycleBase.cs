namespace PlantHarvest.Contract.Base;

public abstract record GardenBedPlantHarvestCycleBase
{
    public string HarvestCycleId { get; set; }=string.Empty;
    public string PlantHarvestCycleId { get; set; } = string.Empty;
    public string PlantId { get; set; } = string.Empty;
    public string PlantName { get; set; } = string.Empty;
    public string? PlantVarietyId { get; set; }
    public string? PlantVarietyName { get; set; }
    public string GardenId { get; set; } = string.Empty;
    public string GardenBedId { get; set; } = string.Empty;
    public int NumberOfPlants { get; set; }
    public double PlantsPerFoot { get; set; }

    public double X { get; set; }
    public double Y { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }

    public double PatternWidth { get; set; }
    public double PatternLength { get; set; }
}
