namespace PlantHarvest.Domain.HarvestAggregate.Events;

public record HarvestChildEvent : BaseDomainEvent
{
    public HarvestEventTriggerEnum Trigger { get; init; }
    public TriggerEntity TriggerEntity { get; init; }

    public HarvestChildEvent(HarvestEventTriggerEnum trigger, TriggerEntity entity)
    {
        Trigger = trigger;
        TriggerEntity = entity;
    }
}