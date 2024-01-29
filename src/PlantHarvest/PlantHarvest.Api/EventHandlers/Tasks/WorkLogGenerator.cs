using GardenLog.SharedKernel;
using GardenLog.SharedKernel.Enum;
using PlantHarvest.Domain.HarvestAggregate.Events;
using System.Text;

namespace PlantHarvest.Api.EventHandlers.Tasks;

public class WorkLogGenerator : INotificationHandler<HarvestEvent>
{
    private readonly IWorkLogCommandHandler _workLogCommandHandler;

    public WorkLogGenerator(IWorkLogCommandHandler workLogCommandHandler)
    {
        _workLogCommandHandler = workLogCommandHandler;
    }

    public async Task Handle(HarvestEvent harvestEvent, CancellationToken cancellationToken)
    {
        switch (harvestEvent.Trigger)
        {
            case HarvestEventTriggerEnum.PlantHarvestCycleSeeded:
                await GenerateSowIndoorsWorkLog(harvestEvent);
                await GenerateSowOutsideWorkLog(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleGerminated:
                await GenerateInformationWorkLogForGenrmatedEvent(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleTransplanted:
                await GenerateTransplantOutsideWorkLog(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleHarvested:
                await GenerateHarvestWorkLog(harvestEvent);
                break;
            case HarvestEventTriggerEnum.PlantHarvestCycleCompleted:
                await GenerateLastHarvestWorkLog(harvestEvent);
                break;
        }

    }

    public async Task GenerateSowIndoorsWorkLog(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First((Func<PlantHarvestCycle, bool>)(p => p.Id == harvestEvent.TriggerEntity!.EntityId));
        if (plantHarvest.PlantingMethod != PlantingMethodEnum.SeedIndoors) { return; }

        StringBuilder note = new();
        if (plantHarvest.NumberOfSeeds.HasValue) { note.Append($"{plantHarvest.NumberOfSeeds} seeds of "); }
        note.Append(plantHarvest.PlantName);
        if (!string.IsNullOrWhiteSpace(plantHarvest.PlantVarietyName)) { note.Append($"-{plantHarvest.PlantVarietyName} "); }
        if (!string.IsNullOrWhiteSpace(plantHarvest.SeedCompanyName)) { note.Append($"from {plantHarvest.SeedCompanyName} "); }
        if (plantHarvest.SeedingDate.HasValue)
        {
            note.Append($" were seeded indoors on {plantHarvest.SeedingDate.Value.ToShortDateString()} ");
        }

        var relatedEntities = new List<RelatedEntity>();
        relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.HarvestCycle, harvestEvent.Harvest.Id, harvestEvent.Harvest.HarvestCycleName));
        string plantName = string.IsNullOrEmpty(plantHarvest.PlantVarietyName) ? plantHarvest.PlantName : $"{plantHarvest.PlantName}-{plantHarvest.PlantVarietyName}";
        relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.PlantHarvestCycle, plantHarvest.Id, plantName));

        var command = new CreateWorkLogCommand()
        {
            EnteredDateTime = DateTime.Now,
            EventDateTime = plantHarvest.SeedingDate.HasValue ? plantHarvest.SeedingDate.Value : DateTime.Now,
            Log = note.ToString(),
            Reason = WorkLogReasonEnum.SowIndoors,
            RelatedEntities = relatedEntities
        };
        await _workLogCommandHandler.CreateWorkLog(command);
    }

    public async Task GenerateSowOutsideWorkLog(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First((Func<PlantHarvestCycle, bool>)(p => p.Id == harvestEvent.TriggerEntity!.EntityId));
        if (plantHarvest.PlantingMethod != PlantingMethodEnum.DirectSeed) { return; }

        StringBuilder note = new();
        if (plantHarvest.NumberOfSeeds.HasValue) { note.Append($"{plantHarvest.NumberOfSeeds} seeds of "); }
        note.Append(plantHarvest.PlantName);
        if (!string.IsNullOrWhiteSpace(plantHarvest.PlantVarietyName)) { note.Append($"-{plantHarvest.PlantVarietyName} "); }
        if (!string.IsNullOrWhiteSpace(plantHarvest.SeedCompanyName)) { note.Append($"from {plantHarvest.SeedCompanyName} "); }
        if (plantHarvest.SeedingDate.HasValue) note.Append($" were seeded outside on {plantHarvest.SeedingDate.Value.ToShortDateString()} ");

        var relatedEntities = new List<RelatedEntity>();
        relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.HarvestCycle, harvestEvent.Harvest.Id, harvestEvent.Harvest.HarvestCycleName));
        string plantName = string.IsNullOrEmpty(plantHarvest.PlantVarietyName) ? plantHarvest.PlantName : $"{plantHarvest.PlantName}-{plantHarvest.PlantVarietyName}";
        relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.PlantHarvestCycle, plantHarvest.Id, plantName));

        var command = new CreateWorkLogCommand()
        {
            EnteredDateTime = DateTime.Now,
            EventDateTime = plantHarvest.SeedingDate.HasValue ? plantHarvest.SeedingDate.Value : DateTime.Now,
            Log = note.ToString(),
            Reason = WorkLogReasonEnum.SowOutside,
            RelatedEntities = relatedEntities
        };
        await _workLogCommandHandler.CreateWorkLog(command);
    }

    public async Task GenerateInformationWorkLogForGenrmatedEvent(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First((Func<PlantHarvestCycle, bool>)(p => p.Id == harvestEvent.TriggerEntity!.EntityId));

        StringBuilder note = new();
        if (plantHarvest.GerminationRate.HasValue) { note.Append($"{plantHarvest.GerminationRate.Value}% germanation of "); }
        note.Append(plantHarvest.PlantName);
        if (!string.IsNullOrWhiteSpace(plantHarvest.PlantVarietyName)) { note.Append($"-{plantHarvest.PlantVarietyName} "); }
        if (!string.IsNullOrWhiteSpace(plantHarvest.SeedCompanyName)) { note.Append($"from {plantHarvest.SeedCompanyName} "); }
        if (plantHarvest.GerminationDate.HasValue) note.Append($" were germinated on {plantHarvest.GerminationDate.Value.ToShortDateString()} ");

        var relatedEntities = new List<RelatedEntity>();
        relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.HarvestCycle, harvestEvent.Harvest.Id, harvestEvent.Harvest.HarvestCycleName));
        relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.PlantHarvestCycle, plantHarvest.Id, plantHarvest.PlantName));

        var command = new CreateWorkLogCommand()
        {
            EnteredDateTime = DateTime.Now,
            EventDateTime = plantHarvest.GerminationDate.HasValue ? plantHarvest.GerminationDate.Value : DateTime.Now,
            Log = note.ToString(),
            Reason = WorkLogReasonEnum.Information,
            RelatedEntities = relatedEntities
        };
        await _workLogCommandHandler.CreateWorkLog(command);
    }

    public async Task GenerateTransplantOutsideWorkLog(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First((Func<PlantHarvestCycle, bool>)(p => p.Id == harvestEvent.TriggerEntity!.EntityId));

        StringBuilder note = new();
        if (plantHarvest.NumberOfTransplants.HasValue) { note.Append($"{plantHarvest.NumberOfTransplants} plants of "); }
        note.Append(plantHarvest.PlantName);
        if (!string.IsNullOrWhiteSpace(plantHarvest.PlantVarietyName)) { note.Append($"-{plantHarvest.PlantVarietyName} "); }
        if (!string.IsNullOrWhiteSpace(plantHarvest.SeedCompanyName)) { note.Append($"from {plantHarvest.SeedCompanyName} "); }
        if(plantHarvest.TransplantDate.HasValue) note.Append($" were transplanted outside on {plantHarvest.TransplantDate.Value.ToShortDateString()} ");

        var relatedEntities = new List<RelatedEntity>();
        relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.HarvestCycle, harvestEvent.Harvest.Id, harvestEvent.Harvest.HarvestCycleName));
        string plantName = string.IsNullOrEmpty(plantHarvest.PlantVarietyName) ? plantHarvest.PlantName : $"{plantHarvest.PlantName}-{plantHarvest.PlantVarietyName}";
        relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.PlantHarvestCycle, plantHarvest.Id, plantName));

        var command = new CreateWorkLogCommand()
        {
            EnteredDateTime = DateTime.Now,
            EventDateTime = plantHarvest.TransplantDate.HasValue?plantHarvest.TransplantDate.Value:DateTime.Now,
            Log = note.ToString(),
            Reason = WorkLogReasonEnum.TransplantOutside,
            RelatedEntities = relatedEntities
        };
        await _workLogCommandHandler.CreateWorkLog(command);
    }

    public async Task GenerateHarvestWorkLog(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First((Func<PlantHarvestCycle, bool>)(p => p.Id == harvestEvent.TriggerEntity!.EntityId));

        StringBuilder note = new();
        if (plantHarvest.TotalItems.HasValue) { note.Append($"{plantHarvest.TotalItems} of plants. "); }
        if (plantHarvest.TotalWeightInPounds.HasValue) { note.Append($"{plantHarvest.TotalWeightInPounds}lb of "); }
        note.Append(plantHarvest.PlantName);
        if (!string.IsNullOrWhiteSpace(plantHarvest.PlantVarietyName)) { note.Append($"-{plantHarvest.PlantVarietyName} "); }
        if (!string.IsNullOrWhiteSpace(plantHarvest.SeedCompanyName)) { note.Append($"from {plantHarvest.SeedCompanyName} "); }
        if (plantHarvest.FirstHarvestDate.HasValue) note.Append($" were harvested on {plantHarvest.FirstHarvestDate.Value.ToShortDateString()} ");

        var relatedEntities = new List<RelatedEntity>();
        relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.HarvestCycle, harvestEvent.Harvest.Id, harvestEvent.Harvest.HarvestCycleName));
        string plantName = string.IsNullOrEmpty(plantHarvest.PlantVarietyName) ? plantHarvest.PlantName : $"{plantHarvest.PlantName}-{plantHarvest.PlantVarietyName}";
        relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.PlantHarvestCycle, plantHarvest.Id, plantName));

        var command = new CreateWorkLogCommand()
        {
            EnteredDateTime = DateTime.Now,
            EventDateTime = plantHarvest.FirstHarvestDate.HasValue ? plantHarvest.FirstHarvestDate.Value : DateTime.Now,
            Log = note.ToString(),
            Reason = WorkLogReasonEnum.Harvest,
            RelatedEntities = relatedEntities
        };
        await _workLogCommandHandler.CreateWorkLog(command);
    }

    public async Task GenerateLastHarvestWorkLog(HarvestEvent harvestEvent)
    {
        var plantHarvest = harvestEvent.Harvest!.Plants.First((Func<PlantHarvestCycle, bool>)(p => p.Id == harvestEvent.TriggerEntity!.EntityId));

        if (plantHarvest.FirstHarvestDate.HasValue && plantHarvest.LastHarvestDate.HasValue && plantHarvest.FirstHarvestDate.Value.Date == plantHarvest.LastHarvestDate.Value.Date) { return; }

        StringBuilder note = new();
        if (plantHarvest.TotalItems.HasValue) { note.Append($"{plantHarvest.TotalItems} of plants. "); }
        if (plantHarvest.TotalWeightInPounds.HasValue) { note.Append($"{plantHarvest.TotalWeightInPounds}lb of "); }
        note.Append(plantHarvest.PlantName);
        if (!string.IsNullOrWhiteSpace(plantHarvest.PlantVarietyName)) { note.Append($"-{plantHarvest.PlantVarietyName} "); }
        if (!string.IsNullOrWhiteSpace(plantHarvest.SeedCompanyName)) { note.Append($"from {plantHarvest.SeedCompanyName} "); }
        if(plantHarvest.LastHarvestDate.HasValue) note.Append($" were completely harvested on {plantHarvest.LastHarvestDate.Value.ToShortDateString()} ");

        var relatedEntities = new List<RelatedEntity>();
        relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.HarvestCycle, harvestEvent.Harvest.Id, harvestEvent.Harvest.HarvestCycleName));
        string plantName = string.IsNullOrEmpty(plantHarvest.PlantVarietyName) ? plantHarvest.PlantName : $"{plantHarvest.PlantName}-{plantHarvest.PlantVarietyName}";
        relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.PlantHarvestCycle, plantHarvest.Id, plantName));

        var command = new CreateWorkLogCommand()
        {
            EnteredDateTime = DateTime.Now,
            EventDateTime = plantHarvest.LastHarvestDate.HasValue?plantHarvest.LastHarvestDate.Value:DateTime.Now,
            Log = note.ToString(),
            Reason = WorkLogReasonEnum.Harvest,
            RelatedEntities = relatedEntities
        };
        await _workLogCommandHandler.CreateWorkLog(command);
    }
}
