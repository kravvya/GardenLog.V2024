namespace PlantCatalog.Contract.Base;

public abstract record SeedVendorBase
{
    public string Name { get; set; } = string.Empty;
    public string WebSiteUrl { get; set; }=string.Empty;
    public bool HasPaperCatalog { get; set; }
}
