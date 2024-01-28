namespace ImageCatalog.Api.CommandHandlers;

public interface IImageCommandHandler
{
    Task<string> CreateImageAsync(string userProfileId, CreateImageCommand request);
    Task<bool> DeleteImageAsync(string imageId);
    Task<bool> UpdateImageAsync(UpdateImageCommand request);
}

public class ImageCommandHandler : IImageCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageRepository _imageRepository;
    private readonly ILogger<ImageCommandHandler> _logger;

    public ImageCommandHandler(IUnitOfWork unitOfWork, IImageRepository imageRepository, ILogger<ImageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _imageRepository = imageRepository;
        _logger = logger;
    }

    public async Task<string> CreateImageAsync(string userProfileId, CreateImageCommand request)
    {
        _logger.LogInformation("Received request to create a new image {@plant}", request);

        var image = Model.Image.Create(
            request.ImageName,
            request.Label,
            request.RelatedEntityType,
            request.RelatedEntityId,
            request.FileName,
            request.FileType,
            userProfileId,
            request.RelatedEntities
           );

        _imageRepository.Add(image);

        await _unitOfWork.SaveChangesAsync();

        return image.Id;

    }

    public async Task<bool> UpdateImageAsync(UpdateImageCommand request)
    {
        _logger.LogInformation("Received request to update image {@plant}", request);

        var image = await _imageRepository.GetByIdAsync(request.ImageId);

        image.Update(request.Label);

        _imageRepository.Update(image);

        await _unitOfWork.SaveChangesAsync();

        return true;

    }

    public async Task<bool> DeleteImageAsync(string imageId)
    {
        _logger.LogInformation("Received request to delete image {@imageId}", imageId);

        _imageRepository.Delete(imageId);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
