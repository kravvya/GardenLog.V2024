namespace PlantHarvest.Domain.HarvestAggregate.Events.Meta;

public enum HarvestEventTriggerEnum
{
    HarvestCycleCreated = 1,
    HarvestCycleUpdated = 2,
    HarvestCycleDeleted = 3,
    PlantAddedToHarvestCycle = 4,
    PlantHarvestCycleUpdated = 5,
    PlantHarvestCycleDeleted = 6,
    PlantScheduleCreated = 7,
    PlantScheduleUpdated = 8,
    PlantScheduleDeleted = 9,
    PlantHarvestCycleSeeded = 10,
    PlantHarvestCycleGerminated = 11,
    PlantHarvestCycleTransplanted = 12,
    PlantHarvestCycleHarvested = 13,
    PlantHarvestCycleCompleted = 14,
    GardenBedPlantHarvestCycleCreated = 15,
    GardenBedPlantHarvestCycleUpdated = 16,
    GardenBedPlantHarvestCycleDeleted = 17,
    HarvestCycleCompleted = 18,
    PlantingMethodChanged = 19
}
