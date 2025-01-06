using PlantHarvest.Contract.ViewModels;
using PlantHarvest.IntegrationTest.Clients;
using PlantHarvest.IntegrationTest.Fixture;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace PlantHarvest.IntegrationTest;

public partial class PlantHarvestTests : IClassFixture<PlantHarvestServiceFixture>
{
    private readonly ITestOutputHelper _output;
    private readonly PlantHarvestClient _plantHarvestClient;
    private readonly WorkLogClient _workLogClient;
    private readonly PlantTaskClient _plantTaskClient;

    public const string TEST_HARVEST_CYCLE_NAME = "Test Harvest Cycle";
    public const string TEST_PLANT_ID = "a461feac-a128-4b56-ab35-89ef71264107";
    public const string TEST_PLANT_VARIETY_ID = "7cfcc6cc-db99-4dc7-b2dc-88e6959896de";

    private const string TEST_DELETE_PLANT_ID = "Delete-Fake-Plant-Id";
    private const string TEST_DELETE_VARIETY_ID = "Delete-Fake-Variety-Id";
    private const string TEST_DELETE_HARVEST_CYCLE_NAME = "Delete Harvest Cycle";

    public PlantHarvestTests(PlantHarvestServiceFixture fixture, ITestOutputHelper output)
    {
        _plantHarvestClient = fixture.PlantHarvestClient;
        _workLogClient = fixture.WorkLogClient;
        _plantTaskClient = fixture.PlantTaskClient;

        _output = output;
        _output.WriteLine($"Service id {fixture.FixtureId} @ {DateTime.Now.ToString("F")}");
    }

    #region Harvest Cycle
    [Fact]
    public async Task Post_HarvestCycle_MayCreateNewHarvestCycle()
    {
        var response = await _plantHarvestClient.CreateHarvestCycle(TEST_HARVEST_CYCLE_NAME);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to create harvest cycle responded with {response.StatusCode} code and {returnString} message");

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            Assert.NotEmpty(returnString);
            Assert.True(Guid.TryParse(returnString, out var harvestCycleId));
        }
        else
        {
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
            Assert.NotEmpty(returnString);
            Assert.Contains("Garden Plan with this name already exists", returnString);
        }
    }

    [Fact]
    public async Task Post_HarvestCycle_ShouldNotCreateNewHarvestCycle_WithoutName()
    {
        var response = await _plantHarvestClient.CreateHarvestCycle("");

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        Assert.NotEmpty(returnString);
        Assert.Contains("'Harvest Cycle Name' must not be empty.", returnString);
    }

    [Fact]
    public async Task Put_HarvestCycle_ShouldUpdateHarvestCycle()
    {
        var harvest = await GetHarvestCycleToWorkWith(TEST_HARVEST_CYCLE_NAME);

        harvest.Notes = harvest.Notes.Length < 900 ? $"{harvest.Notes} last pdated: {DateTime.Now.ToString()}" : $"{harvest.Notes.Substring(0, 100)} last pdated: {DateTime.Now.ToString()}";

        var response = await _plantHarvestClient.UpdateHarvestCycle(harvest);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to update harvest cycle responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
    }

    [Fact]
    public async Task Delete_HarvestCycle_ShouldDelete()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(TEST_DELETE_HARVEST_CYCLE_NAME);

        var response = await _plantHarvestClient.DeleteHarvestCycle(harvestId);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to delete harvest cycle responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
    }

    [Fact]
    public async Task End_HarvestCycle_ShouldCompleteAllPlants()
    {
        var harvest = await GetHarvestCycleToWorkWith(TEST_DELETE_HARVEST_CYCLE_NAME);
        var plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvest.HarvestCycleId, TEST_DELETE_PLANT_ID, TEST_DELETE_VARIETY_ID);
        await CreateGardenBedPlantHarvestCycleToWorkWith(plant);

        harvest.EndDate = DateTime.Now;

        var response = await _plantHarvestClient.UpdateHarvestCycle(harvest);

        plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvest.HarvestCycleId, TEST_DELETE_PLANT_ID, TEST_DELETE_VARIETY_ID);
        var bed = plant.GardenBedLayout.FirstOrDefault();

        Assert.True(plant.LastHarvestDate.HasValue);
        Assert.NotNull(bed);
        Assert.True(bed.EndDate.HasValue);

        await _plantHarvestClient.DeleteHarvestCycle(harvest.HarvestCycleId);
    }

    [Fact]
    public async Task Get_Should_Return_All_HarvestCycles()
    {
        var response = await _plantHarvestClient.GetAllHarvestCycles();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                {
                    new JsonStringEnumConverter(),
                },
        };

        var returnString = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Service for all harvest cycles responded with {response.StatusCode} code and {returnString} message");

        var harvests = await response.Content.ReadFromJsonAsync<List<HarvestCycleViewModel>>(options);

        Assert.NotNull(harvests);
        Assert.NotEmpty(harvests);

        var testHarvest = harvests.FirstOrDefault(plants => plants.HarvestCycleName == TEST_HARVEST_CYCLE_NAME);

        Assert.NotNull(testHarvest);
    }

    [Fact]
    public async Task Get_Should_Return_HarvestCycle()
    {
        var plant = await GetHarvestCycleToWorkWith(TEST_HARVEST_CYCLE_NAME);

        Assert.NotNull(plant);
        Assert.Equal(TEST_HARVEST_CYCLE_NAME, plant.HarvestCycleName);
    }

    #endregion

    #region Plant Harevst Cycle
    [Fact]
    public async Task Post_PlantHarvestCycle_MayCreateNew()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(TEST_HARVEST_CYCLE_NAME);

        var original = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, TEST_PLANT_ID, TEST_PLANT_VARIETY_ID);

        if (original != null)
        {
            var response = await _plantHarvestClient.DeletePlantHarvestCycle(original.HarvestCycleId, original.PlantHarvestCycleId);
        }

        var harvestCycleId = await _plantHarvestClient.CreatePlantHarvestCycleToWorkWith(harvestId, TEST_PLANT_ID, TEST_PLANT_VARIETY_ID);
        Assert.NotNull(harvestCycleId);
    }

    [Fact]
    public async Task Get_PlantHarvestCycle_ByHarvestId()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(TEST_HARVEST_CYCLE_NAME);
        var plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, TEST_PLANT_ID, TEST_PLANT_VARIETY_ID);

        Assert.NotNull(plant);
        _output.WriteLine($"Found '{plant.PlantVarietyId}' Plant Harvest Cycle");
        Assert.NotEmpty(plant.PlantVarietyId!);
    }

    [Fact]
    public async Task Get_PlantHarvestCycle_One()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(TEST_HARVEST_CYCLE_NAME);
        var plants = await GetPlantHarvestCyclesToWorkWith(harvestId, TEST_PLANT_ID, TEST_PLANT_VARIETY_ID);

        var original = plants.First(g => g.PlantVarietyId == TEST_PLANT_VARIETY_ID);

        if (plants.Count == 1)
        {
            //create new HarvestCycle to make sure we only get one back
            var response = await _plantHarvestClient.CreatePlantHarvestCycle(harvestId, TEST_PLANT_ID, TEST_PLANT_VARIETY_ID);
        }

        var plant = await GetPlantHarvestCycleToWorkWith(harvestId, original.PlantHarvestCycleId);

        _output.WriteLine($"Found '{plant.PlantVarietyId}' Plant Harvest Cycle");
        Assert.Equal(original.PlantVarietyId, plant.PlantVarietyId);
    }

    [Fact]
    public async Task Get_PlantHarvestCycles_ByPlantId()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(TEST_HARVEST_CYCLE_NAME);
        var plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, TEST_PLANT_ID, TEST_PLANT_VARIETY_ID);

        var searchResponse = await _plantHarvestClient.GetPlantHarvestCyclesByPlantId(TEST_PLANT_ID);

        Assert.NotNull(searchResponse);
        Assert.True(searchResponse.StatusCode == System.Net.HttpStatusCode.OK);

        var returnString = await searchResponse.Content.ReadAsStringAsync();

        _output.WriteLine($"Get_PlantHarvestCycles_ByPlantId - Found '{returnString}' by searching by plant Id");
        Assert.Contains(plant.PlantHarvestCycleId, returnString);
    }

    [Fact]
    public async Task Put_PlantHarvestCycle_ShouldUpdate()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(TEST_HARVEST_CYCLE_NAME);
        var plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, TEST_PLANT_ID, TEST_PLANT_VARIETY_ID);


        plant.NumberOfSeeds = 1 + plant.NumberOfSeeds ?? 1;

        var response = await _plantHarvestClient.UpdatePlantHarvestCycle(plant);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to update Plant Harvest Cycle responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
    }

    [Fact]
    public async Task Put_PlantHarvestCycle_GrowingMethod_ShouldUpdate()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(TEST_HARVEST_CYCLE_NAME);
        var plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, TEST_PLANT_ID, TEST_PLANT_VARIETY_ID);


        plant.PlantingMethod = plant.PlantingMethod==Contract.Enum.PlantingMethodEnum.DirectSeed
                ? Contract.Enum.PlantingMethodEnum.SeedIndoors
                : Contract.Enum.PlantingMethodEnum.DirectSeed;

        var response = await _plantHarvestClient.UpdatePlantHarvestCycle(plant);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to update Plant Harvest Cycle responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
    }

    [Fact]
    public async Task Delete_PlantHarvestCycle_ShouldDelete()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(TEST_DELETE_HARVEST_CYCLE_NAME);

        var plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, TEST_DELETE_PLANT_ID, TEST_DELETE_VARIETY_ID);


        var response = await _plantHarvestClient.DeletePlantHarvestCycle(plant.HarvestCycleId, plant.PlantHarvestCycleId);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to delete plant HarvestCycle responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
    }
    #endregion

    #region Plant Schedule
    [Fact]
    public async Task Post_PlantSchedule_MayCreateNew()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(TEST_HARVEST_CYCLE_NAME);

        var plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, TEST_PLANT_ID, TEST_PLANT_VARIETY_ID);

        var plantScheduleId = await CreatePlantScheduleToWorkWith(plant);

        Assert.NotNull(plantScheduleId);
    }

    [Fact]
    public async Task Put_PlantSchedule_ShouldUpdate()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(TEST_HARVEST_CYCLE_NAME);
        var plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, TEST_PLANT_ID, TEST_PLANT_VARIETY_ID);

        var schedule = plant.PlantCalendar.FirstOrDefault();
        if (schedule == null)
        {
            await _plantHarvestClient.CreatePlantSchedule(plant.HarvestCycleId, plant.PlantHarvestCycleId);
            plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, TEST_PLANT_ID, TEST_PLANT_VARIETY_ID);
            schedule = plant.PlantCalendar.FirstOrDefault();
        }

        schedule!.Notes += $" Last update on {DateTime.Now}";

        var response = await _plantHarvestClient.UpdatePlantSchedule(schedule);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to update Plant Schedule responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
    }

    [Fact]
    public async Task Delete_PlantSchedule_ShouldDelete()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(TEST_DELETE_HARVEST_CYCLE_NAME);

        var plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, TEST_DELETE_PLANT_ID, TEST_DELETE_VARIETY_ID);

        var schedule = plant.PlantCalendar.FirstOrDefault();
        if (schedule == null)
        {
            await _plantHarvestClient.CreatePlantSchedule(plant.HarvestCycleId, plant.PlantHarvestCycleId);
            plant = await GetPlantHarvestCycleToWorkWith(plant.HarvestCycleId, plant.PlantHarvestCycleId);
            schedule = plant.PlantCalendar.FirstOrDefault();
        }

        var response = await _plantHarvestClient.DeletePlantSchedule(plant.HarvestCycleId, plant.PlantHarvestCycleId, schedule!.PlantScheduleId);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to delete plant scheule responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
    }
    #endregion

    #region Garden Layout Plant
    [Fact]
    public async Task Post_GardenBedPlantHarvestCycle_MayCreateNew()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(TEST_HARVEST_CYCLE_NAME);

        var plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, TEST_PLANT_ID, TEST_PLANT_VARIETY_ID);

        var gardenBedPlantId = await CreateGardenBedPlantHarvestCycleToWorkWith(plant);

        Assert.NotNull(gardenBedPlantId);
    }

    [Fact]
    public async Task Put_GardenBedPlantHarvestCycle_ShouldUpdate()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(TEST_HARVEST_CYCLE_NAME);
        var plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, TEST_PLANT_ID, TEST_PLANT_VARIETY_ID);

        var gardenBedPlant = plant.GardenBedLayout.FirstOrDefault();
        if (gardenBedPlant == null)
        {
            await _plantHarvestClient.CreateGardenBedPlantHarvestCycle(plant.HarvestCycleId, plant.PlantHarvestCycleId);
            plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, TEST_PLANT_ID, TEST_PLANT_VARIETY_ID);
            gardenBedPlant = plant.GardenBedLayout.FirstOrDefault();
        }

        gardenBedPlant!.X++;

        var response = await _plantHarvestClient.UpdateGardenBedPlantHarvestCycle(gardenBedPlant);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to update Garden Bed Plant Layout responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
    }

    [Fact]
    public async Task Delete_GardenBedPlantHarvestCycle_ShouldDelete()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(TEST_DELETE_HARVEST_CYCLE_NAME);

        var plant = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, TEST_DELETE_PLANT_ID, TEST_DELETE_VARIETY_ID);

        var gardenBedPlant = plant.GardenBedLayout.FirstOrDefault();
        if (gardenBedPlant == null)
        {
            await _plantHarvestClient.CreateGardenBedPlantHarvestCycle(plant.HarvestCycleId, plant.PlantHarvestCycleId);
            plant = await GetPlantHarvestCycleToWorkWith(plant.HarvestCycleId, plant.PlantHarvestCycleId);
            gardenBedPlant = plant.GardenBedLayout.FirstOrDefault();
        }

        var response = await _plantHarvestClient.DeleteGardenBedPlantHarvestCycle(plant.HarvestCycleId, plant.PlantHarvestCycleId, gardenBedPlant!.GardenBedPlantHarvestCycleId);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to delete plant scheule responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
    }
    #endregion


    #region Shared Private Functions


    private async Task<HarvestCycleViewModel> GetHarvestCycleToWorkWith(string harvestName)
    {

        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(harvestName);

        var response = await _plantHarvestClient.GetHarvestCycle(harvestId);
        var returnString = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                {
                    new JsonStringEnumConverter(),
                },
        };
        var harvest = await response.Content.ReadFromJsonAsync<HarvestCycleViewModel>(options);

        return harvest!;
    }



    private async Task<List<PlantHarvestCycleViewModel>> GetPlantHarvestCyclesToWorkWith(string harvestId, string plantId, string plantVarietyId)
    {
        var response = await _plantHarvestClient.GetPlantHarvestCycles(harvestId);
        _output.WriteLine($"Service to get all plant harvest cycles responded with {response.StatusCode} code");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                {
                    new JsonStringEnumConverter(),
                },
        };
        var plants = await response.Content.ReadFromJsonAsync<List<PlantHarvestCycleViewModel>>(options);

        if (plants == null || plants.Count == 0)
        {
            await _plantHarvestClient.CreatePlantHarvestCycleToWorkWith(harvestId, plantId, plantVarietyId);

            response = await _plantHarvestClient.GetPlantHarvestCycles(harvestId);

            _output.WriteLine($"Service to get all plan harvest cycles responded with {response.StatusCode} code");

            plants = await response.Content.ReadFromJsonAsync<List<PlantHarvestCycleViewModel>>(options);
        }

        return plants!;

    }

    private async Task<PlantHarvestCycleViewModel> GetPlantHarvestCycleToWorkWith(string harvestId, string id)
    {
        var response = await _plantHarvestClient.GetPlantHarvestCycle(harvestId, id);

        _output.WriteLine($"Service to get single plant harvest cycle responded with {response.StatusCode} code");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                {
                    new JsonStringEnumConverter(),
                },
        };
        var plant = await response.Content.ReadFromJsonAsync<PlantHarvestCycleViewModel>(options);

        Assert.NotNull(plant);

        return plant;
    }

    private async Task<string> CreatePlantScheduleToWorkWith(PlantHarvestCycleViewModel plant)
    {
        var response = await _plantHarvestClient.CreatePlantSchedule(plant.HarvestCycleId, plant.PlantHarvestCycleId);
        var returnString = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            Assert.NotEmpty(returnString);
            Assert.True(Guid.TryParse(returnString, out var plantHarvestCycleId));
        }
        else
        {
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
            returnString = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(returnString);
            Assert.Contains("This plant is already a part of this plan", returnString);
        }
        return returnString;
    }

    private async Task<string> CreateGardenBedPlantHarvestCycleToWorkWith(PlantHarvestCycleViewModel plant)
    {
        var response = await _plantHarvestClient.CreateGardenBedPlantHarvestCycle(plant.HarvestCycleId, plant.PlantHarvestCycleId);
        var returnString = await response.Content.ReadAsStringAsync();

        Assert.NotEmpty(returnString);
        Assert.True(Guid.TryParse(returnString, out var plantHarvestCycleId));

        return returnString;
    }
    #endregion
}
