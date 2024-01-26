using UserManagement.Contract.Base;

namespace UserManagement.Contract.ViewModels;

public record UserProfileViewModel :UserProfileBase
{
    public string UserProfileId { get; set; }=string.Empty;
    public DateTime UserProfileCreatedDateTimeUtc { get;  set; }

}
