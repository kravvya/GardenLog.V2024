namespace PlantCatalog.Contract.ViewModels;

public record SeedVendorViewModel: SeedVendorBase
{
    public string SeedVendorId { get; set; } = string.Empty;
}
