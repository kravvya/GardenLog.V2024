namespace ImageCatalog.Api.Model;


public record ImageEvent : BaseDomainEvent
{
    public Image? Image { get; init; }
    public ImageCatalogEventTriggerEnum Trigger { get; init; }
    public RelatedEntity? TriggerEntity { get; init; }
    public string ImageId { get { return Image!.Id; } init { } }
    public string UserProfileId { get { return Image!.UserProfileId; } init { } }

    private ImageEvent() { }

    public ImageEvent(Image seed, ImageCatalogEventTriggerEnum trigger, RelatedEntity triggerEntity)
    {
        Image = seed;
        Trigger = trigger;
        TriggerEntity = triggerEntity;
    }


}