namespace PlantCatalog.Contract.ViewModels;

public record PlantNameOnlyViewModel
{
    public string PlantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
