namespace UserManagement.Api.Model;

public record UserProfileEvent : BaseDomainEvent
{
    public UserProfile? UserProfile { get; init; }
    public UserProfileEventTriggerEnum Trigger { get; init; }
    public TriggerEntity? TriggerEntity { get; init; }
    public string UserProfileId { get { return UserProfile!.Id; } init { } }
    private UserProfileEvent() { }

    public UserProfileEvent(UserProfile userProfile, UserProfileEventTriggerEnum trigger, UserManagment.Api.Model.Meta.TriggerEntity entity)
    {
        UserProfile = userProfile;
        Trigger = trigger;
        TriggerEntity = entity;
    }

}
