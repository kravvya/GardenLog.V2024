

namespace UserManagement.Api.Model;

public class UserProfile : BaseEntity, IAggregateRoot
{
    public string UserName { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string EmailAddress { get; private set; } = string.Empty;
    public string UserProfileId { get; private set; } = string.Empty;

    public DateTime UserProfileCreatedDateTimeUtc { get; private set; }

    private UserProfile() { }

    public static UserProfile Create(string userName, string firstName, string lastName, string emailAddress)
    {
        var user = new UserProfile()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = userName,
            FirstName = firstName,
            LastName = lastName,
            EmailAddress = emailAddress,
            UserProfileCreatedDateTimeUtc = DateTime.UtcNow
        };

        user.DomainEvents.Add(
                  new UserProfileEvent(user, UserProfileEventTriggerEnum.UserProfileCreated, new UserManagment.Api.Model.Meta.TriggerEntity(EntityTypeEnum.UserProfile, user.Id)));

        return user;
    }

    public void Update(string firstName, string lastName)
    {        

        this.Set<string>(() => this.FirstName, firstName);

        this.Set<string>(() => this.LastName, lastName);

    }

    public void SetUserProfileId(string identityId)
    {
        if (string.IsNullOrWhiteSpace(this.UserProfileId))
        {
            this.UserProfileId = identityId;
        }
        else
        {
            throw new ArgumentException("UserProfileId can not be modified", nameof(identityId));
        }
    }

    protected override void AddDomainEvent(string attributeName)
    {
        if (this.DomainEvents.Count == 0)
        {
            this.DomainEvents.Add(
                  new UserProfileEvent(this, UserProfileEventTriggerEnum.UserProfileUpdated, new UserManagment.Api.Model.Meta.TriggerEntity(EntityTypeEnum.UserProfile, this.Id)));
        }
    }
}

