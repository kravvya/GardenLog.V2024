using Azure;
using PlantHarvest.Contract.ViewModels;
using PlantHarvest.IntegrationTest.Clients;
using PlantHarvest.IntegrationTest.Fixture;
using SendGrid;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace PlantHarvest.IntegrationTest;

public partial class PlantHarvestTests // : IClassFixture<PlantHarvestServiceFixture>
{
    //private readonly ITestOutputHelper _output;
    //private readonly PlantTaskClient _workLogClient;
    //private readonly PlantHarvestClient _plantHarvestClient;

    //public PlantTaskTests(PlantHarvestServiceFixture fixture, ITestOutputHelper output)
    //{
    //    _workLogClient = fixture.PlantTaskClient;
    //    _plantHarvestClient=fixture.PlantHarvestClient;
    //    _output = output;
    //    _output.WriteLine($"Service id {fixture.FixtureId} @ {DateTime.Now.ToString("F")}");
    //}

    #region Plant Task
    [Fact]
    public async Task Post_PlantTask_CreateNew_PlantTask()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(PlantHarvestTests.TEST_HARVEST_CYCLE_NAME);
        var plantHarvest = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, PlantHarvestTests.TEST_PLANT_ID, PlantHarvestTests.TEST_PLANT_VARIETY_ID);

        var response = await _plantTaskClient.CreatePlantTask(harvestId, plantHarvest.PlantHarvestCycleId);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to create task responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
        Assert.True(Guid.TryParse(returnString, out var plantTaskId));

    }

    [Fact]
    public async Task Post_PlantTask_ShouldNotCreateNewPlantTask_WithoutPlantHarvestId()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(PlantHarvestTests.TEST_HARVEST_CYCLE_NAME);

        var response = await _plantTaskClient.CreatePlantTask(harvestId, string.Empty);

        var returnString = await response.Content.ReadAsStringAsync();


        _output.WriteLine($"Service to create work log responded with {response.StatusCode} code and {returnString} message");


        Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        Assert.NotEmpty(returnString);
        Assert.Contains("'Plant Harvest Cycle Id' must not be empty.", returnString);
    }

    [Fact]
    public async Task Put_PlantTask_ShouldUpdatePlantTask()
    {
        var tasks = await GetPlantTasksToWorkWith();

        if (tasks != null && tasks.Count > 0)
        {
            var task = tasks.First();

            task.Notes = $"{task.Notes} last pdated: {DateTime.Now.ToString()}";

            var response = await _plantTaskClient.UpdatePlantTask(task);

            var returnString = await response.Content.ReadAsStringAsync();

            _output.WriteLine($"Service to update worklog responded with {response.StatusCode} code and {returnString} message");

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
            Assert.NotEmpty(returnString);
        }
    }

    [Fact]
    public async Task Complete_HarvestCyclePlantTask_ShouldComplete()
    {
        var tasks = await GetActivePlantTasksToWorkWith();

        if (tasks == null || tasks.Count == 0)
        {
            var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(PlantHarvestTests.TEST_HARVEST_CYCLE_NAME);
            var plantHarvest = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, PlantHarvestTests.TEST_PLANT_ID, PlantHarvestTests.TEST_PLANT_VARIETY_ID);
            await _plantTaskClient.CreatePlantTask(harvestId, plantHarvest.PlantHarvestCycleId);
        }

        tasks = await GetActivePlantTasksToWorkWith();

        var task = tasks.First();

        task.CompletedDateTime = DateTime.Now;

        var response = await _plantTaskClient.CompletePlantTask(task);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to update task responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);

    }

    [Fact]
    public async Task Get_Should_Return_PlantTasks()
    {
        var tasks = await GetPlantTasksToWorkWith();

        Assert.True(tasks.Count > 0);
    }

    [Fact]
    public async Task Search_Should_Return_PlantTasksInJson()
    {
        var response = await _plantTaskClient.SearchTasks("json");

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(response.Content);

        var returnString = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Service to update task responded with {response.StatusCode} code and {returnString} message");

        Assert.NotEmpty(returnString);
    }

    [Fact]
    public async Task Search_Should_Return_PlantTasksInHtml()
    {
        var response = await _plantTaskClient.SearchTasks("html");

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(response.Content);

        var returnString = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Service to update task responded with {response.StatusCode} code and {returnString} message");

        Assert.NotEmpty(returnString);
        Assert.Contains("table", returnString);
    }

    [Fact]
    public async Task Get_Should_Return_ActivePlantTasks()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(PlantHarvestTests.TEST_HARVEST_CYCLE_NAME);
        var plantHarvest = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, PlantHarvestTests.TEST_PLANT_ID, PlantHarvestTests.TEST_PLANT_VARIETY_ID);
        await _plantTaskClient.CreatePlantTask(harvestId, plantHarvest.PlantHarvestCycleId);

        var tasks = await GetActivePlantTasksToWorkWith();

        Assert.True(tasks.Count > 0);
    }

    [Fact]
    public async Task Get_Should_DeleteNotCompletedSystemTasks()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(PlantHarvestTests.TEST_HARVEST_CYCLE_NAME);
        var plantHarvest = await _plantHarvestClient.GetPlantHarvestCycleToWorkWith(harvestId, PlantHarvestTests.TEST_PLANT_ID, PlantHarvestTests.TEST_PLANT_VARIETY_ID);
        await _plantTaskClient.CreatePlantTask(harvestId, plantHarvest.PlantHarvestCycleId);

        var response = await _plantTaskClient.DeleteSystemTasks(plantHarvest.PlantHarvestCycleId);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to delete system tasks {response.StatusCode} code and {returnString} message");
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
        Assert.True(long.TryParse(returnString, out var counter));
    }

    [Fact]
    public async Task Get_PlantTask_Completed_Count()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(PlantHarvestTests.TEST_HARVEST_CYCLE_NAME);

        var response = await _plantTaskClient.GetCompleteTaskCount(harvestId);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to get count of compelted tasks {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
        Assert.True(long.TryParse(returnString, out var counter));

    }

    #endregion

    private async Task<List<PlantTaskViewModel>> GetPlantTasksToWorkWith()
    {
        var response = await _plantTaskClient.GetPlantTasks();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                {
                    new JsonStringEnumConverter(),
                },
        };

        var returnString = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Service for tasks responded with {response.StatusCode} code and {returnString} message");

        var tasks = await response.Content.ReadFromJsonAsync<List<PlantTaskViewModel>>(options);

        return tasks!;
    }

    private async Task<List<PlantTaskViewModel>> GetActivePlantTasksToWorkWith()
    {
        var response = await _plantTaskClient.GetActivePlantTasks();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                {
                    new JsonStringEnumConverter(),
                },
        };

        var returnString = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Service for Active tasks responded with {response.StatusCode} code and {returnString} message");

        var tasks = await response.Content.ReadFromJsonAsync<List<PlantTaskViewModel>>(options);

        return tasks!;
    }
}
