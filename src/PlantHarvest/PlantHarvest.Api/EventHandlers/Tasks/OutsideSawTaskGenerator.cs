using PlantHarvest.Domain.HarvestAggregate.Events;
using PlantHarvest.Domain.WorkLogAggregate.Events;

namespace PlantHarvest.Orchestrator.Tasks;


public class OutsideSawTaskGenerator : INotificationHandler<HarvestEvent>//, INotificationHandler<WorkLogEvent>
{
    private readonly IPlantTaskCommandHandler _taskCommandHandler;
    private readonly IPlantTaskQueryHandler _taskQueryHandler;

    public OutsideSawTaskGenerator(IPlantTaskCommandHandler taskCommandHandler, IPlantTaskQueryHandler taskQueryHandler)
    {
        _taskCommandHandler = taskCommandHandler;
        _taskQueryHandler = taskQueryHandler;
    }

    public async Task Handle(HarvestEvent harvestEvent, CancellationToken cancellationToken)
    {
        switch (harvestEvent.Trigger)
        {
            case HarvestEventTriggerEnum.PlantAddedToHarvestCycle:
                await CreateOutsideSowTask(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleSeeded:
                await CompleteOutsideSowTasks(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleDeleted:
                await DeleteOutsideSowTask(harvestEvent);
                break;

        }
    }

    private async Task CompleteOutsideSowTasks(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First(p => p.Id == harvestEvent.TriggerEntity!.EntityId);
        if (plantHarvest.PlantingMethod != PlantingMethodEnum.DirectSeed) return;

        var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plantHarvest.Id, Reason = WorkLogReasonEnum.SowOutside });
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

    private async Task DeleteOutsideSowTask(HarvestEvent harvestEvent)
    {
        var plant = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plant.Id, Reason = WorkLogReasonEnum.SowOutside });
        if (tasks != null && tasks.Any())
        {
            foreach (var task in tasks)
            {
                await _taskCommandHandler.DeletePlantTask(task.PlantTaskId);
            }
        }
    }

    private async Task CreateOutsideSowTask(HarvestEvent harvestEvent)
    {
        var plant = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        var schedule = plant.PlantCalendar.FirstOrDefault(s => s.TaskType == WorkLogReasonEnum.SowOutside);

        if (schedule != null)
        {
            var command = new CreatePlantTaskCommand()
            {
                CreatedDateTime = DateTime.UtcNow,
                HarvestCycleId = harvestEvent.HarvestId,
                IsSystemGenerated = true,
                PlantHarvestCycleId = plant.Id,
                PlantName = string.IsNullOrEmpty(plant.PlantVarietyName)? plant.PlantName : $"{plant.PlantName} - {plant.PlantVarietyName}",
                PlantScheduleId = schedule.Id,
                TargetDateStart = schedule.StartDate,
                TargetDateEnd = schedule.EndDate,
                Type = WorkLogReasonEnum.SowOutside,
                Title = "Sow Outside",
                Notes = schedule.Notes,
            };

            await _taskCommandHandler.CreatePlantTask(command);

        }

    }


}
