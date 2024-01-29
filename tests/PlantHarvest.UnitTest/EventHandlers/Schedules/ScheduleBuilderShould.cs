using Castle.Core.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PlantCatalog.Contract.ViewModels;
using PlantHarvest.Api.Schedules;
using PlantHarvest.Contract.Enum;
using PlantHarvest.Infrastructure.ApiClients;
using System.Net;

namespace PlantHarvest.UnitTest.EventHandlers.Schedules;

public class ScheduleBuilderShould
{
    IConfiguration _configuration;
    IMemoryCache _memoryCache;
    HttpClient _httpPlantCatalogClient;
    HttpClient _httpUserManagementClient;
    ILogger<PlantCatalogApiClient> _loggerPlantCatalogClient;
    ILogger<UserManagementApiClient> _loggerUserManagementClient;
    ILogger<ScheduleBuilder> _loggerScheduleBuilder;

    IPlantCatalogApiClient _apiClinet;
    IUserManagementApiClient _userManagementApiClient;


    public ScheduleBuilderShould()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.SetupGet(x => x[It.Is<string>(s => s == "Services:PlantCatalog.Api")]).Returns(HttpClientsHelper.PLANT_CATALOG_URL);
        configMock.SetupGet(x => x[It.Is<string>(s => s == "Services:UserManagement.Api")]).Returns(HttpClientsHelper.USER_MANAGEMENT_URL);
        _configuration = configMock.Object;

       _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _httpPlantCatalogClient = HttpClientsHelper.GetPlantCatalogHttpClientForGrowInstructions(PlantCatalog.Contract.Enum.PlantingMethodEnum.SeedIndoors);
        _httpUserManagementClient = HttpClientsHelper.GetUserManagementHttpClientForGarden();
        _loggerPlantCatalogClient = new Mock<ILogger<PlantCatalogApiClient>>().Object;
        _loggerUserManagementClient = new Mock<ILogger<UserManagementApiClient>>().Object;
        _loggerScheduleBuilder = new Mock<ILogger<ScheduleBuilder>>().Object;

        _apiClinet = new PlantCatalogApiClient(_httpPlantCatalogClient, _configuration, _loggerPlantCatalogClient, _memoryCache);
        _userManagementApiClient = new UserManagementApiClient(_httpUserManagementClient, _configuration, _loggerUserManagementClient, _memoryCache);
    }

    [Fact]
    public async Task ScheduleBuilder_Creates_IndoorSow_TransplantOutside_Harvst_SchdulesAsync()
    {
        var builder = new ScheduleBuilder(_apiClinet, _userManagementApiClient, _loggerScheduleBuilder);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));

        var schedues = await builder.GeneratePlantCalendarBasedOnGrowInstruction(harvest.Plants.First(p => p.Id == plantHarvestId), PlantsHelper.PLANT_ID, PlantsHelper.GROW_INSTRUCTION_ID, PlantsHelper.PLANT_VARIETY_ID,UserManagementHelper.GARDEN_ID);

        Assert.NotNull(schedues);
        Assert.NotEmpty(schedues);
        Assert.Contains(schedues, schedule => schedule.TaskType == WorkLogReasonEnum.SowIndoors);
        Assert.Contains(schedues, schedule => schedule.TaskType == WorkLogReasonEnum.TransplantOutside);
        Assert.Contains(schedues, schedule => schedule.TaskType == WorkLogReasonEnum.Harvest);
        Assert.Contains(schedues, schedule => schedule.Notes.Equals("Desired number of plants: 30. Seeds from Good Seeds. Fertilize with Half Strength Balanced. Start Seeding Instrunctions"));
    }
}
