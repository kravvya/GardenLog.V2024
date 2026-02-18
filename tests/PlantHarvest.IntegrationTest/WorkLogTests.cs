using GardenLog.SharedKernel.Enum;
using PlantHarvest.Contract.Enum;
using PlantHarvest.Contract.Query;
using PlantHarvest.Contract.ViewModels;
using PlantHarvest.IntegrationTest.Clients;
using PlantHarvest.IntegrationTest.Fixture;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace PlantHarvest.IntegrationTest;

public partial class PlantHarvestTests // : IClassFixture<PlantHarvestServiceFixture>
{
    //private readonly ITestOutputHelper _output;
    //private readonly WorkLogClient _workLogClient;
    //private readonly PlantHarvestClient _plantHarvestClient;

    //public WorkLogTests(PlantHarvestServiceFixture fixture, ITestOutputHelper output)
    //{
    //    _workLogClient = fixture.WorkLogClient;
    //    _plantHarvestClient=fixture.PlantHarvestClient;
    //    _output = output;
    //    _output.WriteLine($"Service id {fixture.FixtureId} @ {DateTime.Now.ToString("F")}");
    //}

    #region Work Log
    [Fact]
    public async Task Post_WorkLog_CreateNew_WorkLog()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(PlantHarvestTests.TEST_HARVEST_CYCLE_NAME);

        var response = await _workLogClient.CreateWorkLog(RelatedEntityTypEnum.HarvestCycle, harvestId);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to create work log responded with {response.StatusCode} code and {returnString} message");

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        Assert.NotEmpty(returnString);
        Assert.True(Guid.TryParse(returnString, out _));

    }

    [Fact]
    public async Task Post_WorkLog_ShouldNotCreateNewWorkLog_WithoutEntityId()
    {
        var response = await _workLogClient.CreateWorkLog(RelatedEntityTypEnum.HarvestCycle, string.Empty);

        var returnString = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"Service to create work log responded with {response.StatusCode} code and {returnString} message");


        Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        Assert.NotEmpty(returnString);
        Assert.Contains("'Related Entities' must not be empty.", returnString);
    }

    [Fact]
    public async Task Put_WorkLog_ShouldUpdateWorkLog()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(PlantHarvestTests.TEST_HARVEST_CYCLE_NAME);

        var workLogs = await GetWorkLogsToWorkWith(harvestId);

        if (workLogs != null && workLogs.Count > 0)
        {
            var work = workLogs.First();

            work.Log = $"{work.Log} last pdated: {DateTime.Now}";

            var response = await _workLogClient.UpdateWorkLog(work);

            var returnString = await response.Content.ReadAsStringAsync();

            _output.WriteLine($"Service to update worklog responded with {response.StatusCode} code and {returnString} message");

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
            Assert.NotEmpty(returnString);
        }


    }

    [Fact]
    public async Task Delete_HarvestCycleWorkLog_ShouldDelete()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(PlantHarvestTests.TEST_HARVEST_CYCLE_NAME);

        var workLogs = await GetWorkLogsToWorkWith(harvestId);

        if (workLogs != null && workLogs.Count > 0)
        {
            var work = workLogs.First();

            var response = await _workLogClient.DeleteWorkLog(work.WorkLogId);

            var returnString = await response.Content.ReadAsStringAsync();

            _output.WriteLine($"Service to delete work log responded with {response.StatusCode} code and {returnString} message");

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
            Assert.NotEmpty(returnString);
        }
    }

    [Fact]
    public async Task Get_Should_Return_WorkLogs()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(PlantHarvestTests.TEST_HARVEST_CYCLE_NAME);

        var workLogs = await GetWorkLogsToWorkWith(harvestId);
        if (workLogs.Count == 0)
        {
            await _workLogClient.CreateWorkLog(RelatedEntityTypEnum.HarvestCycle, harvestId);
        }
        workLogs = await GetWorkLogsToWorkWith(harvestId);

        Assert.NotNull(workLogs);
        Assert.NotEmpty(workLogs);
    }

    [Fact]
    public async Task Get_SearchWorkLogs_Should_Return_FilteredRecords()
    {
        var harvestId = await _plantHarvestClient.GetHarvestCycleIdToWorkWith(PlantHarvestTests.TEST_HARVEST_CYCLE_NAME);

        var createResponse = await _workLogClient.CreateWorkLog(RelatedEntityTypEnum.HarvestCycle, harvestId);
        Assert.Equal(System.Net.HttpStatusCode.OK, createResponse.StatusCode);

        var search = new WorkLogSearch
        {
            StartDate = DateTime.UtcNow.AddDays(-2),
            EndDate = DateTime.UtcNow.AddDays(2),
            Reason = WorkLogReasonEnum.Information,
            Limit = 50
        };

        var searchResponse = await _workLogClient.SearchWorkLogs(search);
        var returnString = await searchResponse.Content.ReadAsStringAsync();

        _output.WriteLine($"Search WorkLogs responded with {searchResponse.StatusCode} code and {returnString} message");

        Assert.Equal(System.Net.HttpStatusCode.OK, searchResponse.StatusCode);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
                {
                    new JsonStringEnumConverter(),
                },
        };

        var workLogs = await searchResponse.Content.ReadFromJsonAsync<List<WorkLogViewModel>>(options);
        Assert.NotNull(workLogs);
        Assert.NotEmpty(workLogs);
        Assert.Contains(workLogs, w => w.Reason == WorkLogReasonEnum.Information);
    }

    #endregion

    private async Task<List<WorkLogViewModel>> GetWorkLogsToWorkWith(string harvestId)
    {
        var response = await _workLogClient.GetWorkLogs(RelatedEntityTypEnum.HarvestCycle, harvestId);

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

        var workLogs = await response.Content.ReadFromJsonAsync<List<WorkLogViewModel>>(options);

        return workLogs!;
    }
}
