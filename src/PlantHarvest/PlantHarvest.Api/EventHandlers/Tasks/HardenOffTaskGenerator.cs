using GardenLog.SharedKernel.Enum;
using PlantHarvest.Domain.HarvestAggregate.Events;
using PlantHarvest.Domain.WorkLogAggregate.Events;

namespace PlantHarvest.Api.EventHandlers.Tasks;

public class HardenOffTaskGenerator : INotificationHandler<HarvestEvent> , INotificationHandler<WorkLogEvent>
{
    private readonly IPlantTaskCommandHandler _taskCommandHandler;
    private readonly IPlantTaskQueryHandler _taskQueryHandler;
    private readonly IHarvestQueryHandler _harvestQueryHandler;
    private readonly ILogger<HardenOffTaskGenerator> _logger;

    public HardenOffTaskGenerator(IPlantTaskCommandHandler taskCommandHandler, IPlantTaskQueryHandler taskQueryHandler, IHarvestQueryHandler harvestQueryHandler, ILogger<HardenOffTaskGenerator> logger)
    {
        _taskCommandHandler = taskCommandHandler;
        _taskQueryHandler = taskQueryHandler;
        _harvestQueryHandler = harvestQueryHandler;
        _logger = logger;
    }

    public async Task Handle(HarvestEvent harvestEvent, CancellationToken cancellationToken)
    {
        switch (harvestEvent.Trigger)
        {
            case HarvestEventTriggerEnum.PlantAddedToHarvestCycle:
                var plant = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
                if (plant != null && plant.PlantingMethod == PlantingMethodEnum.Transplanting && !plant.TransplantDate.HasValue)
                {
                    await CreateHardenOffTask(harvestEvent);
                }
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleSeeded:
                await CreateHardenOffTask(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleTransplanted:
                await CompleteHardenOffTask(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleDeleted:
                await DeleteHardenOffTask(harvestEvent);
                break;

        }
    }

    public async Task Handle(WorkLogEvent workEvent, CancellationToken cancellationToken)
    {
        if (workEvent.Work!.Reason == WorkLogReasonEnum.Harden)
        {
            await CompleteHardenOffTask(workEvent);
            await CreateHardenOffTask(workEvent);
        }
    }

    private async Task CompleteHardenOffTask(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First(p => p.Id == harvestEvent.TriggerEntity!.EntityId);
        var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plantHarvest.Id, Reason = WorkLogReasonEnum.Harden });
        if (tasks != null && tasks.Any())
        {
            foreach (var task in tasks)
            {
                await _taskCommandHandler.CompletePlantTask(new UpdatePlantTaskCommand()
                {
                    PlantTaskId = task.PlantTaskId,
                    CompletedDateTime = plantHarvest.TransplantDate.HasValue ? plantHarvest.TransplantDate.Value : DateTime.Now,
                    Notes = task.Notes,
                    TargetDateEnd = task.TargetDateEnd,
                    TargetDateStart = task.TargetDateStart
                });
            };
        }
    }

    private async Task CompleteHardenOffTask(WorkLogEvent workEvent)
    {
        if (workEvent.Work!.RelatedEntities == null)
        {
            _logger.LogError("Unable to complete task based on workLog: {workEvent.Work.Id}. No related entities present.", workEvent.Work.Id);
            return;
        }
        var plantHarvestRelatedEntity = workEvent.Work!.RelatedEntities.FirstOrDefault(e => e.EntityType == RelatedEntityTypEnum.PlantHarvestCycle);

        if (plantHarvestRelatedEntity != null)
        {
            var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plantHarvestRelatedEntity.EntityId, Reason = WorkLogReasonEnum.Harden });
            if (tasks != null && tasks.Any())
            {
                foreach (var task in tasks)
                {
                    await _taskCommandHandler.CompletePlantTask(new UpdatePlantTaskCommand()
                    {
                        PlantTaskId = task.PlantTaskId,
                        CompletedDateTime = workEvent.Work.EventDateTime,
                        Notes = task.Notes,
                        TargetDateEnd = task.TargetDateEnd,
                        TargetDateStart = task.TargetDateStart
                    });
                };
            }
        }
        else
        {
            _logger.LogError("Unable to complete task based on workLog: {workEvent.Work.Id}. Plant Harvest is not found", workEvent.Work.Id);
        }
    }

    private async Task DeleteHardenOffTask(HarvestEvent harvestEvent)
    {
        var plant = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plant.Id, Reason = WorkLogReasonEnum.Harden });
        if (tasks != null && tasks.Any())
        {
            foreach (var task in tasks)
            {
                await _taskCommandHandler.DeletePlantTask(task.PlantTaskId);
            }
        }
    }

    private async Task CreateHardenOffTask(WorkLogEvent workLogEvent)
    {
        if (workLogEvent.Work!.RelatedEntities == null)
        {
            _logger.LogError("Unable to complete task based on workLog: {workEvent.Work.Id}. No related entities present.", workLogEvent.Work.Id);
            return;
        }
        var harvestRelatedEntity = workLogEvent.Work!.RelatedEntities.FirstOrDefault(e => e.EntityType == RelatedEntityTypEnum.HarvestCycle);
        if (harvestRelatedEntity == null) { return; }

        var plantHarvestRelatedEntity = workLogEvent.Work.RelatedEntities.FirstOrDefault(e => e.EntityType == RelatedEntityTypEnum.PlantHarvestCycle);
        if (plantHarvestRelatedEntity == null) { return; }

        var plantHarvest = await _harvestQueryHandler.GetPlantHarvestCycle(harvestRelatedEntity.EntityId, plantHarvestRelatedEntity.EntityId);

        if (plantHarvest.PlantGrowthInstructionId == null || plantHarvest.PlantingMethod != PlantingMethodEnum.SeedIndoors || !plantHarvest.GerminationDate.HasValue || plantHarvest.TransplantDate.HasValue)
        {
            return;
        }              

        //we started to HardenOff. So use worklog eventdate as a base. 
        var hardenOffDate = workLogEvent.Work.EventDateTime.AddDays(1);

        //make sure that fertilization date is before projected transplant date
        var transplantSchedule = plantHarvest.PlantCalendar.FirstOrDefault(s => s.TaskType == WorkLogReasonEnum.TransplantOutside);
        if (transplantSchedule != null && transplantSchedule.StartDate <= hardenOffDate)
        {
            return;
        }

        var command = new CreatePlantTaskCommand()
        {
            CreatedDateTime = DateTime.UtcNow,
            HarvestCycleId = plantHarvest.HarvestCycleId,
            IsSystemGenerated = true,
            PlantHarvestCycleId = plantHarvest.PlantHarvestCycleId,
            PlantName = string.IsNullOrEmpty(plantHarvest.PlantVarietyName) ? plantHarvest.PlantName : $"{plantHarvest.PlantName} - {plantHarvest.PlantVarietyName}",
            PlantScheduleId = string.Empty,
            TargetDateStart = hardenOffDate,
            TargetDateEnd = hardenOffDate.AddDays(1),
            Type = WorkLogReasonEnum.Harden,
            Title = "Harden Off Seedlings",
            Notes = "Bringing the seedlings outdoors for several periods of time. Start with just a couple of hours and gradually increase every day"
        };

        await _taskCommandHandler.CreatePlantTask(command);
    }

    private async Task CreateHardenOffTask(HarvestEvent harvestEvent)
    {
        var plant = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        var schedule = plant.PlantCalendar.FirstOrDefault(s => s.TaskType == WorkLogReasonEnum.TransplantOutside);

        if (schedule != null)
        {            
            //only create Harden Off task if plats were grown from seeds inside
            if (plant.PlantingMethod == PlantingMethodEnum.SeedIndoors)
            {

                var command = new CreatePlantTaskCommand()
                {
                    CreatedDateTime = DateTime.UtcNow,
                    HarvestCycleId = harvestEvent.HarvestId,
                    IsSystemGenerated = true,
                    PlantHarvestCycleId = plant.Id,
                    PlantName = string.IsNullOrEmpty(plant.PlantVarietyName) ? plant.PlantName : $"{plant.PlantName} - {plant.PlantVarietyName}",
                    PlantScheduleId = schedule.Id,
                    TargetDateStart = schedule.StartDate.AddDays(-1 * GlobalConstants.DEFAULT_HardenOffPeriodInDays),
                    TargetDateEnd = schedule.StartDate,
                    Type = WorkLogReasonEnum.Harden,
                    Title = "Harden Off",
                    Notes = "Bringing the seedlings outdoors for several periods of time. Start with just a couple of hours and gradually increase every day"
                };

                await _taskCommandHandler.CreatePlantTask(command);
            }
        }

    }
}
