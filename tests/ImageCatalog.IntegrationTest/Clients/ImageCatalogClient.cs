using GardenLog.SharedInfrastructure.Extensions;
using GardenLog.SharedKernel.Enum;
using GardenLog.SharedKernel;
using ImageCatalog.Contract;
using ImageCatalog.Contract.Commands;
using ImageCatalog.Contract.Queries;
using MongoDB.Driver;

namespace ImageCatalog.IntegrationTest.Clients
{
    public class ImageCatalogClient
    {
        private readonly Uri _baseUrl;
        private readonly HttpClient _httpClient;

        public ImageCatalogClient(Uri baseUrl, HttpClient httpClient)
        {
            _baseUrl = baseUrl;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("RequestUser", "86377291-980f-4af2-8608-39dbbf7e09e1");
        }

        #region Image
        public async Task<HttpResponseMessage> CreateImage(string name)
        {
            var url = $"{this._baseUrl.OriginalString}{ImageRoutes.CrerateImage}/";

            var createImageCommand = PopulateCreateImageCommand(name);

            using var requestContent = createImageCommand.ToJsonStringContent();

            return await this._httpClient.PostAsync(url, requestContent);

        }

        public async Task<HttpResponseMessage> UpdateImage(ImageViewModel image)
        {
            var url = $"{this._baseUrl.OriginalString}{ImageRoutes.UpdateImage}";

            using var requestContent = image.ToJsonStringContent();

            return await this._httpClient.PutAsync(url.Replace("{imageId}", image.ImageId), requestContent);

        }

        public async Task<HttpResponseMessage> DeleteIamge(string id)
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

        private static CreateImageCommand PopulateCreateImageCommand(string name)
        {
            var command =  new CreateImageCommand()
            {
                FileName = name,
                FileType = "*.test",
                ImageName = "TestFile",
                Label = "Test Label",
                RelatedEntityId = "TestEntity1",
                RelatedEntityType = RelatedEntityTypEnum.Plant
            };
            command.RelatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.Plant, "TestEntity1", string.Empty));
            return command;
        }
        
        #endregion

    }
}
