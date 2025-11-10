namespace ImageCatalog.Api.Services;

public interface IFileCommandHandler
{
    string GenerateSasToken(string fileName);
    void ProcessUploadedImage(string fileName);
    Task ProcessUploadedImageAsync(string fileName);
}

public class FileCommandHandler : IFileCommandHandler
{
    readonly IFileRepository _fileRepository;
    private readonly ILogger<FileCommandHandler> _logger;
    private readonly IImageConversionService _conversionService;
    private readonly IImageProcessor _thumbnailProcessor;
    private readonly int _thumbnailWidth;

    public FileCommandHandler(
        IFileRepository fileRepository, 
        ILogger<FileCommandHandler> logger,
        IImageConversionService conversionService,
        IEnumerable<IImageProcessor> imageProcessors,
        IConfiguration configuration)
    {
        _fileRepository = fileRepository;
        _logger = logger;
        _conversionService = conversionService;
        _thumbnailWidth = configuration.GetValue<int>("ThumbnailWidth");
        
        // Use ImageSharp for all thumbnail generation (it handles JPEG well)
        _thumbnailProcessor = imageProcessors.FirstOrDefault(p => p is ImageSharpProcessor) 
            ?? throw new InvalidOperationException("ImageSharpProcessor not registered");
    }

    public string GenerateSasToken(string fileName) => _fileRepository.GenerateSasToken(fileName);

    public void ProcessUploadedImage(string fileName)
    {
        Task.Run(async () =>
        {
            try
            {
                await ProcessUploadedImageAsync(fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process uploaded image {fileName}", fileName);
            }
        });
    }

    public async Task ProcessUploadedImageAsync(string fileName)
    {
        try
        {
            _logger.LogInformation("Processing uploaded image: {fileName}", fileName);

            var details = await _fileRepository.GetImageDetailFromStorage(fileName);
            
            _logger.LogInformation("Image details: {fileName}, Extension: {extension}, Size: {contentLength} bytes", 
                details.FileName, details.FileExtension, details.ContentLength);

            // Step 1: Convert HEIC/HEIF to JPEG if needed (replaces original in storage)
            var finalFileName = await ConvertToJpegIfNeeded(details);

            // Step 2: Create thumbnail from the (now JPEG) image
            await CreateThumbnail(finalFileName);
            
            _logger.LogInformation("Successfully processed image: {finalFileName}", finalFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception processing image: {fileName}", fileName);
            throw;
        }
    }

    private async Task<string> ConvertToJpegIfNeeded(ImageDetail details)
    {
        var extension = details.FileExtension.ToLowerInvariant();
        
        // Only convert HEIC/HEIF files
        if (extension != ".heic" && extension != ".heif")
        {
            _logger.LogDebug("{fileName} does not need conversion ({extension})", 
                details.FileName, extension);
            return details.FileName;
        }

        _logger.LogInformation("Converting {fileName} from {extension} to JPEG", 
            details.FileName, extension);

        try
        {
            // Download the HEIC file
            using var heicStream = await _fileRepository.DownloadImageFromStorage(details.FileName);
            
            // Convert to JPEG
            var (jpegStream, width, height, newFileName, contentType) = 
                await _conversionService.ConvertIfNeededAsync(heicStream, details.FileName, details.ContentType);

            using (jpegStream)
            {
                _logger.LogInformation("Converted {oldFile} to {newFile} ({width}x{height})", 
                    details.FileName, newFileName, width, height);

                // Upload JPEG with the new filename (e.g., .heic -> .jpg)
                await _fileRepository.UploadImageToStorage(jpegStream, contentType, newFileName);
                
                // Delete the original HEIC file if conversion was successful
                if (newFileName != details.FileName)
                {
                    await _fileRepository.DeleteImageFromStorage(details.FileName);
                    _logger.LogInformation("Deleted original HEIC file: {fileName}", details.FileName);
                }
            }

            return newFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert {fileName}, will use original", details.FileName);
            return details.FileName; // Fall back to original on error
        }
    }

    private async Task CreateThumbnail(string fileName)
    {
        _logger.LogInformation("Creating thumbnail for {fileName}", fileName);

        // Download the (now JPEG) image
        using var imageStream = await _fileRepository.DownloadImageFromStorage(fileName);
        
        // Create thumbnail using ImageSharp
        var (thumbnailStream, width, height) = await _thumbnailProcessor.ResizeToJpegAsync(
            imageStream, 
            _thumbnailWidth, 
            quality: 75);

        using (thumbnailStream)
        {
            _logger.LogInformation("Created thumbnail for {fileName}: {width}x{height}", 
                fileName, width, height);

            // Upload thumbnail
            await _fileRepository.UploadThumbnailToStorage(thumbnailStream, "image/jpeg", fileName);
        }
    }
}
