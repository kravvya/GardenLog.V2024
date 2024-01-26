using GardenLog.SharedInfrastructure.Extensions;
using UserManagement.Contract;
using UserManagement.Contract.Command;
using UserManagement.Contract.Query;
using UserManagement.Contract.ViewModels;

namespace UserManagement.IntegrationTest.Clients;

public class UserProfileClient
{
    private readonly Uri _baseUrl;
    private readonly HttpClient _httpClient;

    public UserProfileClient(Uri baseUrl, HttpClient httpClient)
    {
        _baseUrl = baseUrl;
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> CreateUserProfile(string userName)
    {
        var url = $"{this._baseUrl.OriginalString}{UserProfileRoutes.CreateUserProfile}/";

        var createUserProfileCommand = PopulateCreateUserProfileCommand(userName);

        using var requestContent = createUserProfileCommand.ToJsonStringContent();

        return await this._httpClient.PostAsync(url, requestContent);

    }

    public async Task<HttpResponseMessage> UpdateUserProfile(UserProfileViewModel userProfile)
    {
        var url = $"{this._baseUrl.OriginalString}{UserProfileRoutes.UpdateUserProfile}";

        using var requestContent = userProfile.ToJsonStringContent();

        return await this._httpClient.PutAsync(url.Replace("{userProfileId}", userProfile.UserProfileId), requestContent);
    }

    public async Task<HttpResponseMessage> DeleteUserProfile(string id)
    {
        var url = $"{this._baseUrl.OriginalString}{UserProfileRoutes.DeleteUserProfile}";

        return await this._httpClient.DeleteAsync(url.Replace("{userProfileId}", id));
    }

    public async Task<HttpResponseMessage> GetUserProfile(string userName)
    {
        var url = $"{this._baseUrl.OriginalString}{UserProfileRoutes.SearchUserProfile}/";
        SearchUserProfiles search = new SearchUserProfiles(userName, string.Empty);
        using var requestContent = search.ToJsonStringContent();
        return await this._httpClient.PostAsync(url, requestContent);

    }

    public async Task<HttpResponseMessage> GetUserProfileById(string userPrifleId)
    {
        var url = $"{this._baseUrl.OriginalString}{UserProfileRoutes.GetUserProfileById}/";
            
        return await this._httpClient.GetAsync(url.Replace("{userProfileId}", userPrifleId));

    }

    public async Task<HttpResponseMessage> SendEmail()
    {
        var url = $"{this._baseUrl.OriginalString}{UserProfileRoutes.SendEmail}";

        using var requestContent = CreateEmailCommand().ToJsonStringContent();

        return await this._httpClient.PostAsync(url, requestContent);

    }

    private static SendEmailCommand CreateEmailCommand()
    {
        return new SendEmailCommand()
        {
            EmailAddress = "kravvya@chrobinson.com",
            Name = "AAT Tester",
            Subject = "Test from AAT",
            Message = $"This is only a test @ {DateTime.Now.ToLongTimeString()}"
        };
    }

    private static CreateUserProfileCommand PopulateCreateUserProfileCommand(string userName)
    {
        return new CreateUserProfileCommand()
        {
            UserName = userName,
            EmailAddress = $"{userName}@gardenlog.com",
            FirstName = "Test",
            LastName = "User",
            Password = "A12345678z"
        };
    }
}
