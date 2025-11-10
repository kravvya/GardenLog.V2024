using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Text.RegularExpressions;
using Sharp = SixLabors.ImageSharp;

namespace ImageCatalog.Api.Services;

public interface IFileCommandHandler
{
    string GenerateSasToken(string fileName);
    void ResizeImageToThumbnail(string fileName);
    Task ResizeImageToThumbnailAsync(string fileName);
}

public class FileCommandHandler : IFileCommandHandler
{
    readonly IFileRepository _fileRepository;
    private readonly ILogger<FileCommandHandler> _logger;
    private readonly int _thumbnailWidth;

    public FileCommandHandler(IFileRepository fileRepository, ILogger<FileCommandHandler> logger, IConfiguration configuration)
    {
        _fileRepository = fileRepository;
        _logger = logger;
        _thumbnailWidth = configuration.GetValue<int>("ThumbnailWidth");
    }

    public string GenerateSasToken(string fileName) => _fileRepository.GenerateSasToken(fileName);

    public void ResizeImageToThumbnail(string fileName)
    {
        Task.Run(async () =>
        {
            try
            {
                await ResizeImageToThumbnailAsync(fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create thumbnail for {fileName}", fileName);
            }
        });
    }

    public async Task ResizeImageToThumbnailAsync(string fileName)
    {
        try
        {
            _logger.LogInformation("Resize image {fileName}", fileName);

            var details = await _fileRepository.GetImageDetailFromStorage(fileName);

            var encoder = GetEncoder(details.FileExtension);

            if (encoder != null)
            {
                _logger.LogInformation("Processed image Name:{fileName} Size:{contentLength} Bytes", details.FileName, details.ContentLength);

                using var output = new MemoryStream();
                using Sharp.Image originalImage = Sharp.Image.Load(await _fileRepository.DownloadImageFromStorage(details.FileName));

                var divisor = originalImage.Width / _thumbnailWidth;
                var height = Convert.ToInt32(Math.Round((decimal)(originalImage.Height / divisor)));

                originalImage.Mutate(x => x.Resize(_thumbnailWidth, height));
                originalImage.Save(output, encoder);
                output.Position = 0;
                await _fileRepository.UploadThumbnailToStorage(output, details.ContentType, details.FileName);
                
                _logger.LogInformation("Successfully created thumbnail for {fileName}", fileName);
            }
            else
            {
                _logger.LogWarning("No encoder support for: {fileName}", details.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception resizing image: {fileName}", fileName);
            throw;
        }
    }

    public static IImageEncoder? GetEncoder(string extension)
    {
        IImageEncoder? encoder = null;

        extension = extension.Replace(".", "");

        var isSupported = Regex.IsMatch(extension, "gif|png|jpg|jpeg|bmp|pbm|tga|tiff|webp", RegexOptions.IgnoreCase);

        if (isSupported)
        {
            switch (extension.ToLower())
            {
                case "png":
                    encoder = new PngEncoder();
                    break;
                case "jpg":
                    encoder = new JpegEncoder();
                    break;
                case "jpeg":
                    encoder = new JpegEncoder();
                    break;
                case "gif":
                    encoder = new GifEncoder();
                    break;
                case "bmp":
                    encoder = new BmpEncoder();
                    break;
                case "pbm":
                    encoder = new PbmEncoder();
                    break;
                case "tga":
                    encoder = new TgaEncoder();
                    break;
                case "tiff":
                    encoder = new TgaEncoder();
                    break;
                case "webp":
                    encoder = new WebpEncoder();
                    break;
                default:
                    break;
            }
        }

        return encoder;
    }
}
