namespace ImageCatalog.Api.QueryHandlers;

public interface IImageQueryHandler
{
    Task<IReadOnlyCollection<ImageViewModel>> GetImagesByRelatedEntitiesAsync(GetImagesByRelatedEntities request, string userProfileId);
    Task<IReadOnlyCollection<ImageViewModel>> GetImagesByRelatedEntityAsync(GetImagesByRelatedEntity request, string userProfileId);
}

public class ImageQueryHandler : IImageQueryHandler
{
    private readonly IImageRepository _imageRepository;

    public ImageQueryHandler(IImageRepository imageRepository)
    {
        _imageRepository = imageRepository;
    }

    public Task<IReadOnlyCollection<ImageViewModel>> GetImagesByRelatedEntityAsync(GetImagesByRelatedEntity request, string userProfileId)
       => _imageRepository.GetImagesByRelatedEntityAsync(request, userProfileId);

    public Task<IReadOnlyCollection<ImageViewModel>> GetImagesByRelatedEntitiesAsync(GetImagesByRelatedEntities request, string userProfileId)
     => _imageRepository.GetImagesByRelatedEntitiesAsync(request, userProfileId);
}
