using Amazon.Runtime.Internal.Util;
using GardenLog.SharedInfrastructure;
using GardenLog.SharedInfrastructure.ApiClients;
using GardenLog.SharedInfrastructure.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace PlantCatalog.IntegrationTest;

public class PlantCatalogTests : IClassFixture<PlantCatalogServiceFixture>
{
    private readonly ITestOutputHelper _output;
    private readonly PlantCatalogClient _plantCatalogClient;
    private const string TEST_PLANT_NAME = "Blackmelon";
    private const string TEST_GROW_INSTRUCTION_NAME = "Start at home and pick in the Summer";
    private const string TEST_VARIETY_NAME = "Black Beauty";
    private const string TEST_DELETE_GROW_INSTRUCTION_NAME = "Go buy something fromt the store";
    private const string TEST_DELETE_PLANT_NAME = "DeletePlantName";
    private const string TEST_DELETE_VARIETY_NAME = "Delete Black Beauty";

    public PlantCatalogTests(PlantCatalogServiceFixture fixture, ITestOutputHelper output)
    {
        _plantCatalogClient = fixture.PlantCatalogClient;
        _output = output;
        _output.WriteLine($"Service id {fixture.FixtureId} @ {DateTime.Now:F}");
    }

    [Fact]
    public async Task WhyDoesItFail()
    {
        var token = (new Auth0Helper()).GetToken(typeof(Program).Assembly);

        _output.WriteLine($"Token is {token}");

        Assert.NotNull(token);
    }

    [Fact]
    public async Task WhyDoesItFail2()
    {
        var configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .AddUserSecrets(typeof(Program).Assembly)
              .AddEnvironmentVariables()
              .Build();

        var serviceProvider = new ServiceCollection()
            .AddHttpClient()
            .AddLogging()
            .AddMemoryCache()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<IConfigurationService, ConfigurationService>()
            .AddSingleton<IAuth0AuthenticationApiClient, Auth0AuthenticationApiClient>()
            .BuildServiceProvider();

        var authApiClient = serviceProvider.GetRequiredService<IAuth0AuthenticationApiClient>();
        var appSettings = serviceProvider.GetRequiredService<IConfigurationService>().GetAuthSettings();

        if (appSettings.Audience == null) throw new ArgumentException("Required Audience paramter is not found. Can not generate access token without Audience", "Audience");

        _output.WriteLine($"Token is {appSettings.Audience}");


        TokenRequest _tokenRequest = new()
        {
            ClientId = appSettings.ClientId,
            ClientSecret = appSettings.ClientSecret,
            GrantType = "client_credentials"
        };

      
        _tokenRequest.Audience = appSettings.Audience;

        _output.WriteLine($"Request for token {_tokenRequest}");

        var accessToken = authApiClient.GetAccessToken(appSettings.Audience).GetAwaiter().GetResult();

        _output.WriteLine($"{accessToken}");

        Assert.NotNull(accessToken);
    }
    //#region Plant
    //[Fact]
    //public async Task Post_Plant_MayCreateNewProduct()
    //{
    //    var response = await _plantCatalogClient.CreatePlant(TEST_PLANT_NAME);

    //    var returnString = await response.Content.ReadAsStringAsync();

    //    _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

    //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    //    {
    //        Assert.NotEmpty(returnString);
    //        Assert.True(Guid.TryParse(returnString, out _));
    //    }
    //    else
    //    {
    //        Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
    //        Assert.NotEmpty(returnString);
    //        Assert.Contains("Plant with this name already exists", returnString);
    //    }
    //}

    //[Fact]
    //public async Task Post_Plant_ShouldNotCreateNewProduct_WithoutName()
    //{
    //    var response = await _plantCatalogClient.CreatePlant("");

    //    var returnString = await response.Content.ReadAsStringAsync();

    //    _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

    //    Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
    //    Assert.NotEmpty(returnString);
    //    Assert.Contains("'Name' must not be empty.", returnString);
    //}

    //[Fact]
    //public async Task Put_Plant_ShouldUpdatePlant()
    //{
    //    var response = await _plantCatalogClient.GetPlantIdByPlantName(TEST_PLANT_NAME);

    //    var returnString = await response.Content.ReadAsStringAsync();

    //    _output.WriteLine($"Plant to update was found with service responded with {response.StatusCode} code and {returnString} message");

    //    if (response.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(returnString))
    //    {
    //        _output.WriteLine($"Plant to update is not found. Will create new one");
    //        response = await _plantCatalogClient.CreatePlant(TEST_PLANT_NAME);

    //        returnString = await response.Content.ReadAsStringAsync();

    //        _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");
    //    }

    //    response = await _plantCatalogClient.GetPlant(returnString);
    //    returnString = await response.Content.ReadAsStringAsync();
    //    _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

    //    var options = new JsonSerializerOptions
    //    {
    //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //        Converters =
    //            {
    //                new JsonStringEnumConverter(),
    //            },
    //    };
    //    var plant = await response.Content.ReadFromJsonAsync<PlantViewModel>(options);

    //    plant!.SeedViableForYears += 1;

    //    response = await _plantCatalogClient.UpdatePlant(plant);

    //    returnString = await response.Content.ReadAsStringAsync();

    //    _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

    //    Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
    //    Assert.NotEmpty(returnString);
    //}

    //[Fact]
    //public async Task Delete_Plant()
    //{
    //    var response = await _plantCatalogClient.GetPlantIdByPlantName(TEST_DELETE_PLANT_NAME);

    //    var returnString = await response.Content.ReadAsStringAsync();

    //    _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

    //    if (response.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(returnString))
    //    {
    //        _output.WriteLine($"Plant to delete is not found. Will create new one");
    //        response = await _plantCatalogClient.CreatePlant(TEST_DELETE_PLANT_NAME);

    //        returnString = await response.Content.ReadAsStringAsync();

    //        _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");
    //    }

    //    response = await _plantCatalogClient.DeletePLant(returnString);

    //    returnString = await response.Content.ReadAsStringAsync();

    //    _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

    //    Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
    //    Assert.NotEmpty(returnString);
    //}

    //[Fact]
    //public async Task Get_Should_Return_All_Plants()
    //{
    //    var response = await _plantCatalogClient.GetAllPlants();

    //    var options = new JsonSerializerOptions
    //    {
    //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //        Converters =
    //            {
    //                new JsonStringEnumConverter(),
    //            },
    //    };

    //    var returnString = await response.Content.ReadAsStringAsync();
    //    _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

    //    var plants = await response.Content.ReadFromJsonAsync<List<PlantViewModel>>(options);

    //    Assert.NotNull(plants);
    //    Assert.NotEmpty(plants);

    //    var testPlant = plants.FirstOrDefault(plants => plants.Name == TEST_PLANT_NAME);

    //    Assert.NotNull(testPlant);
    //}

    //[Fact]
    //public async Task Get_Should_Return_All_PlantNames()
    //{
    //    var response = await _plantCatalogClient.GetAllPlantNames();

    //    var options = new JsonSerializerOptions
    //    {
    //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //        Converters =
    //            {
    //                new JsonStringEnumConverter(),
    //            },
    //    };

    //    var returnString = await response.Content.ReadAsStringAsync();
    //    _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

    //    var plants = await response.Content.ReadFromJsonAsync<List<PlantNameOnlyViewModel>>(options);

    //    Assert.NotNull(plants);
    //    Assert.NotEmpty(plants);

    //    var testPlant = plants.FirstOrDefault(plants => plants.Name == TEST_PLANT_NAME);

    //    Assert.NotNull(testPlant);
    //}

    //[Fact]
    //public async Task Get_Should_Return_Plant()
    //{
    //    var plantId = await GetPlantIdToWorkWith(TEST_PLANT_NAME);


    //    var response = await _plantCatalogClient.GetPlant(plantId);

    //    var options = new JsonSerializerOptions
    //    {
    //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //        Converters =
    //            {
    //                new JsonStringEnumConverter(),
    //            },
    //    };

    //    var returnString = await response.Content.ReadAsStringAsync();
    //    _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

    //    var plant = await response.Content.ReadFromJsonAsync<PlantViewModel>(options);

    //    Assert.NotNull(plant);
    //    Assert.Equal(TEST_PLANT_NAME, plant.Name);
    //}

    //#endregion

    //#region Plant Grow Instruction

    //[Fact]
    //public async Task Post_PlantGrowInstruction_MayCreateNew()
    //{
    //    var plantId = await GetPlantIdToWorkWith(TEST_PLANT_NAME);

    //    var response = await _plantCatalogClient.CreatePlantGrowInstruction(plantId, TEST_GROW_INSTRUCTION_NAME);
    //    var returnString = await response.Content.ReadAsStringAsync();

    //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    //    {
    //        Assert.NotEmpty(returnString);
    //        Assert.True(Guid.TryParse(returnString, out _));
    //    }
    //    else
    //    {
    //        Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
    //        returnString = await response.Content.ReadAsStringAsync();
    //        Assert.NotEmpty(returnString);
    //        Assert.Contains("Grow Instruction with this name already exists", returnString);
    //    }
    //}

    //[Fact]
    //public async Task Get_PlantGrowInstruction_All()
    //{
    //    var growInstructions = await GetPlantGrowInstructionsToWorkWith();

    //    Assert.NotNull(growInstructions);
    //    _output.WriteLine($"Found '{growInstructions.First().Name}' grow instruction");
    //    Assert.NotEmpty(growInstructions.First().HarvestInstructions);
    //}

    //[Fact]
    //public async Task Get_PlantGrowInstruction_One()
    //{
    //    var growInstructions = await GetPlantGrowInstructionsToWorkWith();

    //    var original = growInstructions.First(g => g.Name == TEST_GROW_INSTRUCTION_NAME);

    //    if (growInstructions.Count == 1)
    //    {
    //        //create new grow instruction to make sure we only get one back
    //        var response = await _plantCatalogClient.CreatePlantGrowInstruction(original.PlantId, TEST_DELETE_GROW_INSTRUCTION_NAME);
    //    }

    //    var growInstruction = await GetPlantGrowInstructionToWorkWith(original.PlantId, original.PlantGrowInstructionId);

    //    _output.WriteLine($"Found '{growInstruction.Name}' grow instruction");
    //    Assert.Equal(original.Name, growInstruction.Name);
    //}

    //[Fact]
    //public async Task Put_PlantGrowInstruction_ShouldUpdate()
    //{
    //    var grow = (await GetPlantGrowInstructionsToWorkWith()).First(g => g.Name == TEST_GROW_INSTRUCTION_NAME);

    //    //Step 3 update grow Instruction

    //    grow.DaysToSproutMin += 1;

    //    var response = await _plantCatalogClient.UpdatePlantGrowInstruction(grow);

    //    var returnString = await response.Content.ReadAsStringAsync();

    //    _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

    //    Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
    //    Assert.NotEmpty(returnString);
    //}

    //[Fact]
    //public async Task Put_PlantGrowInstruction_ShouldDelete()
    //{
    //    var grow = (await GetPlantGrowInstructionsToWorkWith()).FirstOrDefault(g => g.Name == TEST_DELETE_GROW_INSTRUCTION_NAME);

    //    if (grow == null)
    //    {
    //        //oh well. something deleted this grow nstruction already. will skip this round
    //        return;
    //    }

    //    var response = await _plantCatalogClient.DeletePlantGrowInstruction(grow.PlantId, grow.PlantGrowInstructionId);

    //    var returnString = await response.Content.ReadAsStringAsync();

    //    _output.WriteLine($"Service to delete plant grow instruction responded with {response.StatusCode} code and {returnString} message");

    //    Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
    //    Assert.NotEmpty(returnString);
    //}

    //#endregion

    //#region PLant Variety
    //[Fact]
    //public async Task Post_PlantVariety_MayCreateNew()
    //{
    //    var plantId = await GetPlantIdToWorkWith(TEST_PLANT_NAME);

    //    var varieties = await GetPlantVarietiesToWorkWith(plantId);

    //    var original = varieties.FirstOrDefault(v => v.Name.Equals(TEST_VARIETY_NAME));

    //    if (original != null)
    //    {
    //        var response = await _plantCatalogClient.DeletePlantVariety(original.PlantId, original.PlantVarietyId);
    //    }

    //    var varietyId = await CreatePLantVarietyToWorkWith(plantId, TEST_VARIETY_NAME);
    //}

    //[Fact]
    //public async Task Get_PlantVariety_All()
    //{
    //    var response = await _plantCatalogClient.GetPlantVarieties();

    //    _output.WriteLine($"Service to get all plant varieties responded with {response.StatusCode} code");

    //    var options = new JsonSerializerOptions
    //    {
    //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //        Converters =
    //            {
    //                new JsonStringEnumConverter(),
    //            },
    //    };
    //    var varieties = await response.Content.ReadFromJsonAsync<List<PlantVarietyViewModel>>(options);

    //    Assert.NotNull(varieties);
    //    Assert.NotEmpty(varieties);
    //}

    //[Fact]
    //public async Task Get_PlantVariety_ByPLantId()
    //{
    //    var variety = await GetPlantVarietiesBasedOnTestPlantToWorkWith();

    //    Assert.NotNull(variety);
    //    _output.WriteLine($"Found '{variety.First().Name}' variety");
    //    Assert.NotEmpty(variety.First().Title);
    //}

    //[Fact]
    //public async Task Get_PlantVariety_One()
    //{
    //    var varieties = await GetPlantVarietiesBasedOnTestPlantToWorkWith();

    //    var original = varieties.First(g => g.Name == TEST_VARIETY_NAME);

    //    if (varieties.Count == 1)
    //    {
    //        //create new variety to make sure we only get one back
    //        var response = await _plantCatalogClient.CreatePlantVariety(original.PlantId, TEST_DELETE_VARIETY_NAME);
    //    }

    //    var variety = await GetPlantVarietyToWorkWith(original.PlantId, original.PlantVarietyId);

    //    _output.WriteLine($"Found '{variety.Name}' variety");
    //    Assert.Equal(original.Name, variety.Name);
    //}

    //[Fact]
    //public async Task Put_PlantVariety_ShouldUpdate()
    //{
    //    var variety = (await GetPlantVarietiesBasedOnTestPlantToWorkWith()).First(g => g.Name == TEST_VARIETY_NAME);

    //    //Step 3 update variety

    //    variety.DaysToMaturityMax += 1;
    //    variety.Tags.Add("Black " + DateTime.Now.ToString());

    //    var response = await _plantCatalogClient.UpdatePlantVariety(variety);

    //    var returnString = await response.Content.ReadAsStringAsync();

    //    _output.WriteLine($"Service to update Variety responded with {response.StatusCode} code and {returnString} message");

    //    Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
    //    Assert.NotEmpty(returnString);
    //}

    //[Fact]
    //public async Task Delete_PlantVariety_ShouldDelete()
    //{
    //    var plantId = await GetPlantIdToWorkWith(TEST_PLANT_NAME);

    //    var variety = (await GetPlantVarietiesToWorkWith(plantId)).FirstOrDefault(g => g.Name == TEST_DELETE_VARIETY_NAME);

    //    if (variety == null)
    //    {
    //        var varietyId = await CreatePLantVarietyToWorkWith(plantId, TEST_DELETE_VARIETY_NAME);
    //        variety = new PlantVarietyViewModel() { PlantId = plantId, PlantVarietyId = varietyId };
    //    }

    //    var response = await _plantCatalogClient.DeletePlantVariety(variety.PlantId, variety.PlantVarietyId);

    //    var returnString = await response.Content.ReadAsStringAsync();

    //    _output.WriteLine($"Service to delete plant variety responded with {response.StatusCode} code and {returnString} message");

    //    Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
    //    Assert.NotEmpty(returnString);
    //}
    //#endregion

    //#region Shared Private Functions
    //private async Task<string> GetPlantIdToWorkWith(string plantName)
    //{
    //    //Step 1 - Get Plant to work with
    //    var response = await _plantCatalogClient.GetPlantIdByPlantName(plantName);
    //    var plantId = await response.Content.ReadAsStringAsync();

    //    if (response.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(plantId))
    //    {
    //        _output.WriteLine($"Plant is not found. Will create new one");
    //        response = await _plantCatalogClient.CreatePlant(plantName);

    //        plantId = await response.Content.ReadAsStringAsync();

    //        _output.WriteLine($"Service to create plant responded with {response.StatusCode} code and {plantId} message");
    //    }
    //    else
    //    {
    //        _output.WriteLine($"Plant was found with service responded with {response.StatusCode} code and {plantId} message");
    //    }

    //    return plantId;
    //}

    //private async Task<List<PlantGrowInstructionViewModel>> GetPlantGrowInstructionsToWorkWith()
    //{
    //    var plantId = await GetPlantIdToWorkWith(TEST_PLANT_NAME);

    //    //Step 2 - Get Grow Instruction to update
    //    var response = await _plantCatalogClient.GetPlantGrowInstructions(plantId);

    //    _output.WriteLine($"Service to get all plant grow instructions responded with {response.StatusCode} code");

    //    var options = new JsonSerializerOptions
    //    {
    //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //        Converters =
    //            {
    //                new JsonStringEnumConverter(),
    //            },
    //    };
    //    var growInstructions = await response.Content.ReadFromJsonAsync<List<PlantGrowInstructionViewModel>>(options);

    //    Assert.NotNull(growInstructions);
    //    Assert.NotEmpty(growInstructions);

    //    return growInstructions;
    //}

    //private async Task<PlantGrowInstructionViewModel> GetPlantGrowInstructionToWorkWith(string plantId, string growId)
    //{
    //    var response = await _plantCatalogClient.GetPlantGrowInstruction(plantId, growId);

    //    _output.WriteLine($"Service to get single plant grow instruction responded with {response.StatusCode} code");

    //    var options = new JsonSerializerOptions
    //    {
    //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //        Converters =
    //            {
    //                new JsonStringEnumConverter(),
    //            },
    //    };
    //    var growInstruction = await response.Content.ReadFromJsonAsync<PlantGrowInstructionViewModel>(options);

    //    Assert.NotNull(growInstruction);

    //    return growInstruction;
    //}

    //private async Task<List<PlantVarietyViewModel>> GetPlantVarietiesToWorkWith(string plantId)
    //{
    //    //Step 2 - Get Grow Instruction to update
    //    var response = await _plantCatalogClient.GetPlantVarieties(plantId);

    //    _output.WriteLine($"Service to get all plant varieties responded with {response.StatusCode} code");

    //    var options = new JsonSerializerOptions
    //    {
    //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //        Converters =
    //            {
    //                new JsonStringEnumConverter(),
    //            },
    //    };
    //    var varieties = await response.Content.ReadFromJsonAsync<List<PlantVarietyViewModel>>(options);


    //    return varieties!;
    //}

    //private async Task<List<PlantVarietyViewModel>> GetPlantVarietiesBasedOnTestPlantToWorkWith()
    //{
    //    var plantId = await GetPlantIdToWorkWith(TEST_PLANT_NAME);

    //    return await GetPlantVarietiesToWorkWith(plantId);

    //}

    //private async Task<PlantVarietyViewModel> GetPlantVarietyToWorkWith(string plantId, string varietyId)
    //{
    //    var response = await _plantCatalogClient.GetPlantVariety(plantId, varietyId);

    //    _output.WriteLine($"Service to get single plant variety responded with {response.StatusCode} code");

    //    var options = new JsonSerializerOptions
    //    {
    //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //        Converters =
    //            {
    //                new JsonStringEnumConverter(),
    //            },
    //    };
    //    var variety = await response.Content.ReadFromJsonAsync<PlantVarietyViewModel>(options);

    //    Assert.NotNull(variety);

    //    return variety;
    //}

    //private async Task<string> CreatePLantVarietyToWorkWith(string plantId, string plantName)
    //{
    //    var response = await _plantCatalogClient.CreatePlantVariety(plantId, plantName);
    //    var returnString = await response.Content.ReadAsStringAsync();

    //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    //    {
    //        Assert.NotEmpty(returnString);
    //        Assert.True(Guid.TryParse(returnString, out _));
    //    }
    //    else
    //    {
    //        Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
    //        returnString = await response.Content.ReadAsStringAsync();
    //        Assert.NotEmpty(returnString);
    //        Assert.Contains("Variety with this name already exists", returnString);
    //    }
    //    return returnString;
    //}

    //#endregion
}
