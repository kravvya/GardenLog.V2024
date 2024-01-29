using GardenLog.SharedKernel.Enum;
using MongoDB.Driver.Linq;
using PlantCatalog.Contract.Enum;
using PlantHarvest.Domain.HarvestAggregate.Events;
using PlantHarvest.Domain.WorkLogAggregate.Events;
using PlantHarvest.Infrastructure.ApiClients;
using System.Text;

namespace PlantHarvest.Orchestrator.Tasks;


public class OutdoorFertilizeTaskGenerator : INotificationHandler<HarvestEvent>, INotificationHandler<WorkLogEvent>
{
    private readonly IPlantTaskCommandHandler _taskCommandHandler;
    private readonly IPlantTaskQueryHandler _taskQueryHandler;
    private readonly IPlantCatalogApiClient _plantCatalogApi;
    private readonly IHarvestQueryHandler _harvestQueryHandler;
    private readonly ILogger<OutdoorFertilizeTaskGenerator> _logger;

    public OutdoorFertilizeTaskGenerator(IPlantTaskCommandHandler taskCommandHandler, IPlantTaskQueryHandler taskQueryHandler, IPlantCatalogApiClient plantCatalogApi, IHarvestQueryHandler harvestQueryHandler, ILogger<OutdoorFertilizeTaskGenerator> logger)
    {
        _taskCommandHandler = taskCommandHandler;
        _taskQueryHandler = taskQueryHandler;
        _plantCatalogApi = plantCatalogApi;
        _harvestQueryHandler = harvestQueryHandler;
        _logger = logger;
    }

    public async Task Handle(HarvestEvent harvestEvent, CancellationToken cancellationToken)
    {
        switch (harvestEvent.Trigger)
        {
            case HarvestEventTriggerEnum.PlantHarvestCycleTransplanted:
                await CreateFertilizeOutdoorTask(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleDeleted:
                await DeleteFertilizeOutdoorTask(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleHarvested:
                await DeleteFertilizeOutdoorTask(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleCompleted:
                await DeleteFertilizeOutdoorTask(harvestEvent);
                break;

        }
    }

    public async Task Handle(WorkLogEvent workEvent, CancellationToken cancellationToken)
    {
        if (workEvent.Work!.Reason == WorkLogReasonEnum.FertilizeOutside)
        {
            await CompleteFertilizeOutdoorTask(workEvent);
            await CreateFertilizeOutdoorTask(workEvent);
        }
    }

    private async Task CompleteFertilizeOutdoorTask(WorkLogEvent workEvent)
    {
        if(workEvent.Work!.RelatedEntities==null)
        {
            _logger.LogError("Unable to complete task based on workLog: {workEvent.Work.Id}. Not Related Entities", workEvent.Work.Id);
            return;
        }
        var plantHarvestRelatedEntity = workEvent.Work!.RelatedEntities.FirstOrDefault(e => e.EntityType == RelatedEntityTypEnum.PlantHarvestCycle);

        if (plantHarvestRelatedEntity != null)
        {
            var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plantHarvestRelatedEntity.EntityId, Reason = WorkLogReasonEnum.FertilizeOutside });
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

    private async Task CreateFertilizeOutdoorTask(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        if (plantHarvest.PlantGrowthInstructionId == null || !plantHarvest.TransplantDate.HasValue)
        {
            return;
        }

        var growInstruction = await _plantCatalogApi.GetPlantGrowInstruction(plantHarvest.PlantId, plantHarvest.PlantGrowthInstructionId);

        if (growInstruction == null || growInstruction.Fertilizer == FertilizerEnum.Unspecified)
        {
            return;
        }

        //assume this is first time we are going to fertilize. So use tranplant date date as a base. All subsequent fertilizations will be based on the last fertilie event from WorkLog
        if (growInstruction != null)
        {
            var firstFertilizeDate = growInstruction.FertilizeFrequencyInWeeks.HasValue ?
                                plantHarvest.TransplantDate.Value.AddDays(7 * growInstruction.FertilizeFrequencyInWeeks.Value) :
                                plantHarvest.TransplantDate.Value.AddDays(7 * GlobalConstants.DEFAULT_FertilizeFrequencyInWeeks);


            var command = new CreatePlantTaskCommand()
            {
                CreatedDateTime = DateTime.UtcNow,
                HarvestCycleId = harvestEvent.HarvestId,
                IsSystemGenerated = true,
                PlantHarvestCycleId = plantHarvest.Id,
                PlantName = string.IsNullOrEmpty(plantHarvest.PlantVarietyName) ? plantHarvest.PlantName : $"{plantHarvest.PlantName} - {plantHarvest.PlantVarietyName}",
                PlantScheduleId = string.Empty,
                TargetDateStart = firstFertilizeDate,
                TargetDateEnd = firstFertilizeDate.AddDays(1),
                Type = WorkLogReasonEnum.FertilizeOutside,
                Title = "Fertilize",
                Notes = GetFertilizeOutsideNotes(growInstruction)
            };

            await _taskCommandHandler.CreatePlantTask(command);

        }

    }

    private async Task CreateFertilizeOutdoorTask(WorkLogEvent workLogEvent)
    {
        if (workLogEvent.Work!.RelatedEntities == null)
        {
            _logger.LogError("Unable to complete task based on workLog: {workLogEvent.Work.Id}. Not Related Entities", workLogEvent.Work.Id);
            return;
        }
        var harvestRelatedEntity = workLogEvent.Work.RelatedEntities.FirstOrDefault(e => e.EntityType == RelatedEntityTypEnum.HarvestCycle);
        if (harvestRelatedEntity == null) { return;  }

        var plantHarvestRelatedEntity = workLogEvent.Work.RelatedEntities.FirstOrDefault(e => e.EntityType == RelatedEntityTypEnum.PlantHarvestCycle);
        if (plantHarvestRelatedEntity == null) { return; }

        var plantHarvest = await _harvestQueryHandler.GetPlantHarvestCycle(harvestRelatedEntity.EntityId, plantHarvestRelatedEntity.EntityId);

        if (plantHarvest.PlantGrowthInstructionId == null || !plantHarvest.TransplantDate.HasValue)
        {
            return;
        }

        var growInstruction = await _plantCatalogApi.GetPlantGrowInstruction(plantHarvest.PlantId, plantHarvest.PlantGrowthInstructionId);

        if (growInstruction == null || growInstruction.Fertilizer == FertilizerEnum.Unspecified)
        {
            return;
        }

        //we just fertilized. So use worklog eventdate as a base. 
        var fertilizeDate = growInstruction.FertilizeFrequencyInWeeks.HasValue ?
                            workLogEvent.Work.EventDateTime.AddDays(7 * growInstruction.FertilizeFrequencyInWeeks.Value) :
                            workLogEvent.Work.EventDateTime.AddDays(7 * GlobalConstants.DEFAULT_FertilizeFrequencyInWeeks);

        //make sure that fertilization date is before projected harvest date
        var harvestSchedule = plantHarvest.PlantCalendar.FirstOrDefault(s => s.TaskType == WorkLogReasonEnum.Harvest);
        if(harvestSchedule!= null && harvestSchedule.StartDate <= fertilizeDate)
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
            TargetDateStart = fertilizeDate,
            TargetDateEnd = fertilizeDate.AddDays(1),
            Type = WorkLogReasonEnum.FertilizeOutside,
            Title = "Fertilize",
            Notes = GetFertilizeOutsideNotes(growInstruction)
        };

        await _taskCommandHandler.CreatePlantTask(command);
    }

    private static string GetFertilizeOutsideNotes(PlantGrowInstructionViewModel growInstruction)
    {
        StringBuilder notes = new();
        notes.Append($"Fertilize with {growInstruction.Fertilizer.GetDescription()} ");
        if (growInstruction.FertilizeFrequencyInWeeks.HasValue)
            notes.Append($" every {growInstruction.FertilizeFrequencyInWeeks.Value} weeks.");
        else
            notes.Append($" every {GlobalConstants.DEFAULT_FertilizeFrequencyInWeeks} weeks.");
        return notes.ToString();
    }

    private async Task DeleteFertilizeOutdoorTask(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plantHarvest.Id, Reason = WorkLogReasonEnum.FertilizeOutside });
        if (tasks != null && tasks.Any())
        {
            foreach (var task in tasks)
            {
                await _taskCommandHandler.DeletePlantTask(task.PlantTaskId);
            }
        }
    }

}
