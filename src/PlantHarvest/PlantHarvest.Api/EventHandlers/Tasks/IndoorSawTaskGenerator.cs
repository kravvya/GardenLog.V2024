using PlantHarvest.Domain.HarvestAggregate.Events;
using PlantHarvest.Domain.WorkLogAggregate.Events;

namespace PlantHarvest.Orchestrator.Tasks;


public class IndoorSawTaskGenerator : INotificationHandler<HarvestEvent>//, INotificationHandler<WorkLogEvent>
{
    private readonly IPlantTaskCommandHandler _taskCommandHandler;
    private readonly IPlantTaskQueryHandler _taskQueryHandler;

    public IndoorSawTaskGenerator(IPlantTaskCommandHandler taskCommandHandler, IPlantTaskQueryHandler taskQueryHandler)
    {
        _taskCommandHandler = taskCommandHandler;
        _taskQueryHandler = taskQueryHandler;
    }

    public async Task Handle(HarvestEvent harvestEvent, CancellationToken cancellationToken)
    {
        switch (harvestEvent.Trigger)
        {
            case HarvestEventTriggerEnum.PlantAddedToHarvestCycle:
            case HarvestEventTriggerEnum.PlantingMethodChanged:
                await CreateIndoorSowTask(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleSeeded:
                await CompleteIndoorSowTasks(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleDeleted:
                await DeleteIndoorSowTask(harvestEvent);
                break;

        }
    }

    //public async Task Handle(WorkLogEvent workEvent, CancellationToken cancellationToken)
    //{
    //    if (workEvent.Work.Reason == WorkLogReasonEnum.SowIndoors && !string.IsNullOrEmpty(workEvent.Work.RelatedEntityid))
    //    {
    //        await CompleteIndoorSowTasks(workEvent);
    //    }
    //}

    private async Task CompleteIndoorSowTasks(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First(p => p.Id == harvestEvent.TriggerEntity!.EntityId);
        var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plantHarvest.Id, Reason = WorkLogReasonEnum.SowIndoors });
        if (tasks != null && tasks.Any())
        {
            foreach (var task in tasks)
            {
                await _taskCommandHandler.CompletePlantTask(new UpdatePlantTaskCommand()
                {
                    PlantTaskId = task.PlantTaskId,
                    CompletedDateTime = plantHarvest.SeedingDate.HasValue?plantHarvest.SeedingDate.Value:DateTime.Now,
                    Notes = task.Notes,
                    TargetDateEnd = task.TargetDateEnd,
                    TargetDateStart = task.TargetDateStart
                });
            };
        }
    }

    private async Task DeleteIndoorSowTask(HarvestEvent harvestEvent)
    {
        var plant = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plant.Id, Reason = WorkLogReasonEnum.SowIndoors });
        if (tasks != null && tasks.Any())
        {
            foreach (var task in tasks)
            {
                await _taskCommandHandler.DeletePlantTask(task.PlantTaskId);
            }
        }
    }

    private async Task CreateIndoorSowTask(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        if (plantHarvest.PlantingMethod != PlantingMethodEnum.SeedIndoors) return;

        var schedule = plantHarvest.PlantCalendar.FirstOrDefault(s => s.TaskType == WorkLogReasonEnum.SowIndoors);
        if (schedule != null)
        {
            var command = new CreatePlantTaskCommand()
            {
                CreatedDateTime = DateTime.UtcNow,
                HarvestCycleId = harvestEvent.HarvestId,
                IsSystemGenerated = true,
                PlantHarvestCycleId = plantHarvest.Id,
                PlantName = string.IsNullOrEmpty(plantHarvest.PlantVarietyName) ? plantHarvest.PlantName : $"{plantHarvest.PlantName} - {plantHarvest.PlantVarietyName}",
                PlantScheduleId = schedule.Id,
                TargetDateStart = schedule.StartDate,
                TargetDateEnd = schedule.EndDate,
                Type = WorkLogReasonEnum.SowIndoors,
                Title = "Sow Seeds Indoors",
                Notes = schedule.Notes,
                CompletedDateTime = plantHarvest.SeedingDate.HasValue ? plantHarvest.SeedingDate.Value : null, //in case if seeds were already planted when plant was addede to harvest
            };

            await _taskCommandHandler.CreatePlantTask(command);

        }

    }


}
