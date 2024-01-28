using ImageCatalog.Contract.Base;

namespace ImageCatalog.Contract.ViewModels;

public record ImageViewModel : ImageBase
{
    public string ImageId { get; set; } = string.Empty;
    public DateTime CreatedDateTimeUtc { get; set; }
}
