using GardenLog.SharedKernel;

namespace ImageCatalog.Api.Model;

public class Image : BaseEntity, IEntity
{
    public string UserProfileId { get; private set; } = string.Empty;
    public string ImageName { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public RelatedEntityTypEnum RelatedEntityType { get; private set; }
    public string RelatedEntityId { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string FileType { get; private set; } = string.Empty;
    public DateTime CreatedDateTimeUtc { get; private set; }
    public IList<RelatedEntity>? RelatedEntities { get; private set; }

    private Image()
    {

    }

    public static Image Create(
        string imageName,
        string label,
        RelatedEntityTypEnum relatedEntity,
        string relatedEntityId,
        string fileName,
        string fileType,
        string userProfileId,
        IList<RelatedEntity> relatedEntities
        )
    {
        if (relatedEntities == null) { relatedEntities = new List<RelatedEntity>(); }
        if (!relatedEntities.Any(e => e.EntityType == relatedEntity)) { relatedEntities.Add(new RelatedEntity(relatedEntity, relatedEntityId, string.Empty)); }

        var image = new Image()
        {
            Id = System.Guid.NewGuid().ToString(),
            ImageName = imageName,
            Label = label,
            RelatedEntityType = relatedEntity,
            RelatedEntityId = relatedEntityId,
            FileName = fileName,
            FileType = fileType,
            UserProfileId = userProfileId,
            CreatedDateTimeUtc = DateTime.UtcNow,
            RelatedEntities= relatedEntities
        };

        image.DomainEvents.Add(
            new ImageEvent(image, ImageCatalogEventTriggerEnum.ImageCreated, new RelatedEntity(image.RelatedEntityType, image.RelatedEntityId, string.Empty)));

        return image;
    }


    public void Update(string label)
    {
        this.Set<string>(() => this.Label, label);
    }

    public void SetRelatedEntities(IList<RelatedEntity> relatedEntities)
    {
        this.RelatedEntities = relatedEntities;
    }

    protected override void AddDomainEvent(string attributeName)
    {
        if (this.DomainEvents.Count == 0)
        {
            if (attributeName.Equals("Label"))
                this.DomainEvents.Add(new ImageEvent(this, ImageCatalogEventTriggerEnum.LabelChanged, new RelatedEntity(this.RelatedEntityType, this.RelatedEntityId, string.Empty)));
        }
    }

}

