using UserManagement.Contract.Command;

namespace GardenLogWeb.Services
{
    public interface IContactService
    {
        Task<ApiResponse> SendEmail(SendEmailCommand email);
    }

    public class ContactService : IContactService
    {
        private readonly ILogger<ContactService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IGardenLogToastService _toastService;


        public ContactService(ILogger<ContactService> logger, IHttpClientFactory clientFactory, IGardenLogToastService toastService)
        {
            _logger = logger;
            _httpClientFactory = clientFactory;
            _toastService = toastService;
        }

        public async Task<ApiResponse> SendEmail(SendEmailCommand email)
        {
            var httpClient = _httpClientFactory.CreateClient(GlobalConstants.USERMANAGEMENT_NO_AUTH);

            var response = await httpClient.ApiPostAsync(UserProfileRoutes.SendEmail, email);

            if (response.ValidationProblems != null)
            {
                _toastService.ShowToast($"Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
            }
            else if (!response.IsSuccess)
            {
                _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
            }
            else
            {
                _toastService.ShowToast($"Message received.", GardenLogToastLevel.Success);
            }

            return response;
        }



    }
}
