using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace ImageCatalog.Api.Data;

public interface IFileRepository
{
    Task<Stream> DownloadImageFromStorage(string fileName);
    string GenerateSasToken(string fileName);
    Task<ImageDetail> GetImageDetailFromStorage(string fileName);
    Task UploadThumbnailToStorage(Stream content, string fileType, string fileName);
}

public class BlobRepository : IFileRepository
{
    private readonly ILogger<BlobRepository> _logger;
    private readonly IConfigurationService _configurationService;
    private readonly AzureBlobConfig _config;
    private readonly BlobContainerClient _imageContainer;
    private readonly BlobContainerClient _thumbnailContainer;

    public BlobRepository(IConfiguration configuration, ILogger<BlobRepository> logger, IConfigurationService configurationService)
    {
        _config = configuration.GetSection(AzureBlobConfig.Key).Get<AzureBlobConfig>()!;
        _logger = logger;
        _configurationService = configurationService;

        var connectionString = _configurationService.GetImageBlobConnectionString();

        var blobServiceClient = new BlobServiceClient(connectionString);

        _imageContainer = blobServiceClient.GetBlobContainerClient(_config.ImageContainer);
        _thumbnailContainer = blobServiceClient.GetBlobContainerClient(_config.ThumbnailContainer);

    }


    public string GenerateSasToken(string fileName)
    {
        var blob = _imageContainer.GetBlobClient(fileName);

        var sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = _imageContainer.Name,
            Resource = "c"
        };

        sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(5);
        sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

        Uri sasUri = blob.GenerateSasUri(sasBuilder);

        _logger.LogInformation("Generated SAS {sas}", sasUri);

        return sasUri.ToString();

    }

    public async Task UploadThumbnailToStorage(Stream content, string fileType, string fileName)
    {
        var blob = _thumbnailContainer.GetBlobClient(fileName);

        await blob.UploadAsync(content, new BlobHttpHeaders
        {
            ContentType = fileType
        },
            conditions: null);


    }

    public async Task<Stream> DownloadImageFromStorage(string fileName)
    {
        var blob = _imageContainer.GetBlobClient(fileName);
        var content = await blob.DownloadStreamingAsync();
        return content.Value.Content;
    }

    public async Task<ImageDetail> GetImageDetailFromStorage(string fileName)
    {
        var blob = _imageContainer.GetBlobClient(fileName);
        var content = await blob.GetPropertiesAsync();
        return new ImageDetail(fileName, content.Value.ContentType, content.Value.ContentLength, Path.GetExtension(fileName));
    }

}
