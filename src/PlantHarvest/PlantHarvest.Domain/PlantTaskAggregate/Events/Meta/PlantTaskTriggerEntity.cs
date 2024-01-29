namespace PlantHarvest.Domain.PlantTaskAggregate.Events.Meta;

public record PlantTaskTriggerEntity(PlantTaskEntityTypeEnum entityType, string entityId);