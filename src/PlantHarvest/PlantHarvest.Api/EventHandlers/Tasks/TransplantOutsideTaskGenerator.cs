using PlantHarvest.Domain.HarvestAggregate.Events;

namespace PlantHarvest.Orchestrator.Tasks;


public class TransplantOutsideTaskGenerator : INotificationHandler<HarvestEvent>
{
    private readonly IPlantTaskCommandHandler _taskCommandHandler;
    private readonly IPlantTaskQueryHandler _taskQueryHandler;

    public TransplantOutsideTaskGenerator(IPlantTaskCommandHandler taskCommandHandler, IPlantTaskQueryHandler taskQueryHandler)
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
                var plant = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
                if (plant != null && plant.PlantingMethod == PlantingMethodEnum.Transplanting && !plant.TransplantDate.HasValue)
                {
                    await CreateTransplantOutsideTask(harvestEvent);
                }
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleSeeded:
                await CreateTransplantOutsideTask(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleTransplanted:
                await CompleteTransplantOutsideTasks(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleDeleted:
                await DeleteTransplantOutsideTask(harvestEvent);
                break;

        }
    }
   
    private async Task CompleteTransplantOutsideTasks(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First(p => p.Id == harvestEvent.TriggerEntity!.EntityId);
        var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plantHarvest.Id, Reason = WorkLogReasonEnum.TransplantOutside });
        if (tasks != null && tasks.Any())
        {
            foreach (var task in tasks)
            {
                await _taskCommandHandler.CompletePlantTask(new UpdatePlantTaskCommand()
                {
                    PlantTaskId = task.PlantTaskId,
                    CompletedDateTime = plantHarvest.TransplantDate.HasValue? plantHarvest.TransplantDate.Value: DateTime.Now,
                    Notes = task.Notes,
                    TargetDateEnd = task.TargetDateEnd,
                    TargetDateStart = task.TargetDateStart
                });
            };
        }
    }

    private async Task DeleteTransplantOutsideTask(HarvestEvent harvestEvent)
    {
        var plant = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plant.Id, Reason = WorkLogReasonEnum.TransplantOutside });
        if (tasks != null && tasks.Any())
        {
            foreach (var task in tasks)
            {
                await _taskCommandHandler.DeletePlantTask(task.PlantTaskId);
            }
        }
    }

    private async Task CreateTransplantOutsideTask(HarvestEvent harvestEvent)
    {
        var plant = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        var schedule = plant.PlantCalendar.FirstOrDefault(s => s.TaskType == WorkLogReasonEnum.TransplantOutside);

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
                Type = WorkLogReasonEnum.TransplantOutside,
                Title = "Transplant Outside",
                Notes = schedule.Notes,
            };

            await _taskCommandHandler.CreatePlantTask(command);
        }

    }


}
