using PlantHarvest.Api.EventHandlers.Tasks;
using PlantHarvest.Domain.HarvestAggregate.Events;

namespace PlantHarvest.UnitTest.EventHandlers.Tasks;

public class WorkLogGeneratorsShould
{
    Mock<IWorkLogCommandHandler> _workLogCommandHandlerMock;

    public WorkLogGeneratorsShould()
    {
        _workLogCommandHandlerMock = new Mock<IWorkLogCommandHandler>();
    }

    [Fact]
    public async Task WorkLogGenerator_Creates_WorkLog_When_PlantHarvest_Sends_Seeded_Event_Inside()
    {
        var workLogGenerator = new WorkLogGenerator(_workLogCommandHandlerMock.Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { SeedingDate = DateTime.UtcNow,  PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleSeeded);

        await workLogGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _workLogCommandHandlerMock.Verify(t => t.CreateWorkLog(It.Is<CreateWorkLogCommand>(c => c.Reason == WorkLogReasonEnum.SowIndoors)), Times.Once);
        _workLogCommandHandlerMock.Verify(t => t.CreateWorkLog(It.Is<CreateWorkLogCommand>(c => c.Log.Contains("Test Plant were seeded indoors on "))), Times.Once);
    }

    [Fact]
    public async Task WorkLogGenerator_Creates_WorkLog_When_PlantHarvest_Sends_Seeded_Event_Outside()
    {
        var workLogGenerator = new WorkLogGenerator(_workLogCommandHandlerMock.Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.DirectSeed));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { SeedingDate = DateTime.UtcNow,  PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleSeeded);

        await workLogGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _workLogCommandHandlerMock.Verify(t => t.CreateWorkLog(It.Is<CreateWorkLogCommand>(c => c.Reason == WorkLogReasonEnum.SowOutside)), Times.Once);
        _workLogCommandHandlerMock.Verify(t => t.CreateWorkLog(It.Is<CreateWorkLogCommand>(c => c.Log.Contains("Test Plant were seeded outside on "))), Times.Once);
    }

    [Fact]
    public async Task WorkLogGenerator_Creates_WorkLog_When_PlantHarvest_Sends_Germinated_Event()
    {
        var workLogGenerator = new WorkLogGenerator(_workLogCommandHandlerMock.Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { GerminationDate = DateTime.Now, GerminationRate = 80, SeedVendorName = "Good seeds", PlantHarvestCycleId = plantHarvestId });

        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleGerminated);

        await workLogGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _workLogCommandHandlerMock.Verify(t => t.CreateWorkLog(It.Is<CreateWorkLogCommand>(c => c.Reason == WorkLogReasonEnum.Information)), Times.Once);
        _workLogCommandHandlerMock.Verify(t => t.CreateWorkLog(It.Is<CreateWorkLogCommand>(c => c.Log.Equals($"80% germanation of Test Plantfrom Good seeds  were germinated on {DateTime.Now.ToShortDateString()} "))), Times.Once);
    }

    [Fact]
    public async Task WorkLogGenerator_Creates_WorkLog_When_PlantHarvest_Sends_Transplanted_Event()
    {
        var workLogGenerator = new WorkLogGenerator(_workLogCommandHandlerMock.Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { TransplantDate = DateTime.Now, NumberOfTransplants=50, PlantVarietyName="Test Vaiery", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleTransplanted);

        await workLogGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _workLogCommandHandlerMock.Verify(t => t.CreateWorkLog(It.Is<CreateWorkLogCommand>(c => c.Reason == WorkLogReasonEnum.TransplantOutside)), Times.Once);
        _workLogCommandHandlerMock.Verify(t => t.CreateWorkLog(It.Is<CreateWorkLogCommand>(c => c.Log.Contains($"50 plants of Test Plant-Test Vaiery  were transplanted outside on {DateTime.Now.ToShortDateString()} "))), Times.Once);
    }

    [Fact]
    public async Task WorkLogGenerator_Creates_WorkLog_When_PlantHarvest_Sends_Harvest_Event()
    {
        var workLogGenerator = new WorkLogGenerator(_workLogCommandHandlerMock.Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { FirstHarvestDate = DateTime.Now, TotalItems=50,  TotalWeightInPounds = 100, PlantVarietyName = "Test Vaiery", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleHarvested);

        await workLogGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _workLogCommandHandlerMock.Verify(t => t.CreateWorkLog(It.Is<CreateWorkLogCommand>(c => c.Reason == WorkLogReasonEnum.Harvest)), Times.Once);
        _workLogCommandHandlerMock.Verify(t => t.CreateWorkLog(It.Is<CreateWorkLogCommand>(c => c.Log.Contains($"50 of plants. 100lb of Test Plant-Test Vaiery  were harvested on {DateTime.Now.ToShortDateString()} "))), Times.Once);
    }

    [Fact]
    public async Task WorkLogGenerator_Creates_WorkLog_When_PlantHarvest_Sends_Completed_Event()
    {
        var workLogGenerator = new WorkLogGenerator(_workLogCommandHandlerMock.Object);

        var harvest = HarvestHelper.GetHarvestCycle();
        var plantHarvestId = harvest.AddPlantHarvestCycle(HarvestHelper.GetCommandToCreatePlantHarvestCycle(Contract.Enum.PlantingMethodEnum.SeedIndoors));
        harvest.UpdatePlantHarvestCycle(new UpdatePlantHarvestCycleCommand() { LastHarvestDate = DateTime.Now, TotalItems = 50, TotalWeightInPounds = 100, PlantVarietyName = "Test Vaiery", PlantHarvestCycleId = plantHarvestId });
        var evt = harvest.DomainEvents.First(e => ((HarvestEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleCompleted);

        await workLogGenerator.Handle((HarvestEvent)evt, new CancellationToken());

        _workLogCommandHandlerMock.Verify(t => t.CreateWorkLog(It.Is<CreateWorkLogCommand>(c => c.Reason == WorkLogReasonEnum.Harvest)), Times.Once);
        _workLogCommandHandlerMock.Verify(t => t.CreateWorkLog(It.Is<CreateWorkLogCommand>(c => c.Log.Contains($"50 of plants. 100lb of Test Plant-Test Vaiery  were completely harvested on {DateTime.Now.ToShortDateString()} "))), Times.Once);
    }
}
