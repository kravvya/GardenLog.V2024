using GardenLog.SharedInfrastructure.Extensions;
using GardenLog.SharedKernel;
using GardenLog.SharedKernel.Enum;
using ImageCatalog.Contract;
using ImageCatalog.Contract.Commands;
using ImageCatalog.Contract.Queries;

namespace ImageCatalog.IntegrationTest.Clients
{
    public class FileCatalogClient
    {
        private readonly Uri _baseUrl;
        private readonly HttpClient _httpClient;

        public FileCatalogClient(Uri baseUrl, HttpClient httpClient)
        {
            _baseUrl = baseUrl;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("RequestUser", "86377291-980f-4af2-8608-39dbbf7e09e1");
        }

        #region Image
        public async Task<HttpResponseMessage> GenerateSasUri(string fileName)
        {
            var url = $"{this._baseUrl.OriginalString}{ImageRoutes.GenerateSasToken}";

            return await this._httpClient.GetAsync(url.Replace("{fileName}", fileName));
        }

        public async Task<HttpResponseMessage> ResizeImageToThumbnail(string fileName)
        {
            var url = $"{this._baseUrl.OriginalString}{ImageRoutes.ResizeImageToThumbnail}";

            return await this._httpClient.GetAsync(url.Replace("{fileName}", fileName));

        }

        public async Task<HttpResponseMessage> DeletePLant(string id)
        {
            var url = $"{this._baseUrl.OriginalString}{ImageRoutes.DeleteImage}";

            return await this._httpClient.DeleteAsync(url.Replace("{imageId}", id));
        }

        public async Task<HttpResponseMessage> SearchImages(GetImagesByRelatedEntity searchQuery)
        {
            var url = $"{this._baseUrl.OriginalString}{ImageRoutes.Search}";

            using var requestContent = searchQuery.ToJsonStringContent();

            return await this._httpClient.PostAsync(url, requestContent);
        }

        public async Task<HttpResponseMessage> SearchImages(GetImagesByRelatedEntities searchQuery)
        {
            var url = $"{this._baseUrl.OriginalString}{ImageRoutes.SearchBatch}";

            using var requestContent = searchQuery.ToJsonStringContent();

            return await this._httpClient.PostAsync(url, requestContent);
        }

        #endregion

    }
}
