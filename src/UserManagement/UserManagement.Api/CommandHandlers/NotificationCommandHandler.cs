using UserManagement.Api.Data.ApiClients;

namespace UserManagement.CommandHandlers
{
    public interface INotificationCommandHandler
    {
        Task<bool> PublishPastDueTasks();
        Task<bool> PublishWeeklyTasks();
    }

    public class NotificationCommandHandler : INotificationCommandHandler
    {

        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IPlantHarvestApiClient _harvestApiClient;
        private readonly IEmailClient _emailClient;
        private readonly ILogger<NotificationCommandHandler> _logger;

        public NotificationCommandHandler(IUserProfileRepository userProfileRepository, IPlantHarvestApiClient harvestApiClient, IEmailClient emailClient, ILogger<NotificationCommandHandler> logger)
        {
            _userProfileRepository = userProfileRepository;
            _harvestApiClient = harvestApiClient;
            _emailClient = emailClient;
            _logger = logger;
        }

        public async Task<bool> PublishWeeklyTasks()
        {
            var users = await _userProfileRepository.GetAllUserProfiles();

            foreach (var user in users)
            {
                if (IsInValidUser(user)) continue;

                var tasks = await _harvestApiClient.GetTasks(user.UserProfileId, false);

                if (string.IsNullOrWhiteSpace(tasks)) continue;

                SendEmailCommand request = new()
                {
                    EmailAddress = user.EmailAddress,
                    Name = $"{user.FirstName} {user.LastName}",
                    Subject = "Weekly tasks",
                    Message = tasks
                };

                await _emailClient.SendEmailToUser(request);
            }

            return true;
        }

        public async Task<bool> PublishPastDueTasks()
        {
            var users = await _userProfileRepository.GetAllUserProfiles();

            foreach (var user in users)
            {
                if (IsInValidUser(user)) continue;

                var tasks = await _harvestApiClient.GetTasks(user.UserProfileId, true);

                if (string.IsNullOrWhiteSpace(tasks)) continue;

                SendEmailCommand request = new()
                {
                    EmailAddress = user.EmailAddress,
                    Name = $"{user.FirstName} {user.LastName}",
                    Subject = "Past Due tasks",
                    Message = tasks
                };

                await _emailClient.SendEmailToUser(request);
            }

            return true;
        }

       private bool IsInValidUser(UserProfileViewModel user)
        {
            return string.IsNullOrWhiteSpace(user.EmailAddress) || string.IsNullOrWhiteSpace(user.FirstName)
                    || string.IsNullOrWhiteSpace(user.LastName) || user.FirstName.Contains("Test") || user.LastName.Contains("Tester");
        }
    }
}
