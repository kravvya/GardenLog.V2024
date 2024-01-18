namespace PlantCatalog.Contract.Base;


public abstract record PlantBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public PlantLifecycleEnum Lifecycle { get; set; }
    public PlantTypeEnum Type { get; set; }
    public MoistureRequirementEnum MoistureRequirement { get; set; }
    public LightRequirementEnum LightRequirement { get; set; }
    public GrowToleranceEnum GrowTolerance { get; set; }
    public string GardenTip { get; set; } = string.Empty;
    public int? SeedViableForYears { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> VarietyColors { get; set; } = new();
    public HarvestSeasonEnum HarvestSeason { get; set; }
    public int? DaysToMaturityMin { get; set; }
    public int? DaysToMaturityMax { get; set; }
}
