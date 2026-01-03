using ImageMagick;

namespace ImageCatalog.Api.Services;

/// <summary>
/// Image processor using ImageMagick.NET library
/// Handles: HEIC, HEIF conversion to JPEG
/// </summary>
public class ImageMagickProcessor : IImageProcessor
{
    private readonly ILogger<ImageMagickProcessor> _logger;

    // Formats that ImageMagick handles (primarily HEIC/HEIF from iPhone)
    private static readonly HashSet<string> SupportedFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        ".heic", ".heif"  // Apple HEIC/HEIF formats
    };

    public ImageMagickProcessor(ILogger<ImageMagickProcessor> logger)
    {
        _logger = logger;
    }

    public bool CanProcess(string fileExtension)
    {
        return SupportedFormats.Contains(fileExtension);
    }

    public async Task<(Stream outputStream, int width, int height)> ConvertToJpegAsync(
        Stream imageStream, 
        int quality = 95)
    {
        try
        {
            _logger.LogDebug("Converting HEIC/HEIF to JPEG with ImageMagick");

            // Load image from stream
            using var image = new MagickImage(imageStream);
            
            var originalWidth = (int)image.Width;
            var originalHeight = (int)image.Height;

            _logger.LogDebug("Converting image {originalWidth}x{originalHeight} to JPEG at quality {quality}", 
                originalWidth, originalHeight, quality);

            // Set output format to JPEG with high quality (no resizing)
            image.Format = MagickFormat.Jpeg;
            image.Quality = (uint)quality;

            // Write to output stream
            var output = new MemoryStream();
            await image.WriteAsync(output);
            output.Position = 0;

            _logger.LogInformation("Successfully converted HEIC/HEIF to JPEG: {width}x{height}", 
                originalWidth, originalHeight);

            return (output, originalWidth, originalHeight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ImageMagick failed to convert image to JPEG");
            throw;
        }
    }

    public async Task<(Stream outputStream, int width, int height)> ResizeToJpegAsync(
        Stream imageStream, 
        int targetWidth, 
        int quality = 75)
    {
        // This should not be called for HEIC files anymore
        // After conversion, ImageSharp handles thumbnail generation
        _logger.LogWarning("ResizeToJpegAsync called on ImageMagickProcessor - this should use ImageSharp instead");
        
        return await ConvertToJpegAsync(imageStream, quality);
    }
}
