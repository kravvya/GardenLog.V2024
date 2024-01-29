using GardenLog.SharedKernel.Enum;
using MongoDB.Driver.Linq;
using PlantHarvest.Domain.HarvestAggregate.Events;
using PlantHarvest.Domain.WorkLogAggregate.Events;

namespace PlantHarvest.Orchestrator.Tasks;


public class ManualScheduleTaskGenerator : INotificationHandler<HarvestEvent>, INotificationHandler<WorkLogEvent>
{
    private readonly IPlantTaskCommandHandler _taskCommandHandler;
    private readonly IPlantTaskQueryHandler _taskQueryHandler;
    private readonly ILogger<IndoorFertilizeTaskGenerator> _logger;

    public ManualScheduleTaskGenerator(IPlantTaskCommandHandler taskCommandHandler, IPlantTaskQueryHandler taskQueryHandler, ILogger<IndoorFertilizeTaskGenerator> logger)
    {
        _taskCommandHandler = taskCommandHandler;
        _taskQueryHandler = taskQueryHandler;
        _logger = logger;
    }

    public async Task Handle(HarvestEvent harvestEvent, CancellationToken cancellationToken)
    {
        switch (harvestEvent.Trigger)
        {
            case HarvestEventTriggerEnum.PlantHarvestCycleCompleted:
                await DeleteManualScheduleTask(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleDeleted:
                await DeleteManualScheduleTask(harvestEvent);
                break;

        }
    }

    public async Task Handle(WorkLogEvent workEvent, CancellationToken cancellationToken)
    {
        if (workEvent.Work!.Reason == WorkLogReasonEnum.IssueResolution ||
            workEvent.Work!.Reason == WorkLogReasonEnum.Maintenance )
        {
            await CompleteManualScheduledTask(workEvent);
        }
    }

    private async Task CompleteManualScheduledTask(WorkLogEvent workEvent)
    {
        if(workEvent.Work!.RelatedEntities == null)
        {
            _logger.LogError("Unable to complete task based on workLog: {workEvent.Work.Id}. No related entities present.", workEvent.Work.Id);
            return;
        }
        var plantHarvestRelatedEntity = workEvent.Work!.RelatedEntities.FirstOrDefault(e => e.EntityType == RelatedEntityTypEnum.PlantHarvestCycle);

        if (plantHarvestRelatedEntity != null)
        {
            var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plantHarvestRelatedEntity.EntityId, Reason = workEvent.Work.Reason });
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

    private async Task DeleteManualScheduleTask(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plantHarvest.Id});
        if (tasks != null && tasks.Any())
        {
            foreach (var task in tasks)
            {
                if(task.Type == WorkLogReasonEnum.IssueResolution || task.Type == WorkLogReasonEnum.Maintenance)
                await _taskCommandHandler.DeletePlantTask(task.PlantTaskId);
            }
        }
    }

}
