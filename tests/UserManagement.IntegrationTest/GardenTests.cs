using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using UserManagement.Contract.Base;
using UserManagement.Contract.ViewModels;
using UserManagement.IntegrationTest.Clients;
using UserManagement.IntegrationTest.Fixture;
using Xunit.Abstractions;

namespace UserManagement.IntegrationTest;

public partial class GardenTests : IClassFixture<UserManagementServiceFixture>
{

    private readonly ITestOutputHelper _output;
    private readonly GardenClient _gardenClient;
    private readonly UserProfileClient _userProfileClient;

    public const string TEST_GARDEN_NAME = "Test Garden";
    private const string TEST_GARDEN_BED_NAME = "Test Garden Bed";

    private const string TEST_DELETE_GARDEN_BED_NAME = "Delete Garden Bed";
    private const string TEST_DELETE_GARDEN_NAME = "Delete Garden";

    public GardenTests(UserManagementServiceFixture fixture, ITestOutputHelper output)
    {
        _gardenClient = fixture.GardenClient;
        _userProfileClient = fixture.UserProfileClient;
        _output = output;
        _output.WriteLine($"Service id {fixture.FixtureId} @ {DateTime.Now:F}");
    }

    #region Garden
    [Fact]
    public async Task Post_Garden_MayCreateNewGarden()
    {
        var response = await _gardenClient.CreateGarden(TEST_GARDEN_NAME);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to create garden responded with {response.StatusCode} code and {returnString} message");

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            Assert.NotEmpty(returnString);
            Assert.True(Guid.TryParse(returnString, out _));
        }
        else
        {
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotEmpty(returnString);
            Assert.Contains("Garden with this name already exists", returnString);
        }
    }

    [Fact]
    public async Task Post_Garden_ShouldNotCreateNewGarden_WithoutName()
    {
        var response = await _gardenClient.CreateGarden("");

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotEmpty(returnString);
        Assert.Contains("'Name' must not be empty.", returnString);
    }

    [Fact]
    public async Task Put_Garden_ShouldUpdateGarden()
    {
        var garden = await GetGardenToWorkWith(TEST_GARDEN_NAME);

        garden.Notes = $"{garden.Notes} last updated: {DateTime.Now}";

        var response = await _gardenClient.UpdateGarden(garden);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to update garden responded with {response.StatusCode} code and {returnString} message");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(returnString);
    }

    [Fact]
    public async Task Delete_Garden_ShouldDelete()
    {
        await _gardenClient.CreateGarden(TEST_DELETE_GARDEN_NAME);

        var gardenId = await GetGardenIdToWorkWith(TEST_DELETE_GARDEN_NAME);

        var response = await _gardenClient.DeleteGarden(gardenId);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to delete garden responded with {response.StatusCode} code and {returnString} message");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(returnString);
    }

    [Fact]
    public async Task Get_Should_Return_All_Gardens()
    {
        var response = await _gardenClient.GetAllGardens();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                {
                    new JsonStringEnumConverter(),
                },
        };

        var returnString = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Service for gardens responded with {response.StatusCode} code and {returnString} message");

        var gardens = await response.Content.ReadFromJsonAsync<List<GardenViewModel>>(options);

        Assert.NotNull(gardens);
        Assert.NotEmpty(gardens);

        var testHarvest = gardens.FirstOrDefault(plants => plants.Name == TEST_GARDEN_NAME);

        Assert.NotNull(testHarvest);
    }

    [Fact]
    public async Task Get_Should_Return_Garden()
    {
        var garden = await GetGardenToWorkWith(TEST_GARDEN_NAME);

        Assert.NotNull(garden);
        Assert.Equal(TEST_GARDEN_NAME, garden.Name);
    }

    #endregion

    #region Garden Bed
    [Fact]
    public async Task Post_GardenBed_MayCreateNew()
    {
        var gardenId = await GetGardenIdToWorkWith(TEST_GARDEN_NAME);

        var original = await GetGardenBedToWorkWith(gardenId, TEST_GARDEN_BED_NAME);

        if (original != null)
        {
            _ = await _gardenClient.DeleteGardenBed(gardenId, original.GardenBedId);
        }

        var gardenBedId = await CreateGardenBedIdToWorkWith(gardenId, TEST_GARDEN_BED_NAME);
        Assert.NotNull(gardenBedId);
    }


    [Fact]
    public async Task Get_GardenBed_ByGardenId()
    {
        var gardenId = await GetGardenIdToWorkWith(TEST_GARDEN_NAME);
        var gardenBed = await GetGardenBedToWorkWith(gardenId, TEST_GARDEN_BED_NAME);

        Assert.NotNull(gardenBed);
        _output.WriteLine($"Found '{gardenBed.GardenBedId}' Garden Bed");
        Assert.NotEmpty(gardenBed.GardenBedId);
    }

    [Fact]
    public async Task Get_GardenBed_One()
    {
        var gardenId = await GetGardenIdToWorkWith(TEST_GARDEN_NAME);
        var gardenBeds = await GetGardenBedsToWorkWith(gardenId);

        var original = gardenBeds.First(g => g.Name == TEST_GARDEN_BED_NAME);

        if (gardenBeds.Count == 1)
        {
            //create new Garden to make sure we only get one back
            var response = await _gardenClient.CreateGardenBed(gardenId, TEST_DELETE_GARDEN_BED_NAME);
        }

        var gardenBed = await GetGardenBedToWorkWith(gardenId, TEST_GARDEN_BED_NAME);

        _output.WriteLine($"Found '{gardenBed.GardenBedId}' Garden Bed");
        Assert.Equal(original.GardenBedId, gardenBed.GardenBedId);
    }

    [Fact]
    public async Task Get_GardenBed_All()
    {
        var gardenId = await GetGardenIdToWorkWith(TEST_GARDEN_NAME);
        var gardenBeds = await GetGardenBedsToWorkWith(gardenId);


        Assert.NotEmpty(gardenBeds);
    }

    [Fact]
    public async Task Put_GardenBed_ShouldUpdate()
    {
        var harvestId = await GetGardenIdToWorkWith(TEST_GARDEN_NAME);
        var gardenBed = await GetGardenBedToWorkWith(harvestId, TEST_GARDEN_BED_NAME);


        gardenBed.Notes += $". Last updated on {DateTime.Now}";

        var response = await _gardenClient.UpdateGardenBed(gardenBed);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to update Garden Bed responded with {response.StatusCode} code and {returnString} message");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(returnString);
    }

    [Fact]
    public async Task Delete_GardenBed_ShouldDelete()
    {
        var gardenId = await GetGardenIdToWorkWith(TEST_DELETE_GARDEN_NAME);

        var gardenBed = await GetGardenBedToWorkWith(gardenId, TEST_DELETE_GARDEN_BED_NAME);


        var response = await _gardenClient.DeleteGardenBed(gardenBed.GardenId, gardenBed.GardenBedId);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to delete garden bed responded with {response.StatusCode} code and {returnString} message");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(returnString);
    }
    #endregion

    #region Shared Private Functions
    public async Task<string> GetGardenIdToWorkWith(string gardenName)
    {
        var garden = await GetGardenToWorkWith(gardenName);
        return garden.GardenId;
    }

    private async Task<GardenViewModel> GetGardenToWorkWith(string gardenName)
    {

        var response = await _gardenClient.GetGardenByName(gardenName);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await _gardenClient.CreateGarden(gardenName);
        }

        response = await _gardenClient.GetGardenByName(gardenName);
        var returnString = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"GetGardenIdToWorkWith - Service to get garden responded with {response.StatusCode} code and {returnString} message");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                {
                    new JsonStringEnumConverter(),
                },
        };
        var garden = await response.Content.ReadFromJsonAsync<GardenViewModel>(options);

        _output.WriteLine($"GetGardenIdToWorkWith - Found  {garden!.GardenId} garden to work with.");
        return garden;
    }

    private async Task<GardenBedViewModel> GetGardenBedToWorkWith(string gardenId, string gardenBedName)
    {
        var response = await _gardenClient.GetGardenBeds(gardenId);

        _output.WriteLine($"Service to get garden beds responded with {response.StatusCode} code");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                {
                    new JsonStringEnumConverter(),
                },
        };
        var gardenBeds = await response.Content.ReadFromJsonAsync<List<GardenBedViewModel>>(options);
        GardenBedViewModel? gardenBed = null;

        if (gardenBeds == null || gardenBeds.Count == 0)
        {
            var gardenBedId = await CreateGardenBedIdToWorkWith(gardenId, gardenBedName);

            response = await _gardenClient.GetGardenBed(gardenId, gardenBedId);

            var returnString = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Service to get garden bed responded with {response.StatusCode} code and message {returnString}");

            gardenBed = await response.Content.ReadFromJsonAsync<GardenBedViewModel>(options);
        }
        else
        {
            gardenBed = gardenBeds.First(p => p.Name == gardenBedName);
        }

        return gardenBed!;
    }

    private async Task<List<GardenBedViewModel>> GetGardenBedsToWorkWith(string gardenId)
    {
        var response = await _gardenClient.GetGardenBeds(gardenId);
        _output.WriteLine($"Service to get garden beds responded with {response.StatusCode} code");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                {
                    new JsonStringEnumConverter(),
                },
        };
        var gardenBeds = await response.Content.ReadFromJsonAsync<List<GardenBedViewModel>>(options);

        if (gardenBeds == null || gardenBeds.Count == 0)
        {
            await CreateGardenBedIdToWorkWith(gardenId, TEST_GARDEN_BED_NAME);

            response = await _gardenClient.GetGardenBeds(gardenId);

            _output.WriteLine($"Service to get garden beds responded with {response.StatusCode} code");

            gardenBeds = await response.Content.ReadFromJsonAsync<List<GardenBedViewModel>>(options);
        }

        return gardenBeds!;

    }

    private async Task<string> CreateGardenBedIdToWorkWith(string gardenId, string gardenBedName)
    {
        var response = await _gardenClient.CreateGardenBed(gardenId, gardenBedName);
        var returnString = await response.Content.ReadAsStringAsync();


        Assert.NotEmpty(returnString);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.True(Guid.TryParse(returnString, out _));

        return returnString;
    }

    #endregion
}
