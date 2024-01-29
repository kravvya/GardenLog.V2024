using MongoDB.Driver.Linq;
using PlantHarvest.Domain.HarvestAggregate.Events;
using PlantHarvest.Infrastructure.ApiClients;

namespace PlantHarvest.Orchestrator.Tasks;


public class GerminateTaskGenerator : INotificationHandler<HarvestEvent>
{
    private readonly IPlantTaskCommandHandler _taskCommandHandler;
    private readonly IPlantTaskQueryHandler _taskQueryHandler;
    private readonly IPlantCatalogApiClient _plantCatalogApi;
    private readonly ILogger<GerminateTaskGenerator> _logger;

    public const string GERMINATE_TASK_TITLE = "Record germination date";

    public GerminateTaskGenerator(IPlantTaskCommandHandler taskCommandHandler, IPlantTaskQueryHandler taskQueryHandler, IPlantCatalogApiClient plantCatalogApi, ILogger<GerminateTaskGenerator> logger)
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
            case HarvestEventTriggerEnum.PlantHarvestCycleSeeded:
                await CreateGerminateTask(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleHarvested:
            case HarvestEventTriggerEnum.PlantHarvestCycleCompleted:
            case HarvestEventTriggerEnum.PlantHarvestCycleTransplanted:
            case HarvestEventTriggerEnum.PlantHarvestCycleDeleted:
                await DeleteGerminateTask(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleGerminated:
                await CompleteGerminateTask(harvestEvent);
                break;

        }
    }

    private async Task CompleteGerminateTask(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);

        if (plantHarvest != null && plantHarvest.GerminationDate.HasValue)
        {
            var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plantHarvest.Id, Reason = WorkLogReasonEnum.Information, IncludeResolvedTasks=false });
            if (tasks != null && tasks.Any())
            {
                foreach (var task in tasks.Where(t => t.Title == GERMINATE_TASK_TITLE).ToList())
                {
                    await _taskCommandHandler.CompletePlantTask(new UpdatePlantTaskCommand()
                    {
                        PlantTaskId = task.PlantTaskId,
                        CompletedDateTime = plantHarvest.GerminationDate,
                        Notes = task.Notes,
                        TargetDateEnd = task.TargetDateEnd,
                        TargetDateStart = task.TargetDateStart
                    });
                };
            }
        }
        else
        {
            _logger.LogError("Unable to complete task for recording when seeds were germinated for : {harvestEvent.TriggerEntity.EntityId}. Plant is not found", harvestEvent.TriggerEntity!.EntityId);
        }
    }

    private async Task CreateGerminateTask(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        if (plantHarvest.PlantingMethod == PlantingMethodEnum.Transplanting || !plantHarvest.SeedingDate.HasValue || plantHarvest.GerminationDate.HasValue)
        {
            return;
        }

        if(plantHarvest.PlantGrowthInstructionId == null)
        {
            return;
        }

        var growInstruction = await _plantCatalogApi.GetPlantGrowInstruction(plantHarvest.PlantId, plantHarvest.PlantGrowthInstructionId);

        if (growInstruction == null)
        {
            return;
        }

       if (growInstruction != null && growInstruction.DaysToSproutMin.HasValue && growInstruction.DaysToSproutMax.HasValue)
        {
            var expectedGerminationDateMin = plantHarvest.SeedingDate.Value.AddDays(growInstruction.DaysToSproutMin.Value);
            var expectedGerminationDateMax = plantHarvest.SeedingDate.Value.AddDays(growInstruction.DaysToSproutMax.Value  );

            var command = new CreatePlantTaskCommand()
            {
                CreatedDateTime = DateTime.UtcNow,
                HarvestCycleId = harvestEvent.HarvestId,
                IsSystemGenerated = true,
                PlantHarvestCycleId = plantHarvest.Id,
                PlantName = string.IsNullOrEmpty(plantHarvest.PlantVarietyName) ? plantHarvest.PlantName : $"{plantHarvest.PlantName} - {plantHarvest.PlantVarietyName}",
                PlantScheduleId = string.Empty,
                TargetDateStart = expectedGerminationDateMin,
                TargetDateEnd = expectedGerminationDateMax,
                Type = WorkLogReasonEnum.Information,
                Title = GERMINATE_TASK_TITLE,
                Notes = "Record germination date and percent of seeds that were germinated "
            };

            await _taskCommandHandler.CreatePlantTask(command);

        }

    }
    private async Task DeleteGerminateTask(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First(plant => plant.Id == harvestEvent.TriggerEntity!.EntityId);
        var tasks = await _taskQueryHandler.SearchPlantTasks(new Contract.Query.PlantTaskSearch() { PlantHarvestCycleId = plantHarvest.Id, Reason = WorkLogReasonEnum.Information, IncludeResolvedTasks=false });
        if (tasks != null && tasks.Any())
        {
            foreach (var task in tasks)
            {
                if (task.Title.Equals(GERMINATE_TASK_TITLE))
                {
                    await _taskCommandHandler.DeletePlantTask(task.PlantTaskId);
                }
            }
        }
    }

}
