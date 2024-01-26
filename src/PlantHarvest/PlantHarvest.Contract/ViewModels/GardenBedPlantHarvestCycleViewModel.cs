﻿namespace PlantHarvest.Contract.ViewModels;

public record GardenBedPlantHarvestCycleViewModel :GardenBedPlantHarvestCycleBase
{
    public string GardenBedPlantHarvestCycleId { get; set; } = string.Empty;
}

public class GardenBedPlantHarvestCycleViewModelValidator : GardenBedPlantHarvestCycleValidator<GardenBedPlantHarvestCycleViewModel>
{
    public GardenBedPlantHarvestCycleViewModelValidator()
    {
    }
}