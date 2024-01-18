using PlantCatalog.Contract.Commands;

namespace PlantCatalog.Domain.PlantAggregate;

public class PlantVariety : BaseEntity, IEntity
{
    public string PlantId { get; private set; } = string.Empty;
    public string PlantName { get; set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int? DaysToMaturityMin { get; private set; }
    public int? DaysToMaturityMax { get; private set; }
    public int? HeightInInches { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public bool IsHeirloom { get; private set; }
    public MoistureRequirementEnum MoistureRequirement { get; private set; }
    public LightRequirementEnum LightRequirement { get; private set; }
    public GrowToleranceEnum GrowTolerance { get; private set; }

    private readonly List<string> _tags = new();
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    private readonly List<string> _colors = new();
    public IReadOnlyCollection<string> Colors => _colors.AsReadOnly();

    private readonly List<string> _sources = new();
    public IReadOnlyCollection<string> Sources => _sources.AsReadOnly();


    private PlantVariety() { }

    private PlantVariety(string plantId, string plantName, string name, string description
        , int? daysToMaturityMin, int? daysToMaturityMax, int? heightInInches, string title, bool isHeirloom
        , MoistureRequirementEnum moistureRequirement, LightRequirementEnum lightRequirement, GrowToleranceEnum growTolerance
        , List<string> tags, List<string> colors, List<string> sources)
    {
        PlantId = plantId;
        PlantName = plantName;
        Name = name;
        Description = description;
        DaysToMaturityMin = daysToMaturityMin;
        DaysToMaturityMax = daysToMaturityMax;
        HeightInInches = heightInInches;
        Title = title;
        IsHeirloom = isHeirloom;
        MoistureRequirement = moistureRequirement;
        LightRequirement = lightRequirement;
        GrowTolerance = growTolerance;
        _tags = tags;
        _colors = colors;
        _sources = sources;
    }

    public static PlantVariety Create(
        CreatePlantVarietyCommand command,
        string plantName
      )
    {
        var variety = new PlantVariety()
        {
            Id = Guid.NewGuid().ToString(),
            PlantName = plantName,
            PlantId = command.PlantId,
            Name = command.Name ?? throw new ArgumentNullException(nameof(command.Name)),
            Description = command.Description ?? throw new ArgumentNullException(nameof(command.Description)),
            DaysToMaturityMin = command.DaysToMaturityMin,
            DaysToMaturityMax = command.DaysToMaturityMax,
            HeightInInches = command.HeightInInches,
            IsHeirloom = command.IsHeirloom,
            MoistureRequirement = command.MoistureRequirement,
            LightRequirement = command.LightRequirement,
            GrowTolerance = command.GrowTolerance,
            Title = command.Title
        };
        variety._tags.AddRange(command.Tags);
        variety._colors.AddRange(command.Colors);
        variety._sources.AddRange(command.Sources);

        return variety;
    }

    public void Update(
        UpdatePlantVarietyCommand command,
        Action<PlantEventTriggerEnum, Events.Meta.TriggerEntity> addPlantEvent
    )
    {
        Set<string>(() => this.Name, command.Name);
        Set<string>(() => this.Description, command.Description);
        Set<int?>(() => this.DaysToMaturityMin, command.DaysToMaturityMin);
        Set<int?>(() => this.DaysToMaturityMax, command.DaysToMaturityMax);
        Set<int?>(() => this.HeightInInches, command.HeightInInches);
        Set<bool>(() => this.IsHeirloom, command.IsHeirloom);
        Set<MoistureRequirementEnum>(() => this.MoistureRequirement, command.MoistureRequirement);
        Set<LightRequirementEnum>(() => this.LightRequirement, command.LightRequirement);
        Set<GrowToleranceEnum>(() => this.GrowTolerance, command.GrowTolerance);
        Set<string>(() => this.Title, command.Title);

        SetCollection<string>(() => this._tags, command.Tags, "Tags");
        SetCollection<string>(() => this._colors, command.Colors, "Colors");
        SetCollection<string>(() => this._sources, command.Sources, "Sources");

        if (this.DomainEvents != null && this.DomainEvents.Count > 0)
        {
            this.DomainEvents.Clear();
            addPlantEvent(PlantEventTriggerEnum.PlantVarietyUpdated, new Events.Meta.TriggerEntity(EntityTypeEnum.PlantVariety, this.Id));
        }
    }

    protected override void AddDomainEvent(string attributeName)
    {
        this.DomainEvents.Add(
            new PlantChildEvent(PlantEventTriggerEnum.GrowInstructionUpdated, new Events.Meta.TriggerEntity(EntityTypeEnum.GrowingInstruction, this.Id)));
    }


}


