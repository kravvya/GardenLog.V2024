namespace PlantHarvest.Domain.WorkLogAggregate.Events;

public record WorkLogEvent : BaseDomainEvent
{
    public WorkLog? Work { get; init; }
    public WorkLogEventTriggerEnum Trigger { get; init; }
    public RelatedEntity? TriggerEntity { get; init; }
    public string WorkLogId { get { return Work!.Id; } init { } }
    public string UserProfileId { get { return Work!.UserProfileId; } init { } }

    private WorkLogEvent() { }

    public WorkLogEvent(WorkLog work, WorkLogEventTriggerEnum trigger, RelatedEntity triggerEntity)
    {
        Work = work;
        Trigger = trigger;
        TriggerEntity = triggerEntity;
    }


}
