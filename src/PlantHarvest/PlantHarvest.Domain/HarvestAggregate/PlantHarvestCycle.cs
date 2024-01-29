using PlantHarvest.Contract.Commands;
using PlantHarvest.Domain.HarvestAggregate.Events;

namespace PlantHarvest.Domain.HarvestAggregate;

public class PlantHarvestCycle : BaseEntity, IEntity
{
    public string PlantId { get; private set; } = string.Empty;
    public string PlantName { get; private set; } = string.Empty;

    public string? PlantVarietyId { get; private set; }
    public string? PlantVarietyName { get; private set; }

    public string? PlantGrowthInstructionId { get; private set; }
    public string? PlantGrowthInstructionName { get; private set; }
    public PlantingMethodEnum PlantingMethod { get; private set; }

    public int? NumberOfSeeds { get; private set; }
    public string? SeedCompanyId { get; private set; }
    public string? SeedCompanyName { get; private set; }

    public DateTime? SeedingDate { get; private set; }

    public DateTime? GerminationDate { get; private set; }
    public decimal? GerminationRate { get; private set; }

    public int? NumberOfTransplants { get; private set; }
    public DateTime? TransplantDate { get; private set; }

    public DateTime? FirstHarvestDate { get; private set; }
    public DateTime? LastHarvestDate { get; private set; }

    public decimal? TotalWeightInPounds { get; private set; }
    public int? TotalItems { get; private set; }

    public string Notes { get; private set; } = string.Empty;
    public int? DesiredNumberOfPlants { get; private set; }
    public int? SpacingInInches { get; private set; }
    public double? PlantsPerFoot { get; private set; }

    private readonly List<PlantSchedule> _plantCalendar = new();
    public IReadOnlyCollection<PlantSchedule> PlantCalendar => _plantCalendar.AsReadOnly();

    private readonly List<GardenBedPlantHarvestCycle> _gardenBedLayout = new();
    public IReadOnlyCollection<GardenBedPlantHarvestCycle> GardenBedLayout => _gardenBedLayout.AsReadOnly();

    private PlantHarvestCycle()
    {
    }

    private PlantHarvestCycle(string plantId, string plantName
        , string? plantVarietyId, string? plantVarietyName
        , string plantGrowthInstructionId, string? plantGrowthInstructionName
        , PlantingMethodEnum plantingMethod
        , int? numberOfSeeds, string? seedCompanyId, string? seedCompanyName, DateTime? seedingDate
        , DateTime? germinationDate, decimal? germinationRate
        , int? numberOfTransplants, DateTime? transplantDate
        , DateTime? firstHarvestDate, DateTime? lastHarvestDate, decimal? totalWeightInPounds, int? totalItems
        , string Notes, int? desiredNumberOfPlants, int? spacingInInches, double? plantsPerFoot, List<PlantSchedule> plantCalendar, List<GardenBedPlantHarvestCycle> gardenBedLayout)
    {
        this.PlantId = plantId;
        this.PlantName = plantName;
        this.PlantVarietyId = plantVarietyId;
        this.PlantVarietyName = plantVarietyName;
        this.PlantGrowthInstructionId = plantGrowthInstructionId;
        this.PlantGrowthInstructionName = plantGrowthInstructionName;
        this.PlantingMethod = plantingMethod;
        this.NumberOfSeeds = numberOfSeeds;
        this.SeedCompanyId = seedCompanyId;
        this.SeedCompanyName = seedCompanyName;
        this.SeedingDate = seedingDate;
        this.GerminationDate = germinationDate;
        this.GerminationRate = germinationRate;
        this.NumberOfTransplants = numberOfTransplants;
        this.TransplantDate = transplantDate;
        this.FirstHarvestDate = firstHarvestDate;
        this.LastHarvestDate = lastHarvestDate;
        this.TotalWeightInPounds = totalWeightInPounds;
        this.TotalItems = totalItems;
        this.Notes = Notes;
        this.DesiredNumberOfPlants = desiredNumberOfPlants;
        this.SpacingInInches = spacingInInches == 0 ? null : spacingInInches;
        this.PlantsPerFoot = plantsPerFoot == 0 ? null : plantsPerFoot;
        this._plantCalendar = plantCalendar;
        this._gardenBedLayout = gardenBedLayout;
    }


    public static PlantHarvestCycle Create(PlantHarvestCycleBase plant, Action<HarvestEventTriggerEnum, TriggerEntity> addHarvestEvent)
    {
        var harvestPlant =  new PlantHarvestCycle()
        {
            Id = Guid.NewGuid().ToString(),
            PlantId = plant.PlantId,
            PlantName = plant.PlantName,
            PlantVarietyId = plant.PlantVarietyId,
            PlantVarietyName = plant.PlantVarietyName,
            PlantGrowthInstructionId = plant.PlantGrowthInstructionId,
            PlantGrowthInstructionName = plant.PlantGrowthInstructionName,
            NumberOfSeeds = plant.NumberOfSeeds,
            SeedCompanyId = plant.SeedVendorId,
            SeedCompanyName = plant.SeedVendorName,            
           
            GerminationRate = plant.GerminationRate,
            NumberOfTransplants = plant.NumberOfTransplants,
            
            TotalWeightInPounds = plant.TotalWeightInPounds,
            TotalItems = plant.TotalItems,
            Notes = plant.Notes,
            DesiredNumberOfPlants = plant.DesiredNumberOfPlants,
            SpacingInInches = plant.SpacingInInches,
            PlantsPerFoot = plant.PlantsPerFoot,
            PlantingMethod = plant.PlantingMethod,
        };

        //only add updates here that have dedicated events. Otherwise - generic update will be published.
        harvestPlant.Set<DateTime?>(() => harvestPlant.SeedingDate, plant.SeedingDate);
        harvestPlant.Set<DateTime?>(() => harvestPlant.GerminationDate, plant.GerminationDate);
        harvestPlant.Set<DateTime?>(() => harvestPlant.TransplantDate, plant.TransplantDate);
        harvestPlant.Set<DateTime?>(() => harvestPlant.FirstHarvestDate, plant.FirstHarvestDate);
        harvestPlant.Set<DateTime?>(() => harvestPlant.LastHarvestDate, plant.LastHarvestDate);

        foreach (var evt in harvestPlant.DomainEvents)
        {
            addHarvestEvent(((HarvestChildEvent)evt).Trigger, new TriggerEntity(EntityTypeEnum.PlantHarvestCycle, harvestPlant.Id));
        }

        return harvestPlant;
    }


    public void Update(UpdatePlantHarvestCycleCommand command, Action<HarvestEventTriggerEnum, TriggerEntity> addHarvestEvent)
    {
        this.Set<string?>(() => this.PlantVarietyId, command.PlantVarietyId);
        this.Set<string?>(() => this.PlantVarietyName, command.PlantVarietyName);
        this.Set<int?>(() => this.NumberOfSeeds, command.NumberOfSeeds);
        this.Set<string?>(() => this.SeedCompanyId, command.SeedVendorId);
        this.Set<string?>(() => this.SeedCompanyName, command.SeedVendorName);
        this.Set<DateTime?>(() => this.SeedingDate, command.SeedingDate);
        this.Set<DateTime?>(() => this.GerminationDate, command.GerminationDate);
        this.Set<decimal?>(() => this.GerminationRate, command.GerminationRate);
        this.Set<int?>(() => this.NumberOfTransplants, command.NumberOfTransplants);
        this.Set<DateTime?>(() => this.TransplantDate, command.TransplantDate);
        this.Set<DateTime?>(() => this.FirstHarvestDate, command.FirstHarvestDate);
        this.Set<DateTime?>(() => this.LastHarvestDate, command.LastHarvestDate);
        this.Set<decimal?>(() => this.TotalWeightInPounds, command.TotalWeightInPounds);
        this.Set<int?>(() => this.TotalItems, command.TotalItems);
        this.Set<string>(() => this.Notes, command.Notes);
        this.Set<int?>(() => this.DesiredNumberOfPlants, command.DesiredNumberOfPlants);
        this.Set<int?>(() => this.SpacingInInches, command.SpacingInInches);
        this.Set<double?>(() => this.PlantsPerFoot, command.PlantsPerFoot);
        foreach (var evt in DomainEvents)
        {
            addHarvestEvent(((HarvestChildEvent)evt).Trigger, new TriggerEntity(EntityTypeEnum.PlantHarvestCycle, this.Id));
        }
    }

    protected override void AddDomainEvent(string attributeName)
    {
        switch (attributeName)
        {
            case "SeedingDate":
                if (this.SeedingDate.HasValue) this.DomainEvents.Add(new HarvestChildEvent(HarvestEventTriggerEnum.PlantHarvestCycleSeeded, new TriggerEntity(EntityTypeEnum.PlantHarvestCycle, this.Id)));
                break;
            case "GerminationDate":
                if (this.GerminationDate.HasValue) this.DomainEvents.Add(new HarvestChildEvent(HarvestEventTriggerEnum.PlantHarvestCycleGerminated, new TriggerEntity(EntityTypeEnum.PlantHarvestCycle, this.Id)));
                break;
            case "TransplantDate":
                if (this.TransplantDate.HasValue) this.DomainEvents.Add(new HarvestChildEvent(HarvestEventTriggerEnum.PlantHarvestCycleTransplanted, new TriggerEntity(EntityTypeEnum.PlantHarvestCycle, this.Id)));
                break;
            case "FirstHarvestDate":
                if (FirstHarvestDate.HasValue) this.DomainEvents.Add(new HarvestChildEvent(HarvestEventTriggerEnum.PlantHarvestCycleHarvested, new TriggerEntity(EntityTypeEnum.PlantHarvestCycle, this.Id)));
                break;
            case "LastHarvestDate":
                if (LastHarvestDate.HasValue) this.DomainEvents.Add(new HarvestChildEvent(HarvestEventTriggerEnum.PlantHarvestCycleCompleted, new TriggerEntity(EntityTypeEnum.PlantHarvestCycle, this.Id)));
                break;
            default:
                if (!this.DomainEvents.Any(e => ((HarvestChildEvent)e).Trigger == HarvestEventTriggerEnum.PlantHarvestCycleUpdated))
                {
                    this.DomainEvents.Add(new HarvestChildEvent(HarvestEventTriggerEnum.PlantHarvestCycleUpdated, new TriggerEntity(EntityTypeEnum.PlantHarvestCycle, this.Id)));
                }
                break;
        }

    }

    #region Plant Schedule
    public string AddPlantSchedule(CreatePlantScheduleCommand command)
    {
        var schedule = PlantSchedule.Create(command);

        this._plantCalendar.Add(schedule);

        return schedule.Id;
    }

    public void UpdatePlantSchedule(UpdatePlantScheduleCommand command, Action<HarvestEventTriggerEnum, TriggerEntity> addHarvestEvent)
    {
        this.PlantCalendar.First(i => i.Id == command.PlantScheduleId).Update(command, addHarvestEvent);
    }

    public void DeletePlantSchedule(string plantScheduleId)
    {
        this._plantCalendar.RemoveAll(s => s.Id == plantScheduleId);
    }

    public void DeleteAllSystemGeneratedSchedules()
    {
        this._plantCalendar.RemoveAll(s => s.IsSystemGenerated);
    }
    #endregion

    #region Garden Bed Layout
    public string AddGardenBedPlantHarvestCycle(CreateGardenBedPlantHarvestCycleCommand command)
    {
        var gardenBedPlant = GardenBedPlantHarvestCycle.Create(command);

        this._gardenBedLayout.Add(gardenBedPlant);

        return gardenBedPlant.Id;
    }

    public void UpdateGardenBedPlantHarvestCycle(UpdateGardenBedPlantHarvestCycleCommand command, Action<HarvestEventTriggerEnum, TriggerEntity> addHarvestEvent)
    {
        this.GardenBedLayout.First(i => i.Id == command.GardenBedPlantHarvestCycleId).Update(command, addHarvestEvent);
    }

    public void DeleteGardenBedPlantHarvestCycle(string pGardenBedPlantHarvestCycleId)
    {
        this._gardenBedLayout.RemoveAll(s => s.Id == pGardenBedPlantHarvestCycleId);
    }


    #endregion
}
