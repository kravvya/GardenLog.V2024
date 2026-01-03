using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Sharp = SixLabors.ImageSharp;

namespace ImageCatalog.Api.Services;

/// <summary>
/// Image processor using SixLabors.ImageSharp library
/// Handles: PNG, JPEG, GIF, BMP, WebP, TIFF, TGA, QOI
/// </summary>
public class ImageSharpProcessor : IImageProcessor
{
    private readonly ILogger<ImageSharpProcessor> _logger;

    // Formats supported by ImageSharp (based on available decoders)
    private static readonly HashSet<string> SupportedFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg",  // JPEG
        ".png",           // PNG
        ".gif",           // GIF
        ".bmp",           // Bitmap
        ".webp",          // WebP
        ".tiff", ".tif",  // TIFF
        ".tga",           // Targa
        ".pbm", ".pgm", ".ppm", ".pnm"  // PBM family
    };

    public ImageSharpProcessor(ILogger<ImageSharpProcessor> logger)
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
            _logger.LogDebug("Converting image to JPEG with ImageSharp");

            using var originalImage = await Sharp.Image.LoadAsync(imageStream);
            
            var width = originalImage.Width;
            var height = originalImage.Height;

            _logger.LogDebug("Converting image {width}x{height} to JPEG at quality {quality}", 
                width, height, quality);

            var output = new MemoryStream();
            var jpegEncoder = new JpegEncoder { Quality = quality };
            await originalImage.SaveAsync(output, jpegEncoder);
            
            output.Position = 0;

            return (output, width, height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ImageSharp failed to convert image to JPEG");
            throw;
        }
    }

    public async Task<(Stream outputStream, int width, int height)> ResizeToJpegAsync(
        Stream imageStream, 
        int targetWidth, 
        int quality = 75)
    {
        try
        {
            _logger.LogDebug("Loading image with ImageSharp for thumbnail generation");

            using var originalImage = await Sharp.Image.LoadAsync(imageStream);
            
            var divisor = originalImage.Width / targetWidth;
            var height = Convert.ToInt32(Math.Round((decimal)(originalImage.Height / divisor)));

            _logger.LogDebug("Resizing image from {originalWidth}x{originalHeight} to {thumbnailWidth}x{thumbnailHeight}", 
                originalImage.Width, originalImage.Height, targetWidth, height);

            originalImage.Mutate(x => x.Resize(targetWidth, height));
            
            var output = new MemoryStream();
            var jpegEncoder = new JpegEncoder { Quality = quality };
            await originalImage.SaveAsync(output, jpegEncoder);
            
            output.Position = 0;

            return (output, targetWidth, height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ImageSharp failed to process image");
            throw;
        }
    }
}
