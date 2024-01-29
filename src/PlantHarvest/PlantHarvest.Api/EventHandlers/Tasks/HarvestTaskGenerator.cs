using MongoDB.Driver.Linq;
using PlantHarvest.Domain.HarvestAggregate.Events;
using PlantHarvest.Infrastructure.ApiClients;

namespace PlantHarvest.Orchestrator.Tasks;


public class HarvestTaskGenerator : INotificationHandler<HarvestEvent>
{
    private readonly IPlantTaskCommandHandler _taskCommandHandler;
    private readonly IPlantTaskQueryHandler _taskQueryHandler;
    private readonly IPlantCatalogApiClient _plantCatalogApi;
    private readonly ILogger<HarvestTaskGenerator> _logger;

    public HarvestTaskGenerator(IPlantTaskCommandHandler taskCommandHandler, IPlantTaskQueryHandler taskQueryHandler, IPlantCatalogApiClient plantCatalogApi, ILogger<HarvestTaskGenerator> logger)
    {
        _taskCommandHandler = taskCommandHandler;
        _taskQueryHandler = taskQueryHandler;
        _plantCatalogApi = plantCatalogApi;
        _logger = logger;
    }

    public async Task Handle(HarvestEvent harvestEvent, CancellationToken cancellationToken)
    {
        switch (harvestEvent.Trigger)
        {
            case HarvestEventTriggerEnum.PlantHarvestCycleTransplanted:
                await CreateHarvestTaskForTransplanted(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleGerminated:
                await CreateHarvestTaskForDirectSeed(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleDeleted:
                await DeleteHarvestTask(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleHarvested:
                await CompleteHarvestTask(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleCompleted:
                await DeleteHarvestTask(harvestEvent);
                break;

        }
    }


    private async Task CompleteHarvestTask(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.FirstOrDefault(e => e.Id == harvestEvent.TriggerEntity!.EntityId);

        if (plantHarvest != null)
        {
            var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plantHarvest.Id, Reason = WorkLogReasonEnum.Harvest });
            if (tasks != null && tasks.Any())
            {
                foreach (var task in tasks)
                {
                    await _taskCommandHandler.CompletePlantTask(new UpdatePlantTaskCommand()
                    {
                        PlantTaskId = task.PlantTaskId,
                        CompletedDateTime = plantHarvest.FirstHarvestDate,
                        Notes = task.Notes,
                        TargetDateEnd = task.TargetDateEnd,
                        TargetDateStart = task.TargetDateStart
                    });
                };
            }
        }
        else
        {
            _logger.LogError("Unable to complete task based on plant harvest: {harvestEvent.TriggerEntity!.EntityId}. Plant Harvest is not found", harvestEvent.TriggerEntity!.EntityId);
        }
    }

    private async Task CreateHarvestTaskForTransplanted(HarvestEvent harvestEvent)
    {
        int daysToMaturityMin;
        int daysToMaturityMax;

        var plantHarvest = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        if (!plantHarvest.TransplantDate.HasValue)
        {
            return;
        }

        if (!string.IsNullOrEmpty(plantHarvest.PlantVarietyId))
        {
            var variety = await _plantCatalogApi.GetPlantVariety(plantHarvest.PlantId, plantHarvest.PlantVarietyId);
            if (variety == null || !variety.DaysToMaturityMin.HasValue || !variety.DaysToMaturityMax.HasValue) { return; }
            daysToMaturityMin = variety.DaysToMaturityMin.Value;
            daysToMaturityMax = variety.DaysToMaturityMax.Value;
        }
        else
        {
            var plant = await _plantCatalogApi.GetPlant(plantHarvest.PlantId);
            if (plant == null || !plant.DaysToMaturityMin.HasValue || !plant.DaysToMaturityMax.HasValue) { return; }
            daysToMaturityMin = plant.DaysToMaturityMin.Value;
            daysToMaturityMax = plant.DaysToMaturityMax.Value;
        }

        //if we do not know when plant is going to mature - avoid crating Harvest task.
        if(daysToMaturityMin <= 0 &&  daysToMaturityMax <= 0) {  return; }

        var firstHarvestDate = plantHarvest.TransplantDate.Value.AddDays(daysToMaturityMin);
        var schedule = plantHarvest.PlantCalendar.FirstOrDefault(s => s.TaskType == WorkLogReasonEnum.Harvest);

        var command = new CreatePlantTaskCommand()
        {
            CreatedDateTime = DateTime.UtcNow,
            HarvestCycleId = harvestEvent.HarvestId,
            IsSystemGenerated = true,
            PlantHarvestCycleId = plantHarvest.Id,
            PlantName = string.IsNullOrEmpty(plantHarvest.PlantVarietyName) ? plantHarvest.PlantName : $"{plantHarvest.PlantName} - {plantHarvest.PlantVarietyName}",
            PlantScheduleId = schedule != null ? schedule.Id : string.Empty,
            TargetDateStart = firstHarvestDate,
            TargetDateEnd = firstHarvestDate.AddDays(daysToMaturityMax),
            Type = WorkLogReasonEnum.Harvest,
            Title = "Harvest",
            Notes = schedule != null ? schedule.Notes : string.Empty
        };

        await _taskCommandHandler.CreatePlantTask(command);

    }

    private async Task CreateHarvestTaskForDirectSeed(HarvestEvent harvestEvent)
    {
        int daysToMaturityMin;
        int daysToMaturityMax;

        var plantHarvest = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        if (!plantHarvest.GerminationDate.HasValue || plantHarvest.PlantingMethod != PlantingMethodEnum.DirectSeed)
        {
            return;
        }

        if (!string.IsNullOrEmpty(plantHarvest.PlantVarietyId))
        {
            var variety = await _plantCatalogApi.GetPlantVariety(plantHarvest.PlantId, plantHarvest.PlantVarietyId);
            if (variety == null || !variety.DaysToMaturityMin.HasValue || !variety.DaysToMaturityMax.HasValue) { return; }
            daysToMaturityMin = variety.DaysToMaturityMin.Value;
            daysToMaturityMax = variety.DaysToMaturityMax.Value;
        }
        else
        {
            var plant = await _plantCatalogApi.GetPlant(plantHarvest.PlantId);
            if (plant == null || !plant.DaysToMaturityMin.HasValue || !plant.DaysToMaturityMax.HasValue) { return; }
            daysToMaturityMin = plant.DaysToMaturityMin.Value;
            daysToMaturityMax = plant.DaysToMaturityMax.Value;
        }

        var firstHarvestDate = plantHarvest.GerminationDate.Value.AddDays(daysToMaturityMin);
        var schedule = plantHarvest.PlantCalendar.FirstOrDefault(s => s.TaskType == WorkLogReasonEnum.Harvest);

        var command = new CreatePlantTaskCommand()
        {
            CreatedDateTime = DateTime.UtcNow,
            HarvestCycleId = harvestEvent.HarvestId,
            IsSystemGenerated = true,
            PlantHarvestCycleId = plantHarvest.Id,
            PlantName = string.IsNullOrEmpty(plantHarvest.PlantVarietyName) ? plantHarvest.PlantName : $"{plantHarvest.PlantName} - {plantHarvest.PlantVarietyName}",
            PlantScheduleId = schedule != null? schedule.Id:string.Empty,
            TargetDateStart = firstHarvestDate,
            TargetDateEnd = firstHarvestDate.AddDays(daysToMaturityMax),
            Type = WorkLogReasonEnum.Harvest,
            Title = "Harvest",
            Notes = schedule != null ? schedule.Notes : string.Empty,
        };

        await _taskCommandHandler.CreatePlantTask(command);

    }

    private async Task DeleteHarvestTask(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plantHarvest.Id, Reason = WorkLogReasonEnum.Harvest });
        if (tasks != null && tasks.Any())
        {
            foreach (var task in tasks)
            {
                await _taskCommandHandler.DeletePlantTask(task.PlantTaskId);
            }
        }
    }

}
