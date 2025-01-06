

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlantCatalog.Contract.ViewModels;
using PlantHarvest.Api.EventHandlers.Tasks;
using PlantHarvest.Api.Schedules;
using PlantHarvest.Domain.HarvestAggregate;
using PlantHarvest.Domain.HarvestAggregate.Events;
using PlantHarvest.Domain.WorkLogAggregate.Events;
using PlantHarvest.Infrastructure.ApiClients;
using System.Collections.ObjectModel;
using System.Reflection;
using plantCatalog = PlantCatalog;

namespace PlantHarvest.UnitTest.EventHandlers.Tasks;

public class TaskGeneratorsShould
{
    Mock<IPlantTaskCommandHandler> _taskCommandHandlerMock;
    Mock<IPlantTaskQueryHandler> _taskQueryHandlerMock;

    IPlantCatalogApiClient _plantCatalogApiClient;
    Mock<IHarvestQueryHandler> _harvestQueryHandlerMock;

    public TaskGeneratorsShould()
    {
        _taskCommandHandlerMock = new Mock<IPlantTaskCommandHandler>();
        _taskQueryHandlerMock = new Mock<IPlantTaskQueryHandler>();
        _harvestQueryHandlerMock = new Mock<IHarvestQueryHandler>();

        SetupHarvestQueryHandlerMock();

        var httpPlantCatalogClient = HttpClientsHelper.GetPlantCatalogHttpClientForGrowInstructions(PlantCatalog.Contract.Enum.PlantingMethodEnum.SeedIndoors);

        var configMock = new Mock<IConfiguration>();
        configMock.SetupGet(x => x[It.Is<string>(s => s == "Services:PlantCatalog.Api")]).Returns(HttpClientsHelper.PLANT_CATALOG_URL);
        IConfiguration configuration = configMock.Object;

        ILogger<PlantCatalogApiClient> loggerPlantCatalogClient = new Mock<ILogger<PlantCatalogApiClient>>().Object;

        IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

        _plantCatalogApiClient = new PlantCatalogApiClient(httpPlantCatalogClient, configuration, loggerPlantCatalogClient, memoryCache);


    }

    #region "Sow Indoors"
    [Fact]
    public async Task TaskGenerator_Creates_SowInside_Task_When_PlantHarvest_Created()
    {
        var IndoorSawTaskGenerator = new IndoorSawTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantAddedToHarvestCycle);

        harvest.AddPlantSchedule(HarvestHelper.GetCommandToCreateSchedule(plantHarvestId, WorkLogReasonEnum.SowIndoors));

        await IndoorSawTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Type == WorkLogReasonEnum.SowIndoors)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Deletes_SowInside_Task_When_PlantHarvest_Removed()
    {

        var IndoorSawTaskGenerator = new IndoorSawTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.DeletePlantHarvestCycle(plantHarvestId);
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleDeleted);

        await IndoorSawTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Complete_SowIndoor_Task_When_PlantHarvest_Is_Seeded()
    {
        var IndoorSawTaskGenerator = new IndoorSawTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object);

        //await IndoorSawTaskGenerator.Handle(HarvestHelper.GetWorkLogEvent(WorkLogEventTriggerEnum.WorkLogCreated
        //                                                                    , HarvestHelper.PLANT_HARVEST_CYCLE_ID
        //                                                                    , WorkLogReasonEnum.SowIndoors), new CancellationToken());

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { SeedingDate = DateTime.UtcNow, NumberOfSeeds = 101, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleSeeded);

        await IndoorSawTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());


        _taskCommandHandlerMock.Verify(t => t.CompletePlantTask(It.Is<UpdatePlantTaskCommand>(c => c.PlantTaskId == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }
    #endregion

    #region "Sow Outdoors"
    [Fact]
    public async Task TaskGenerator_Creates_SowOutside_Task_When_PlantHarvest_Created()
    {
        var outsideSawTaskGenerator = new OutsideSawTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantAddedToHarvestCycle);

        harvest.AddPlantSchedule(HarvestHelper.GetCommandToCreateSchedule(plantHarvestId, WorkLogReasonEnum.SowOutside));

        await outsideSawTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Type == WorkLogReasonEnum.SowOutside)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Deletes_SowOutside_Task_When_PlantHarvest_Removed()
    {

        var outsideSawTaskGenerator = new OutsideSawTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.DeletePlantHarvestCycle(plantHarvestId);
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleDeleted);

        await outsideSawTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Complete_SowOutside_Task_When_PlantHarvest_Is_Seeded()
    {
        var outsideSawTaskGenerator = new OutsideSawTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { SeedingDate = DateTime.UtcNow, NumberOfSeeds = 101, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleSeeded);

        await outsideSawTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());


        _taskCommandHandlerMock.Verify(t => t.CompletePlantTask(It.Is<UpdatePlantTaskCommand>(c => c.PlantTaskId == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }
    #endregion

    #region "Indoor Fertilize"

    [Fact]
    public async Task TaskGenerator_Creates_FertilizeIndoor_Task_When_PlantHarvest_Germinated()
    {
        var IndoorSawTaskGenerator = new IndoorFertilizeTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, _harvestQueryHandlerMock.Object, new Mock<ILogger<IndoorFertilizeTaskGenerator>>().Object);


        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { GerminationDate = DateTime.UtcNow, GerminationRate = 80, PlantingMethod = Contract.Enum.PlantingMethodEnum.SeedIndoors, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleGerminated);

        await IndoorSawTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());


        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Type == WorkLogReasonEnum.FertilizeIndoors)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Deletes_FertilizeIndoor_Task_When_PlantHarvest_Removed()
    {
        var IndoorFetilizeTaskGenerator = new IndoorFertilizeTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, _harvestQueryHandlerMock.Object, new Mock<ILogger<IndoorFertilizeTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.DeletePlantHarvestCycle(plantHarvestId);
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleDeleted);

        await IndoorFetilizeTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Deletes_FertilizeIndoor_Task_When_PlantHarvest_Transplanted()
    {
        var IndoorFetilizeTaskGenerator = new IndoorFertilizeTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, _harvestQueryHandlerMock.Object, new Mock<ILogger<IndoorFertilizeTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { TransplantDate = DateTime.UtcNow, NumberOfSeeds = 101, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });

        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleTransplanted);

        await IndoorFetilizeTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Complete_FertilizeIndoor_Task_When_WorkLog_FertilizeIndoors_CreateNewFertilizeTask()
    {
        var schedule = HarvestHelper.GetPlantScheduleViewModel(HarvestHelper.PLANT_HARVEST_CYCLE_ID, WorkLogReasonEnum.TransplantOutside, DateTime.Now.AddDays(90));
        var plantHarvest = HarvestHelper.GetPlantHarvestCycleViewModel(Contract.Enum.PlantingMethodEnum.SeedIndoors, schedule);
        plantHarvest.GerminationDate = DateTime.Now;

        _harvestQueryHandlerMock.SetupGet(x => x.GetPlantHarvestCycle(It.IsAny<string>(), It.IsAny<string>()).Result).Returns(plantHarvest);

        var IndoorFetilizeTaskGenerator = new IndoorFertilizeTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, _harvestQueryHandlerMock.Object, new Mock<ILogger<IndoorFertilizeTaskGenerator>>().Object);

        var workLog = HarvestHelper.GetWorkLog(HarvestHelper.HARVEST_CYCLE_ID, "Test Harvest", HarvestHelper.PLANT_HARVEST_CYCLE_ID, "Test Plant", WorkLogReasonEnum.FertilizeIndoors);

        var evt = workLog.DomainEvents.First(e => ((WorkLogEvent)e).Work!.Reason == WorkLogReasonEnum.FertilizeIndoors);

        await IndoorFetilizeTaskGenerator.Handle((WorkLogEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.CompletePlantTask(It.Is<UpdatePlantTaskCommand>(c => c.PlantTaskId == HarvestHelper.PLANT_TASK_ID)), Times.Once);
        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Type == WorkLogReasonEnum.FertilizeIndoors)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Complete_FertilizeIndoor_Task_When_WorkLog_FertilizeIndoors_DoNotCreateNewFertilizeTask()
    {
        var schedule = HarvestHelper.GetPlantScheduleViewModel(HarvestHelper.PLANT_HARVEST_CYCLE_ID, WorkLogReasonEnum.TransplantOutside, DateTime.Now.AddDays(1));
        var plantHarvest = HarvestHelper.GetPlantHarvestCycleViewModel(Contract.Enum.PlantingMethodEnum.SeedIndoors, schedule);
        plantHarvest.GerminationDate = DateTime.Now;

        _harvestQueryHandlerMock.SetupGet(x => x.GetPlantHarvestCycle(It.IsAny<string>(), It.IsAny<string>()).Result).Returns(plantHarvest);

        var IndoorFetilizeTaskGenerator = new IndoorFertilizeTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, _harvestQueryHandlerMock.Object, new Mock<ILogger<IndoorFertilizeTaskGenerator>>().Object);

        var workLog = HarvestHelper.GetWorkLog(HarvestHelper.HARVEST_CYCLE_ID, "Test Harvest", HarvestHelper.PLANT_HARVEST_CYCLE_ID, "Test Plant", WorkLogReasonEnum.FertilizeIndoors);

        var evt = workLog.DomainEvents.First(e => ((WorkLogEvent)e).Work!.Reason == WorkLogReasonEnum.FertilizeIndoors);

        await IndoorFetilizeTaskGenerator.Handle((WorkLogEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.CompletePlantTask(It.Is<UpdatePlantTaskCommand>(c => c.PlantTaskId == HarvestHelper.PLANT_TASK_ID)), Times.Once);
        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Type == WorkLogReasonEnum.FertilizeIndoors)), Times.Never);

    }

    #endregion

    #region "Harden off"
    [Fact]
    public async Task TaskGenerator_Creates_HardenOff_Task_When_PlantHarvest_Seeded()
    {
        var hardenOffTaskGenerator = new HardenOffTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _harvestQueryHandlerMock.Object, new Mock<ILogger<HardenOffTaskGenerator>>().Object);


        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { SeedingDate = DateTime.Now, NumberOfSeeds = 100, PlantingMethod = Contract.Enum.PlantingMethodEnum.SeedIndoors, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleSeeded);

        harvest.AddPlantSchedule(HarvestHelper.GetCommandToCreateSchedule(plantHarvestId, WorkLogReasonEnum.TransplantOutside));

        await hardenOffTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Type == WorkLogReasonEnum.Harden)), Times.Once);

    }

    [Fact]
    public async Task TaskGenerator_Complete_HardenOff_Task_When_PlantHarvest_Is_Transplanted()
    {
        var hardenOffTaskGenerator = new HardenOffTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _harvestQueryHandlerMock.Object, new Mock<ILogger<HardenOffTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { TransplantDate = DateTime.UtcNow, NumberOfTransplants = 50, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleTransplanted);

        await hardenOffTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());


        _taskCommandHandlerMock.Verify(t => t.CompletePlantTask(It.Is<UpdatePlantTaskCommand>(c => c.PlantTaskId == HarvestHelper.PLANT_TASK_ID)), Times.Exactly(1));
    }

    [Fact]
    public async Task TaskGenerator_Deletes_HardenOff_Task_When_PlantHarvest_Removed()
    {
        var hardenOffTaskGenerator = new HardenOffTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _harvestQueryHandlerMock.Object, new Mock<ILogger<HardenOffTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.DeletePlantHarvestCycle(plantHarvestId);
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleDeleted);

        await hardenOffTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Exactly(1));
    }

    [Fact]
    public async Task TaskGenerator_Complete_HardenOff_Task_When_WorkLog_HardenOff_CreateNewFertilizeTask()
    {
        var schedule = HarvestHelper.GetPlantScheduleViewModel(HarvestHelper.PLANT_HARVEST_CYCLE_ID, WorkLogReasonEnum.TransplantOutside, DateTime.Now.AddDays(90));
        var plantHarvest = HarvestHelper.GetPlantHarvestCycleViewModel(Contract.Enum.PlantingMethodEnum.SeedIndoors, schedule);
        plantHarvest.GerminationDate = DateTime.Now;

        _harvestQueryHandlerMock.SetupGet(x => x.GetPlantHarvestCycle(It.IsAny<string>(), It.IsAny<string>()).Result).Returns(plantHarvest);

        var hardenOffTaskGenerator = new HardenOffTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _harvestQueryHandlerMock.Object, new Mock<ILogger<HardenOffTaskGenerator>>().Object);

        var workLog = HarvestHelper.GetWorkLog(HarvestHelper.HARVEST_CYCLE_ID, "Test Harvest", HarvestHelper.PLANT_HARVEST_CYCLE_ID, "Test Plant", WorkLogReasonEnum.Harden);

        var evt = workLog.DomainEvents.First(e => ((WorkLogEvent)e).Work!.Reason == WorkLogReasonEnum.Harden);

        await hardenOffTaskGenerator.Handle((WorkLogEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.CompletePlantTask(It.Is<UpdatePlantTaskCommand>(c => c.PlantTaskId == HarvestHelper.PLANT_TASK_ID)), Times.Once);
        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Type == WorkLogReasonEnum.Harden)), Times.Once);
    }
    #endregion

    #region "Transplant Outside"
    [Fact]
    public async Task TaskGenerator_Creates_TransplantOutside_Task_When_PlantHarvest_Seeded()
    {
        var tranplantOutsideTaskGenerator = new TransplantOutsideTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object);


        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { SeedingDate = DateTime.Now, NumberOfSeeds = 100, PlantingMethod = Contract.Enum.PlantingMethodEnum.SeedIndoors, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleSeeded);

        harvest.AddPlantSchedule(HarvestHelper.GetCommandToCreateSchedule(plantHarvestId, WorkLogReasonEnum.TransplantOutside));

        await tranplantOutsideTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Type == WorkLogReasonEnum.TransplantOutside)), Times.Once);

    }

    [Fact]
    public async Task TaskGenerator_Complete_TransplantOutside_Task_When_PlantHarvest_Is_Transplanted()
    {
        var tranplantOutsideTaskGenerator = new TransplantOutsideTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object);

        //await IndoorSawTaskGenerator.Handle(HarvestHelper.GetWorkLogEvent(WorkLogEventTriggerEnum.WorkLogCreated
        //                                                                    , HarvestHelper.PLANT_HARVEST_CYCLE_ID
        //                                                                    , WorkLogReasonEnum.SowIndoors), new CancellationToken());

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { TransplantDate = DateTime.UtcNow, NumberOfTransplants = 50, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleTransplanted);

        await tranplantOutsideTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());


        _taskCommandHandlerMock.Verify(t => t.CompletePlantTask(It.Is<UpdatePlantTaskCommand>(c => c.PlantTaskId == HarvestHelper.PLANT_TASK_ID)), Times.Exactly(1));
    }

    [Fact]
    public async Task TaskGenerator_Deletes_TransplantOutside_Task_When_PlantHarvest_Removed()
    {
        var tranplantOutsideTaskGenerator = new TransplantOutsideTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.DeletePlantHarvestCycle(plantHarvestId);
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleDeleted);

        await tranplantOutsideTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Exactly(1));
    }
    #endregion

    #region "Outdoor Fertilize"

    [Fact]
    public async Task TaskGenerator_Creates_FertilizeOutside_Task_When_PlantHarvest_Transplanted()
    {
        var outdoorSawTaskGenerator = new OutdoorFertilizeTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, _harvestQueryHandlerMock.Object, new Mock<ILogger<OutdoorFertilizeTaskGenerator>>().Object);


        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { TransplantDate = DateTime.UtcNow, NumberOfTransplants = 100, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleTransplanted);

        await outdoorSawTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());


        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Type == WorkLogReasonEnum.FertilizeOutside)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Deletes_FertilizeOutside_Task_When_PlantHarvest_Removed()
    {
        var outdoorSawTaskGenerator = new OutdoorFertilizeTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, _harvestQueryHandlerMock.Object, new Mock<ILogger<OutdoorFertilizeTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.DeletePlantHarvestCycle(plantHarvestId);
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleDeleted);

        await outdoorSawTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Deletes_FertilizeOutside_Task_When_PlantHarvest_Harvested()
    {
        var outdoorSawTaskGenerator = new OutdoorFertilizeTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, _harvestQueryHandlerMock.Object, new Mock<ILogger<OutdoorFertilizeTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { FirstHarvestDate = DateTime.UtcNow, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });

        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleHarvested);

        await outdoorSawTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Deletes_FertilizeOutside_Task_When_PlantHarvest_Completed()
    {
        var outdoorSawTaskGenerator = new OutdoorFertilizeTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, _harvestQueryHandlerMock.Object, new Mock<ILogger<OutdoorFertilizeTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { LastHarvestDate = DateTime.UtcNow, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });

        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleCompleted);

        await outdoorSawTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Completes_FertilizeOutside_Task_When_WorkLog_FertilizeIndoors_CreateNewFertilizeTask()
    {
        var schedule = HarvestHelper.GetPlantScheduleViewModel(HarvestHelper.PLANT_HARVEST_CYCLE_ID, WorkLogReasonEnum.Harvest, DateTime.Now.AddDays(90));
        var plantHarvest = HarvestHelper.GetPlantHarvestCycleViewModel(Contract.Enum.PlantingMethodEnum.DirectSeed, schedule);
        plantHarvest.TransplantDate = DateTime.Now;

        _harvestQueryHandlerMock.SetupGet(x => x.GetPlantHarvestCycle(It.IsAny<string>(), It.IsAny<string>()).Result).Returns(plantHarvest);

        var outdoorSawTaskGenerator = new OutdoorFertilizeTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, _harvestQueryHandlerMock.Object, new Mock<ILogger<OutdoorFertilizeTaskGenerator>>().Object);

        var workLog = HarvestHelper.GetWorkLog(HarvestHelper.HARVEST_CYCLE_ID, "Test Harvest", HarvestHelper.PLANT_HARVEST_CYCLE_ID, "Test Plant", WorkLogReasonEnum.FertilizeOutside);

        var evt = workLog.DomainEvents.First(e => ((WorkLogEvent)e).Work!.Reason == WorkLogReasonEnum.FertilizeOutside);

        await outdoorSawTaskGenerator.Handle((WorkLogEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.CompletePlantTask(It.Is<UpdatePlantTaskCommand>(c => c.PlantTaskId == HarvestHelper.PLANT_TASK_ID)), Times.Once);
        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Type == WorkLogReasonEnum.FertilizeOutside)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Completes_Fertilize_Task_When_WorkLog_FertilizeIndoors_DoNotCreateNewFertilizeTask()
    {
        var schedule = HarvestHelper.GetPlantScheduleViewModel(HarvestHelper.PLANT_HARVEST_CYCLE_ID, WorkLogReasonEnum.Harvest, DateTime.Now.AddDays(1));
        var plantHarvest = HarvestHelper.GetPlantHarvestCycleViewModel(Contract.Enum.PlantingMethodEnum.SeedIndoors, schedule);
        plantHarvest.TransplantDate = DateTime.Now;

        _harvestQueryHandlerMock.SetupGet(x => x.GetPlantHarvestCycle(It.IsAny<string>(), It.IsAny<string>()).Result).Returns(plantHarvest);

        var outdoorSawTaskGenerator = new OutdoorFertilizeTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, _harvestQueryHandlerMock.Object, new Mock<ILogger<OutdoorFertilizeTaskGenerator>>().Object);

        var workLog = HarvestHelper.GetWorkLog(HarvestHelper.HARVEST_CYCLE_ID, "Test Harvest", HarvestHelper.PLANT_HARVEST_CYCLE_ID, "Test Plant", WorkLogReasonEnum.FertilizeOutside);

        var evt = workLog.DomainEvents.First(e => ((WorkLogEvent)e).Work!.Reason == WorkLogReasonEnum.FertilizeOutside);

        await outdoorSawTaskGenerator.Handle((WorkLogEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.CompletePlantTask(It.Is<UpdatePlantTaskCommand>(c => c.PlantTaskId == HarvestHelper.PLANT_TASK_ID)), Times.Once);
        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Type == WorkLogReasonEnum.FertilizeOutside)), Times.Never);

    }

    #endregion

    #region Harvest and LastHarvest

    [Fact]
    public async Task TaskGenerator_Creates_Harvest_Task_When_PlantHarvest_Transplanted()
    {
        var harvestTaskGenerator = new HarvestTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, new Mock<ILogger<HarvestTaskGenerator>>().Object);


        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { TransplantDate = DateTime.UtcNow, NumberOfTransplants = 100, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleTransplanted);

        await harvestTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());


        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Type == WorkLogReasonEnum.Harvest)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Creates_Harvest_Task_When_PlantHarvest_Germinated_DirectSeedOnly()
    {
        var harvestTaskGenerator = new HarvestTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, new Mock<ILogger<HarvestTaskGenerator>>().Object);


        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { GerminationDate = DateTime.UtcNow, NumberOfTransplants = 100, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleGerminated);

        await harvestTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());


        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Type == WorkLogReasonEnum.Harvest)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Deletes_Harvest_Task_When_PlantHarvest_Removed()
    {
        var harvestTaskGenerator = new HarvestTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, new Mock<ILogger<HarvestTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.DeletePlantHarvestCycle(plantHarvestId);
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleDeleted);

        await harvestTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Deletes_Harvest_Task_When_PlantHarvest_Complete()
    {
        var harvestTaskGenerator = new HarvestTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, new Mock<ILogger<HarvestTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { LastHarvestDate = DateTime.UtcNow, TotalItems = 50, TotalWeightInPounds = 100, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleCompleted);


        await harvestTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Completes_Harvest_Task_When_PlantHarvest_Harvested()
    {
        var harvestTaskGenerator = new HarvestTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, new Mock<ILogger<HarvestTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { FirstHarvestDate = DateTime.UtcNow, TotalItems = 50, TotalWeightInPounds = 100, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleHarvested);


        await harvestTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.CompletePlantTask(It.Is<UpdatePlantTaskCommand>(c => c.PlantTaskId == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }
    #endregion

    #region Record Germination Date

    [Fact]
    public async Task TaskGenerator_Creates_Germinate_Task_When_PlantHarvest_Seeded()
    {
        var germinateTaskGenerator = new GerminateTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, new Mock<ILogger<GerminateTaskGenerator>>().Object);


        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { SeedingDate = DateTime.UtcNow, NumberOfSeeds = 100, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleSeeded);

        await germinateTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());


        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Type == WorkLogReasonEnum.Information)), Times.Once);
        _taskCommandHandlerMock.Verify(t => t.CreatePlantTask(It.Is<CreatePlantTaskCommand>(c => c.Title == GerminateTaskGenerator.GERMINATE_TASK_TITLE)), Times.Once);
    }


    [Fact]
    public async Task TaskGenerator_Deletes_Germinate_Task_When_PlantHarvest_Removed()
    {
        var germinateTaskGenerator = new GerminateTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, new Mock<ILogger<GerminateTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.DeletePlantHarvestCycle(plantHarvestId);
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleDeleted);

        await germinateTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Deletes_Germinate_Task_When_PlantHarvest_Transplanted()
    {
        var germinateTaskGenerator = new GerminateTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, new Mock<ILogger<GerminateTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { LastHarvestDate = DateTime.UtcNow, TotalItems = 50, TotalWeightInPounds = 100, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleCompleted);


        await germinateTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Deletes_Germinate_Task_When_PlantHarvest_Harvested()
    {
        var germinateTaskGenerator = new GerminateTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, new Mock<ILogger<GerminateTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { LastHarvestDate = DateTime.UtcNow, TotalItems = 50, TotalWeightInPounds = 100, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleCompleted);


        await germinateTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Deletes_Germinate_Task_When_PlantHarvest_Complete()
    {
        var germinateTaskGenerator = new GerminateTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, new Mock<ILogger<GerminateTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { LastHarvestDate = DateTime.UtcNow, TotalItems = 50, TotalWeightInPounds = 100, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleCompleted);


        await germinateTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.DeletePlantTask(It.Is<string>(c => c == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }

    [Fact]
    public async Task TaskGenerator_Completes_Germinate_Task_When_PlantHarvest_Germinated()
    {
        var germinateTaskGenerator = new GerminateTaskGenerator(_taskCommandHandlerMock.Object, _taskQueryHandlerMock.Object, _plantCatalogApiClient, new Mock<ILogger<GerminateTaskGenerator>>().Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { GerminationDate = DateTime.UtcNow, GerminationRate = 50, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleGerminated);


        await germinateTaskGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _taskCommandHandlerMock.Verify(t => t.CompletePlantTask(It.Is<UpdatePlantTaskCommand>(c => c.PlantTaskId == HarvestHelper.PLANT_TASK_ID)), Times.Once);
    }
    #endregion

    #region Planing Method Change
    [Fact]
    public void TaskGenerator_Publishes_PlantingMethodChanged_Event_When_PlantingMethod_Updated()
    {


        var garden = UserManagementHelper.GetGarden();
        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        var plantHarvest = harvest.Plants.First(p => p.Id == plantHarvestId);

        var growInstruction = PlantsHelper.GetGrowInstruction(plantCatalog.Contract.Enum.PlantingMethodEnum.DirectSeed);

        PopulateSchedules(garden, harvest, plantHarvest, growInstruction);

        // Assert that there are schedules
        Assert.NotEmpty(plantHarvest.PlantCalendar);

        // Assert that there is no schedule for DirectSeed
        Assert.Contains(plantHarvest.PlantCalendar, s => s.TaskType == WorkLogReasonEnum.SowOutside);


        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { PlantingMethod = Contract.Enum.PlantingMethodEnum.SeedIndoors, PlantHarvestCycleId = plantHarvestId });
        growInstruction = PlantsHelper.GetGrowInstruction(plantCatalog.Contract.Enum.PlantingMethodEnum.SeedIndoors);

        PopulateSchedules(garden, harvest, plantHarvest, growInstruction);

        // Assert that there are schedules
        Assert.NotEmpty(harvest.Plants.First(p => p.Id == plantHarvestId).PlantCalendar);

        // Assert that there are schedules
        Assert.NotEmpty(harvest.Plants.First(p => p.Id == plantHarvestId).PlantCalendar);

        // Assert that there is no schedule for DirectSeed
        Assert.DoesNotContain(harvest.Plants.First(p => p.Id == plantHarvestId).PlantCalendar, s => s.TaskType == WorkLogReasonEnum.SowOutside);



        Assert.NotNull(harvest.DomainEvents.FirstOrDefault(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantingMethodChanged));
        Assert.Single(harvest.DomainEvents, e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantingMethodChanged);
    }


    #endregion

    private void SetupHarvestQueryHandlerMock()
    {
        _taskQueryHandlerMock.SetupGet(x => x.SearchPlantTasks(It.Is<PlantTaskSearch>(s => s.Reason == WorkLogReasonEnum.SowIndoors)).Result)
       .Returns((new List<PlantTaskViewModel>()
         {
              HarvestHelper.GetPlantTaskViewModel(HarvestHelper.PLANT_TASK_ID, HarvestHelper.PLANT_HARVEST_CYCLE_ID, WorkLogReasonEnum.SowIndoors)
         }).AsReadOnly());

        _taskQueryHandlerMock.SetupGet(x => x.SearchPlantTasks(It.Is<PlantTaskSearch>(s => s.Reason == WorkLogReasonEnum.FertilizeIndoors)).Result)
         .Returns((new List<PlantTaskViewModel>()
           {
              HarvestHelper.GetPlantTaskViewModel(HarvestHelper.PLANT_TASK_ID, HarvestHelper.PLANT_HARVEST_CYCLE_ID, WorkLogReasonEnum.FertilizeIndoors)
           }).AsReadOnly());

        _taskQueryHandlerMock.SetupGet(x => x.SearchPlantTasks(It.Is<PlantTaskSearch>(s => s.Reason == WorkLogReasonEnum.TransplantOutside)).Result)
        .Returns((new List<PlantTaskViewModel>()
          {
              HarvestHelper.GetPlantTaskViewModel(HarvestHelper.PLANT_TASK_ID, HarvestHelper.PLANT_HARVEST_CYCLE_ID, WorkLogReasonEnum.TransplantOutside)
          }).AsReadOnly());

        _taskQueryHandlerMock.SetupGet(x => x.SearchPlantTasks(It.Is<PlantTaskSearch>(s => s.Reason == WorkLogReasonEnum.Harden)).Result)
        .Returns((new List<PlantTaskViewModel>()
          {
              HarvestHelper.GetPlantTaskViewModel(HarvestHelper.PLANT_TASK_ID, HarvestHelper.PLANT_HARVEST_CYCLE_ID, WorkLogReasonEnum.Harden)
          }).AsReadOnly());

        _taskQueryHandlerMock.SetupGet(x => x.SearchPlantTasks(It.Is<PlantTaskSearch>(s => s.Reason == WorkLogReasonEnum.SowOutside)).Result)
      .Returns((new List<PlantTaskViewModel>()
        {
              HarvestHelper.GetPlantTaskViewModel(HarvestHelper.PLANT_TASK_ID, HarvestHelper.PLANT_HARVEST_CYCLE_ID, WorkLogReasonEnum.SowOutside)
        }).AsReadOnly());

        _taskQueryHandlerMock.SetupGet(x => x.SearchPlantTasks(It.Is<PlantTaskSearch>(s => s.Reason == WorkLogReasonEnum.FertilizeOutside)).Result)
        .Returns((new List<PlantTaskViewModel>()
          {
              HarvestHelper.GetPlantTaskViewModel(HarvestHelper.PLANT_TASK_ID, HarvestHelper.PLANT_HARVEST_CYCLE_ID, WorkLogReasonEnum.FertilizeOutside)
          }).AsReadOnly());

        _taskQueryHandlerMock.SetupGet(x => x.SearchPlantTasks(It.Is<PlantTaskSearch>(s => s.Reason == WorkLogReasonEnum.Harvest)).Result)
       .Returns((new List<PlantTaskViewModel>()
         {
              HarvestHelper.GetPlantTaskViewModel(HarvestHelper.PLANT_TASK_ID, HarvestHelper.PLANT_HARVEST_CYCLE_ID, WorkLogReasonEnum.Harvest)
         }).AsReadOnly());

        _taskQueryHandlerMock.SetupGet(x => x.SearchPlantTasks(It.Is<PlantTaskSearch>(s => s.Reason == WorkLogReasonEnum.Information && s.IncludeResolvedTasks == false)).Result)
     .Returns((new List<PlantTaskViewModel>()
       {
              HarvestHelper.GetPlantTaskViewModel(HarvestHelper.PLANT_TASK_ID, HarvestHelper.PLANT_HARVEST_CYCLE_ID, WorkLogReasonEnum.Information, GerminateTaskGenerator.GERMINATE_TASK_TITLE)
       }).AsReadOnly());
    }

    private List<IScheduler> GetSchedulers()
    {
        List<IScheduler> schedulers = new();
        Assembly asm = Assembly.GetAssembly(typeof(ScheduleBuilder))!;

        foreach (Type type in asm.GetTypes())
        {
            if (type.GetInterfaces().Contains(typeof(IScheduler)))
            {
                IScheduler obj = (IScheduler)Activator.CreateInstance(type)!;
                Console.WriteLine("Instance created: " + obj.GetType().Name);
                schedulers.Add(obj);
            }
        }

        return schedulers;
    }

    private void PopulateSchedules(GardenViewModel garden, HarvestCycle harvest, PlantHarvestCycle plantHarvest, PlantGrowInstructionViewModel growInstruction)
    {
        List<CreatePlantScheduleCommand> plantSchedules = new List<CreatePlantScheduleCommand>();

        var schedulers = GetSchedulers();

        schedulers.Where(s => s.CanSchedule(growInstruction)).ToList().ForEach(s =>
        {
            var schedule = s.Schedule(plantHarvest, growInstruction, garden, null, null);
            if (schedule != null)
            {
                plantSchedules.Add(schedule);
            }
        });

        ApplyPlantSchedules(plantSchedules.AsReadOnly(), harvest, plantHarvest.Id);
    }

    private void ApplyPlantSchedules(ReadOnlyCollection<CreatePlantScheduleCommand>? schedules, HarvestCycle harvest, string plantHarvestCycleId)
    {
        if (schedules == null || schedules.Count == 0) { return; }

        harvest.DeleteAllSystemGeneratedSchedules(plantHarvestCycleId);

        foreach (var schedule in schedules)
        {
            schedule.PlantHarvestCycleId = plantHarvestCycleId;
            harvest.AddPlantSchedule(schedule);
        }

    }
}
