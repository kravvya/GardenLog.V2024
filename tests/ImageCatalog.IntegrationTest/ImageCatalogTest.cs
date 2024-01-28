using GardenLog.SharedKernel.Enum;
using ImageCatalog.Contract.Queries;

namespace ImageCatalog.IntegrationTest
{
    public class ImageCatalogTest : IClassFixture<ImageCatalogServiceFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly ImageCatalogClient _imageClient;
        private readonly FileCatalogClient _fileClient;
        private const string TEST_FILE_NAME = "TestFile.Test";
        private const string TEST_RELATED_ENTITY_ID = "TestEntity1";

        public ImageCatalogTest(ImageCatalogServiceFixture fixture, ITestOutputHelper output)
        {
            _imageClient = fixture.ImageCatalogClient;
            _fileClient = fixture.FileCatalogClient;

            _output = output;
            _output.WriteLine($"Service id {fixture.FixtureId} @ {DateTime.Now:F}");

        }

        #region File
        [Fact]
        public async void File_GenerateSas_Should()
        {
            var response = await _fileClient.GenerateSasUri(TEST_FILE_NAME);

            var returnString = await response.Content.ReadAsStringAsync();

            _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
            Assert.NotEmpty(returnString);
        }

        #endregion

        #region Image

        [Fact]
        public async Task Post_Image_CeateNewImage_Should()
        {
            var response = await _imageClient.CreateImage(TEST_FILE_NAME);

            var returnString = await response.Content.ReadAsStringAsync();

            _output.WriteLine($"Service to create image responded with {response.StatusCode} code and {returnString} message");

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
            Assert.NotEmpty(returnString);
        }

        [Fact]
        public async Task Put_Image_UpdateLable_Should()
        {
            ImageViewModel? image = await GetFileToWorkWith();
            image.Label = "Last Updated " + DateTime.Now.ToLongTimeString();

            var response = await _imageClient.UpdateImage(image);

            var returnString = await response.Content.ReadAsStringAsync();

            _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        }

       

        [Fact]
        public async Task Delete_Image_Should()
        {
            ImageViewModel? image = await GetFileToWorkWith();

            var response = await _imageClient.DeleteIamge(image.ImageId);

            _output.WriteLine($"Service responded with {response.StatusCode} code");

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Search_Should_Return_Test_Plant_Images()
        {
            GetImagesByRelatedEntity search = new(RelatedEntityTypEnum.Plant, TEST_RELATED_ENTITY_ID, false);

            List<ImageViewModel>? images = await RunImageSearch(search);

            if(images == null || images.Count == 0)
            {
                await _imageClient.CreateImage(TEST_FILE_NAME);
            }

            images = await RunImageSearch(search);

            Assert.NotNull(images);
            Assert.NotEmpty(images);

            var testImage = images.FirstOrDefault(image => image.RelatedEntityId == TEST_RELATED_ENTITY_ID);

            Assert.NotNull(testImage);
        }



        [Fact]
        public async Task Search_Should_Return_All_Plant_Images()
        {
            GetImagesByRelatedEntity search = new(RelatedEntityTypEnum.Plant, string.Empty, false);

            List<ImageViewModel>? images = await RunImageSearch(search);

            if (images == null || images.Count == 0)
            {
                await _imageClient.CreateImage(TEST_FILE_NAME);
            }

            images = await RunImageSearch(search);

            Assert.NotNull(images);
            Assert.NotEmpty(images);

        }
        #endregion

        private async Task<ImageViewModel> GetFileToWorkWith()
        {
            GetImagesByRelatedEntity search = new(RelatedEntityTypEnum.Plant, TEST_RELATED_ENTITY_ID, false);

            List<ImageViewModel>? images = await RunImageSearch(search);

            if (images == null || images.Count == 0)
            {
                await _imageClient.CreateImage(TEST_FILE_NAME);
                images = await RunImageSearch(search);
            }

            var image = images.FirstOrDefault();
            return image!;
        }

        private async Task<List<ImageViewModel>> RunImageSearch(GetImagesByRelatedEntity search)
        {
            var response = await _imageClient.SearchImages(search);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new JsonStringEnumConverter(),
                },
            };

            var returnString = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Service responded with {response.StatusCode} code and {returnString} message");

            var images = await response.Content.ReadFromJsonAsync<List<ImageViewModel>>(options);
            return images!;
        }
    }
}