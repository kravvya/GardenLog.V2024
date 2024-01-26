namespace UserManagement.Api.Model;

public record GardenChildEvent : BaseDomainEvent
{
    public UserProfileEventTriggerEnum Trigger { get; init; }
    public UserManagment.Api.Model.Meta.TriggerEntity TriggerEntity { get; init; }

    public GardenChildEvent(UserProfileEventTriggerEnum trigger, UserManagment.Api.Model.Meta.TriggerEntity entity)
    {
        Trigger = trigger;
        TriggerEntity = entity;
    }
}