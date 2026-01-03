using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using UserManagement.Api.Model;
using UserManagement.Contract.ViewModels;

namespace UserManagement.IntegrationTest;

#pragma warning disable xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
public partial class GardenTests // : IClassFixture<UserManagementServiceFixture>
#pragma warning restore xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
{
    public const string TEST_USER = "TestUser";
    public const string TEST_DELETE_USER = "TestDeleteUser";

    #region User Profile
    [Fact]
    public async Task Post_UserProfile_CreateNew_UserProfile()
    {
        var response = await _userProfileClient.CreateUserProfile(TEST_USER);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to create user profile responded with {response.StatusCode} code and {returnString} message");

        if (response.IsSuccessStatusCode)
        {
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotEmpty(returnString);
            Assert.Contains("auth0|", returnString);
            returnString = returnString.Replace("auth0|", "");
            Assert.True(Guid.TryParse(returnString, out _));
        }
        else
        {
            Assert.Contains("UserName already in use", returnString);
        }
    }

    [Fact]
    public async Task Post_UserProfile_ShouldNotCreateNewUserProfile_WithoutUserName()
    {
        var response = await _userProfileClient.CreateUserProfile(string.Empty);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to create user profile responded with {response.StatusCode} code and {returnString} message");


        Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        Assert.NotEmpty(returnString);
        Assert.Contains("'User Name' must not be empty.", returnString);
    }

    [Fact]
    public async Task Put_UserProfile_ShouldUpdateUserProfile()
    {
        var userProfile = await GetUserProfileToWorkWith(TEST_USER);

        userProfile.LastName = userProfile.FirstName;
        userProfile.FirstName = userProfile.LastName == "Test"? "User" : "Test";

        var response = await _userProfileClient.UpdateUserProfile(userProfile);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to update user profile responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
    }

    [Fact]
    public async Task Delete_UserProfile_ShouldDelete()
    {
        var userProfile = await GetUserProfileToWorkWith(TEST_DELETE_USER);
        if (userProfile == null)
        {
            await _userProfileClient.CreateUserProfile(TEST_DELETE_USER);
        }

        userProfile = await GetUserProfileToWorkWith(TEST_DELETE_USER);

        var response = await _userProfileClient.DeleteUserProfile(userProfile.UserProfileId);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to delete user profile responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
    }

    #endregion

    #region  Contact

    [Fact]
    public async Task Email_Should_Send()
    {

        var response = await _userProfileClient.SendEmail();

        _output.WriteLine($"Service to send email responded with {response.StatusCode} code");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);

    }

    #endregion

    private async Task<UserProfileViewModel> GetUserProfileToWorkWith(string userName)
    {
        var response = await _userProfileClient.GetUserProfile(userName);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                {
                    new JsonStringEnumConverter(),
                },
        };

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to get user responded with {response.StatusCode} code and {returnString} message");

        UserProfileViewModel? userProfile = null;
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            userProfile = await response.Content.ReadFromJsonAsync<UserProfileViewModel>(options);
        }

        return userProfile!;
    }
}
