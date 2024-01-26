using GardenLog.SharedInfrastructure.Extensions;

namespace UserManagement.QueryHandlers;

public interface IUserProfileQueryHandler
{
    Task<UserProfileViewModel> GetUserProfile();
    Task<UserProfileViewModel> GetUserProfile(string userProfileId);
    Task<UserProfileViewModel> SearchForUserProfile(SearchUserProfiles searchCriteria);
}

public class UserProfileQueryHandler : IUserProfileQueryHandler
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserProfileQueryHandler(IUserProfileRepository userProfileRepository, IHttpContextAccessor httpContextAccessor)
    {
        _userProfileRepository = userProfileRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<UserProfileViewModel> GetUserProfile() => _userProfileRepository.GetUserProfile(_httpContextAccessor.HttpContext!.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers))!;

    public Task<UserProfileViewModel> GetUserProfile(string userProfileId) => _userProfileRepository.GetUserProfile(userProfileId);

    public Task<UserProfileViewModel> SearchForUserProfile(SearchUserProfiles searchCriteria) => _userProfileRepository.SearchForUserProfile(searchCriteria);
}
