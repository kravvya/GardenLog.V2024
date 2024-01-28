namespace ImageCatalog.Contract;


public static class ImageRoutes
{
    public const string ImageCatalogBase = "/v1/api/Images";
    public const string FileCatalogBase = "/v1/api/Files";

    public const string GenerateSasToken = FileCatalogBase + "/tokens/{fileName}";
    public const string ResizeImageToThumbnail = FileCatalogBase + "/{fileName}/thumbnail";

    public const string Search = ImageCatalogBase + "/search";
    public const string SearchBatch = ImageCatalogBase + "/search/batch";

    public const string CrerateImage = ImageCatalogBase;
    public const string UpdateImage = ImageCatalogBase + "/{imageId}";
    public const string DeleteImage = ImageCatalogBase + "/{imageId}";
}