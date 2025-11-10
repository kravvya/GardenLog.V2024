namespace ImageCatalog.Api.Services;

/// <summary>
/// Service to handle image format conversions during upload
/// </summary>
public interface IImageConversionService
{
    /// <summary>
    /// Converts HEIC/HEIF images to JPEG, passes through other formats unchanged
    /// </summary>
    Task<(Stream stream, int width, int height, string newFileName, string contentType)> ConvertIfNeededAsync(
        Stream inputStream, 
        string fileName, 
        string contentType);
}

public class ImageConversionService : IImageConversionService
{
    private readonly IEnumerable<IImageProcessor> _imageProcessors;
    private readonly ILogger<ImageConversionService> _logger;

    private static readonly HashSet<string> FormatsToConvert = new(StringComparer.OrdinalIgnoreCase)
    {
        ".heic", ".heif"
    };

    public ImageConversionService(
        IEnumerable<IImageProcessor> imageProcessors,
        ILogger<ImageConversionService> logger)
    {
        _imageProcessors = imageProcessors;
        _logger = logger;
    }

    public async Task<(Stream stream, int width, int height, string newFileName, string contentType)> ConvertIfNeededAsync(
        Stream inputStream, 
        string fileName, 
        string contentType)
    {
        var extension = Path.GetExtension(fileName);

        // Check if this file needs conversion
        if (!FormatsToConvert.Contains(extension))
        {
            _logger.LogDebug("File {fileName} does not need conversion, extension: {extension}", 
                fileName, extension);
            
            // Return original stream (need to get dimensions though)
            inputStream.Position = 0;
            return (inputStream, 0, 0, fileName, contentType); // Dimensions unknown for non-converted files
        }

        _logger.LogInformation("Converting {fileName} from {extension} to JPEG", fileName, extension);

        // Find processor that can handle this format
        var processor = _imageProcessors.FirstOrDefault(p => p.CanProcess(extension));

        if (processor == null)
        {
            _logger.LogWarning("No processor found for {extension}, returning original", extension);
            inputStream.Position = 0;
            return (inputStream, 0, 0, fileName, contentType);
        }

        try
        {
            // Convert to JPEG (high quality for originals)
            var (outputStream, width, height) = await processor.ConvertToJpegAsync(inputStream, quality: 95);

            // Change file extension to .jpg
            var newFileName = Path.ChangeExtension(fileName, ".jpg");

            _logger.LogInformation("Successfully converted {originalFile} to {newFile} ({width}x{height})", 
                fileName, newFileName, width, height);

            return (outputStream, width, height, newFileName, "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert {fileName}, using original", fileName);
            
            // On error, return original
            inputStream.Position = 0;
            return (inputStream, 0, 0, fileName, contentType);
        }
    }
}
