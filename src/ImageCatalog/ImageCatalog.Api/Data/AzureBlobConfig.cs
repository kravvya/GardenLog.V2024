namespace ImageCatalog.Api.Data;

public record AzureBlobConfig
{
    public const string Key = "AzureBlobConfig";

    public string AccountName { get; set; } = string.Empty;
    public string ImageContainer { get; set; } = string.Empty;
    public string ThumbnailContainer { get; set; } = string.Empty;
}



