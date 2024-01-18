using PlantCatalog.Contract.Commands;
using System.Runtime.CompilerServices;

namespace PlantCatalog.Domain.PlantAggregate;

public class Plant : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;
    public PlantLifecycleEnum Lifecycle { get; private set; }
    public PlantTypeEnum Type { get; private set; }
    public MoistureRequirementEnum MoistureRequirement { get; private set; }
    public LightRequirementEnum LightRequirement { get; private set; }
    public GrowToleranceEnum GrowTolerance { get; private set; }
    public string GardenTip { get; private set; } = string.Empty;
    public int? SeedViableForYears { get; private set; }
    public int? DaysToMaturityMin { get; set; }
    public int? DaysToMaturityMax { get; set; }
    public int GrowInstructionsCount
    {
        get{
            return _growInstructions.Count();
        }
    }
   public int VarietyCount { get; private set; }

    private readonly List<string> _tags = new();
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    private readonly List<string> _varietyColors = new();
    public IReadOnlyCollection<string> VarietyColors => _varietyColors.AsReadOnly();

    private readonly List<PlantGrowInstruction> _growInstructions = new();
    public IReadOnlyCollection<PlantGrowInstruction> GrowInstructions => _growInstructions.AsReadOnly();
    public HarvestSeasonEnum HarvestSeason { get; private set; }

    private Plant() { }

    private Plant(string Name, string Description, string Color, PlantLifecycleEnum Lifecycle, PlantTypeEnum Type, MoistureRequirementEnum MoistureRequirement
        ,LightRequirementEnum LightRequirement, GrowToleranceEnum GrowTolerance, string GardenTip, int? SeedViableForYears, int? DaysToMaturityMin, int? DaysToMaturityMax
        , int GrowInstructionCount, int VarietyCount, List<string> Tags, List<string> VarietyColors, List<PlantGrowInstruction> GrowInstructions
        ,HarvestSeasonEnum HarvestSeason)
    {
        this.Name = Name;
        this.Description = Description;
        this.Color = Color;
        this.Lifecycle = Lifecycle;
        this.Type = Type;
        this.MoistureRequirement = MoistureRequirement;
        this.LightRequirement = LightRequirement;
        this.GrowTolerance = GrowTolerance;
        this.GardenTip = GardenTip;
        this.SeedViableForYears = SeedViableForYears;
        this.VarietyCount= VarietyCount;
        _tags = Tags;
        _varietyColors = VarietyColors;
        _growInstructions = GrowInstructions;
        this.HarvestSeason = HarvestSeason;
        this.DaysToMaturityMin= DaysToMaturityMin;
        this.DaysToMaturityMax= DaysToMaturityMax;
    }

    public static Plant Create(
        string name,
        string description,
        string color,
        PlantTypeEnum type,
        PlantLifecycleEnum lifecycle,
        MoistureRequirementEnum moistureRequirement,
        LightRequirementEnum lightRequirement,
        GrowToleranceEnum growTolerance,
        string gardenTip,
        int? seedViableForYears,
        IList<string> tags,
        IList<string> varietyColors,
        int? daysToMaturityMin,
        int? daysToMaturityMax
        )
    {
        var plant = new Plant()
        {
            Id = System.Guid.NewGuid().ToString(),
            Name = name ?? throw new ArgumentNullException(nameof(name)),
            Description = description ?? throw new ArgumentNullException(nameof(description)),
            Color = color ?? throw new ArgumentNullException(nameof(color)),
            Type = type,
            Lifecycle = lifecycle,
            MoistureRequirement = moistureRequirement,
            LightRequirement = lightRequirement,
            GrowTolerance = growTolerance,
            GardenTip = gardenTip,
            SeedViableForYears = seedViableForYears,
            DaysToMaturityMin = daysToMaturityMin,
            DaysToMaturityMax=daysToMaturityMax
        };

        plant._tags.AddRange(tags);
        plant._varietyColors.AddRange(varietyColors);

        plant.DomainEvents.Add(
            new PlantEvent(plant, PlantEventTriggerEnum.PlantCreated, new Events.Meta.TriggerEntity(EntityTypeEnum.Plant, plant.Id)));

        return plant;

    }

    public void Update(
        string name,
        string description,
        string color,
        PlantTypeEnum type,
        PlantLifecycleEnum lifecycle,
        MoistureRequirementEnum moistureRequirement,
        LightRequirementEnum lightRequirement,
        GrowToleranceEnum growTolerance,
        string gardenTip,
        int? seedViableForYears,
        List<string> tags,
        List<string> varietyColors,
        int? daysToMaturityMin, 
        int? daysToMaturityMax)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Color = color ?? throw new ArgumentNullException(nameof(color));
        Type = type;
        Lifecycle = lifecycle;
        MoistureRequirement = moistureRequirement;
        LightRequirement = lightRequirement;
        GrowTolerance = growTolerance;
        GardenTip = gardenTip;
        SeedViableForYears = seedViableForYears;
        DaysToMaturityMin = daysToMaturityMin;
        DaysToMaturityMax = daysToMaturityMax;

        UpdateCollection<string>(this._tags, tags);
        UpdateCollection<string>(this._varietyColors, varietyColors);

        this.DomainEvents.Add(
           new PlantEvent(this, PlantEventTriggerEnum.PlantUpdated, new Events.Meta.TriggerEntity(EntityTypeEnum.Plant, this.Id)));
    }

    private static void UpdateCollection<T>(List<T> existingList, List<T> newList)
    {
        var elementsToRemove = existingList.Where(t => !newList.Contains(t));
        if (elementsToRemove.Any())
        {
            //logic to do something in case if tags are in use
            existingList.RemoveAll(t => elementsToRemove.Contains(t));
        }

        newList.RemoveAll(t => existingList.Contains(t));

        existingList.AddRange(newList);
    }

    #region Events
    protected override void AddDomainEvent(string attributeName)
    {
        this.DomainEvents.Add(
          new PlantEvent(this, PlantEventTriggerEnum.PlantUpdated, new Events.Meta.TriggerEntity(EntityTypeEnum.Plant, this.Id)));
    }

    private void AddChildDomainEvent(PlantEventTriggerEnum trigger, Events.Meta.TriggerEntity entity)
    {
        var newEvent = new PlantEvent(this, trigger, entity);

        if (!this.DomainEvents.Contains(newEvent))
            this.DomainEvents.Add(newEvent);
    }
    #endregion

    #region Grow Instructions

    public string AddPlantGrowInstruction(CreatePlantGrowInstructionCommand command)
    {
        var instruction = PlantGrowInstruction.Create(command);

        this.HarvestSeason = this.HarvestSeason |= instruction.HarvestSeason;

        this._growInstructions.Add(instruction);

        this.DomainEvents.Add(
          new PlantEvent(this, PlantEventTriggerEnum.GrowInstructionAddedToPlant, new Events.Meta.TriggerEntity(EntityTypeEnum.GrowingInstruction, instruction.Id)));

        return instruction.Id;
    }

    public void UpdatePlantGrowInstructions(UpdatePlantGrowInstructionCommand command)
    {
        this.GrowInstructions.First(i => i.Id == command.PlantGrowInstructionId).Update(command, AddChildDomainEvent);
        this.HarvestSeason = HarvestSeasonEnum.Unspecified;
        foreach (var grow in this._growInstructions)
        {
            this.HarvestSeason |= grow.HarvestSeason;
        }
    }

    public void DeletePlantGrowInstruction(string plantGrowInstructionId)
    {
        this._growInstructions.RemoveAll(i => i.Id == plantGrowInstructionId);

        this.HarvestSeason = HarvestSeasonEnum.Unspecified;
        foreach(var grow in this._growInstructions)
        {
            this.HarvestSeason |= grow.HarvestSeason;
        }

        AddChildDomainEvent(PlantEventTriggerEnum.GrowInstructionDeleted, new Events.Meta.TriggerEntity(EntityTypeEnum.GrowingInstruction, plantGrowInstructionId));

    }

    #endregion

    #region Plant Variety

    public PlantVariety AddPlantVariety(CreatePlantVarietyCommand command)
    {
        var variety = PlantVariety.Create(command, this.Name);
        this.VarietyCount += 1;
                
        this.DomainEvents.Add(
          new PlantEvent(this, PlantEventTriggerEnum.PlantVarietyCreated, new Events.Meta.TriggerEntity(EntityTypeEnum.PlantVariety, variety.Id)));

        return variety;
    }

    public void UpdatePlantVariety(UpdatePlantVarietyCommand command, PlantVariety variety)
    {
        
        variety.Update(command, AddChildDomainEvent);
    }

    public void DeletePlantVariety(string varietyId)
    {
        this.VarietyCount -= 1;

        AddChildDomainEvent(PlantEventTriggerEnum.PlantVarietyDeleted, new Events.Meta.TriggerEntity(EntityTypeEnum.PlantVariety, varietyId));

    }

    #endregion



}
