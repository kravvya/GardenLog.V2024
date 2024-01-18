namespace PlantCatalog.Contract.Base;

public abstract record PlantVarietyBase
{
    public string Name { get; set; } = string.Empty; 
    public string Description { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PlantId { get; set; } = string.Empty;
    public int? DaysToMaturityMin { get; set; }
    public int? DaysToMaturityMax { get; set; }
    public int? HeightInInches { get; set; }    
    public bool IsHeirloom { get; set; }    
    public MoistureRequirementEnum MoistureRequirement { get; set; }    
    public LightRequirementEnum LightRequirement { get; set; }
    public GrowToleranceEnum GrowTolerance { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Colors { get; set; } = new();
    public List<string> Sources { get; set; } = new();
}
