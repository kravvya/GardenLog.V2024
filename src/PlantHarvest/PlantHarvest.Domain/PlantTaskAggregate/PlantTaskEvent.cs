



namespace PlantHarvest.Domain.PlantTaskAggregate.Events;

public record PlantTaskEvent : BaseDomainEvent
{
    public PlantTask? PlantTask { get; init; }
    public PlantTaskEventTriggerEnum Trigger { get; init; }
    public PlantTaskTriggerEntity? TriggerEntity { get; init; }
    public string PlantTaskId { get { return PlantTask!.Id; } init { } }
    public string UserProfileId { get { return PlantTask!.UserProfileId; } init { } }

    private PlantTaskEvent() { }

    public PlantTaskEvent(PlantTask task, PlantTaskEventTriggerEnum trigger, PlantTaskTriggerEntity entity)
    {
        PlantTask = task;
        Trigger = trigger;
        TriggerEntity = entity;
    }


}
