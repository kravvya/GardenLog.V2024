using GardenLog.SharedKernel;
using GardenLog.SharedKernel.Enum;

namespace ImageCatalog.Contract.Base;

public abstract record ImageBase
{
    public string ImageName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public RelatedEntityTypEnum RelatedEntityType { get; set; }
    public string RelatedEntityId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public List<RelatedEntity> RelatedEntities { get; set; } = new();
}
