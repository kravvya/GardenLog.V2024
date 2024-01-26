using UserManagement.Api.Data.ApiClients;

namespace UserManagement.CommandHandlers
{
    public interface IUserProfileCommandHandler
    {
        Task<string> CreateUserProfile(CreateUserProfileCommand request);
        Task<int> DeleteUserProfile(string id);
        Task<int> UpdateUserProfile(UpdateUserProfileCommand request);
    }

    public class UserProfileCommandHandler : IUserProfileCommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IAuth0ManagementApiClient _managementApiClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserProfileCommandHandler> _logger;

        public UserProfileCommandHandler(IUnitOfWork unitOfWork, IUserProfileRepository userProfileRepository, IAuth0ManagementApiClient managementApiClient, IHttpContextAccessor httpContextAccessor, ILogger<UserProfileCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _userProfileRepository = userProfileRepository;
            _managementApiClient = managementApiClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<string> CreateUserProfile(CreateUserProfileCommand request)
        {
            var oldUser = await _userProfileRepository.SearchForUserProfile(new SearchUserProfiles(request.UserName, request.EmailAddress));

            if (oldUser != null && oldUser.UserName.Equals(request.UserName))
                throw new ArgumentException("UserName already in use", nameof(request.UserName));

            if (oldUser != null && oldUser.EmailAddress.Equals(request.EmailAddress))
                throw new ArgumentException("This Email is already registered", nameof(request.UserName));


            var user = UserProfile.Create(request.UserName, request.FirstName, request.LastName, request.EmailAddress);

            //first - check if users already has Auth0 profile - test users
            var id = await _managementApiClient.ReadUserIdByUserNameAndEmail(request.UserName, request.EmailAddress);

            if (string.IsNullOrEmpty(id)) id = await _managementApiClient.CreateUser(request, user.Id);

            user.SetUserProfileId(id);

            _userProfileRepository.Add(user);

            await _unitOfWork.SaveChangesAsync();

            return user.UserProfileId;
        }

        public async Task<int> UpdateUserProfile(UpdateUserProfileCommand request)
        {
            var oldUser = await _userProfileRepository.SearchForUserProfile(new SearchUserProfiles(request.UserName, request.EmailAddress));

            if (oldUser != null && oldUser.UserName.Equals(request.UserName) && !oldUser.UserProfileId.Equals(request.UserProfileId))
                throw new ArgumentException("UserName already in use", nameof(request.UserName));

            if (oldUser != null && oldUser.EmailAddress.Equals(request.EmailAddress) && !oldUser.UserProfileId.Equals(request.UserProfileId))
                throw new ArgumentException("This Email is already registered", nameof(request.UserName));


            var user = await _userProfileRepository.GetByIdAsync(request.UserProfileId);
            if (user == null) return 0;

            user.Update(request.FirstName, request.LastName);

            await _managementApiClient.UpdateUser(request, user.UserProfileId);

            _userProfileRepository.Update(user);

            return await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> DeleteUserProfile(string id)
        {
            var user = await _userProfileRepository.GetByIdAsync(id);
            if (user == null) return 0;

            await _managementApiClient.DeleteUser(user.UserProfileId);

            _userProfileRepository.Delete(user.Id);

            return await _unitOfWork.SaveChangesAsync();
        }

    }
}
