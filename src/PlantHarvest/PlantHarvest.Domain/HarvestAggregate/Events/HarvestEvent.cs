namespace PlantHarvest.Domain.HarvestAggregate.Events;


public record HarvestEvent : BaseDomainEvent
{
    public HarvestCycle? Harvest { get; init; }
    public HarvestEventTriggerEnum Trigger { get; init; }
    public TriggerEntity? TriggerEntity { get; init; }
    public string HarvestId { get { return Harvest!.Id; } init { } }
    public string UserProfileId { get { return Harvest!.UserProfileId; } init { } }

    private HarvestEvent() { }

    public HarvestEvent(HarvestCycle harvest, HarvestEventTriggerEnum trigger, TriggerEntity entity)
    {
        Harvest = harvest;
        Trigger = trigger;
        TriggerEntity = entity;
    }


}




