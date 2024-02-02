using Azure.Storage.Blobs;
using ImageCatalog.Contract.Queries;
using Microsoft.AspNetCore.Components.Forms;
using Image = ImageCatalog.Contract;

namespace GardenLogWeb.Services;

public interface IImageService
{
    Task<ApiObjectResponse<string>> CreateImage(ImageViewModel image);
    Task<List<ImageViewModel>> GetImages(RelatedEntityTypEnum entityType, string entityId, bool FilterUserOnly);
    Task<List<ImageViewModel>> GetImages(RelatedEntityTypEnum entityType, bool FilterUserOnly);
    Task<List<ImageViewModel>> GetImagesInBulk(List<GetImagesByRelatedEntity> entities);
    string GetRawImageUrl(string fileName);
    string GetThumbnailImageUrl(string fileName);
    Task UploadFile(IBrowserFile file, Action<long> progress, string fileName);
}

public class ImageService : IImageService
{
    private const string ThumbnailImageUrl = "https://glimages.blob.core.windows.net/thumbnails/";
    private const string RawImageUrl = "https://glimages.blob.core.windows.net/images/";

    public const string NO_IMAGE = "/images/noimage.png";

    private readonly ILogger<ImageService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IGardenLogToastService _toastService;

    public ImageService(ILogger<ImageService> logger, IHttpClientFactory clientFactory, IGardenLogToastService toastService)
    {
        _logger = logger;
        _httpClientFactory = clientFactory;
        _toastService = toastService;
    }
    public async Task<List<ImageViewModel>> GetImages(RelatedEntityTypEnum entityType, bool FilterUserOnly)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.IMAGEPLANTCATALOG_API);

        GetImagesByRelatedEntity query = new(entityType, null, FilterUserOnly);

        var response = await httpClient.ApiPostAsync<List<ImageViewModel>>(Image.ImageRoutes.Search, query);

        return response.Response!;
    }

    public async Task<List<ImageViewModel>> GetImages(RelatedEntityTypEnum entityType, string entityId, bool FilterUserOnly)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.IMAGEPLANTCATALOG_API);

        GetImagesByRelatedEntity query = new(entityType, entityId, FilterUserOnly);

        var response = await httpClient.ApiPostAsync<List<ImageViewModel>>(Image.ImageRoutes.Search, query);

        return response.Response!;
    }

    public async Task<List<ImageViewModel>> GetImagesInBulk(List<GetImagesByRelatedEntity> entities)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.IMAGEPLANTCATALOG_API);
        var query = new GetImagesByRelatedEntities() { Requests = entities };

        var response = await httpClient.ApiPostAsync<List<ImageViewModel>>(Image.ImageRoutes.SearchBatch, query);

        return response.Response!;

    }

    public async Task<ApiObjectResponse<string>> CreateImage(ImageViewModel image)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.IMAGEPLANTCATALOG_API);
              
        var response = await httpClient.ApiPostAsync(Image.ImageRoutes.CrerateImage, image);


        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to create a Plant. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response from Plant Post: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            image.ImageId = response.Response!;

            _toastService.ShowToast($"Image uploaded. Image id is {image.ImageId}", GardenLogToastLevel.Success);
        }


        return response;
    }

    public async Task UploadFile(IBrowserFile file, Action<long> progressReport, string fileName)
    {
        int maxAllowedSize = 10 * 1024 * 1024;

        var token = await GetSasToken(fileName);

        var blobClient = new BlobServiceClient(new Uri(token.Replace("\"", "")));
        var container = blobClient.GetBlobContainerClient("images");
        var blob = container.GetBlobClient(fileName);

        await blob.UploadAsync(file.OpenReadStream(maxAllowedSize)
            , new Azure.Storage.Blobs.Models.BlobUploadOptions()
            {
                ProgressHandler = new Progress<long>((progress) =>
                {
                    _logger.LogInformation("Uploading {progress}", progress);
                    progressReport(progress);
                })
            });

        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.IMAGEPLANTCATALOG_API);

        var response = await httpClient.GetAsync(Image.ImageRoutes.ResizeImageToThumbnail.Replace("{fileName}", fileName));
    }

    public string GetThumbnailImageUrl(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return $"./{ImageService.NO_IMAGE}";
        }

        if (fileName.Equals(ImageService.NO_IMAGE))
        {
            return $"./{fileName}";
        }
        return $"{ThumbnailImageUrl}{fileName}";
    }

    public string GetRawImageUrl(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return  $"./{ImageService.NO_IMAGE}";
        }

        if (fileName.Equals(ImageService.NO_IMAGE))
        {
            return $"./{fileName}";
        }
        return $"{RawImageUrl}{fileName}";
    }

    private async Task<string> GetSasToken(string fileName)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.IMAGEPLANTCATALOG_API);
        var httpResponseMessage = await httpClient.GetAsync(Image.ImageRoutes.GenerateSasToken.Replace("{fileName}", fileName));

        httpResponseMessage.EnsureSuccessStatusCode();

        return await httpResponseMessage.Content.ReadAsStringAsync();

    }
}
