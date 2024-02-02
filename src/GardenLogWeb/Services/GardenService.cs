

using ImageCatalog.Contract.Queries;

namespace GardenLogWeb.Services
{
    public interface IGardenService
    {
        Task<List<GardenModel>> GetGardens(bool forceRefresh);
        Task<GardenModel?> GetGarden(string gardenId, bool useCache);
        Task<ApiObjectResponse<string>> CreateGarden(GardenModel garden);
        Task<ApiResponse> UpdateGarden(GardenModel garden);
        Task<ApiResponse> DeleteGarden(string gardenId);
        Task<List<GardenBedModel>> GetGardenBeds(string gardenId, bool useCache);
        Task<ApiResponse> DeleteGardenBed(string gardenId, string id);
        Task<ApiObjectResponse<string>> CreateGardenBed(GardenBedModel gardenBed);
        Task<ApiResponse> UpdateGardenBed(GardenBedModel gardenBed);
    }

    public class GardenService : IGardenService
    {
        private readonly ILogger<GardenService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICacheService _cacheService;
        private readonly IGardenLogToastService _toastService;
        private readonly IImageService _imageService;

        private readonly int _cacheDuration = 10;
        private const string GARDENS_KEY = "Gardens";
        private const string GARDEN_BED_KEY = "Garden_{0}_Bed";

        public GardenService(ILogger<GardenService> logger, IHttpClientFactory clientFactory, ICacheService cacheService, IGardenLogToastService toastService, IImageService imageService, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = clientFactory;
            _cacheService = cacheService;
            _toastService = toastService;
            _imageService = imageService;
            if (!int.TryParse(configuration[GlobalConstants.GLOBAL_CACHE_DURATION], out _cacheDuration)) _cacheDuration = 10;
        }

        #region Garden Functions

        public async Task<List<GardenModel>> GetGardens(bool forceRefresh)
        {
            List<GardenModel>? gardens;

            if (forceRefresh || !_cacheService.TryGetValue<List<GardenModel>>(GARDENS_KEY, out gardens))
            {
                _logger.LogDebug("Gardens not in cache or forceRefresh");

                var gardenTast = GetAllGardens();
                var imagesTask = _imageService.GetImages(RelatedEntityTypEnum.Plant, false);

                await Task.WhenAll(gardenTast, imagesTask);

                gardens = gardenTast.Result;
                var images = imagesTask.Result;

                if (gardens.Count > 0)
                {

                    foreach (var garden in gardens)
                    {
                        garden.Images = images.Where(i => i.RelatedEntityId == garden.GardenId).ToList();

                        var image = garden.Images.FirstOrDefault();
                        if (image != null)
                        {
                            garden.ImageFileName = image.FileName;
                            garden.ImageLabel = image.Label;
                        }
                        else
                        {
                            garden.ImageFileName = ImageService.NO_IMAGE;
                            garden.ImageLabel = "Add image";
                        }
                    }

                    // Save data in cache.
                    _cacheService.Set(GARDENS_KEY, gardens, DateTime.Now.AddMinutes(_cacheDuration));
                }

            }
            else
            {
                _logger.LogDebug($"Gardens are in cache. Found {gardens!.Count}");
            }

            return gardens;
        }

        public async Task<GardenModel?> GetGarden(string gardenId, bool useCache)
        {
            GardenModel? garden = null;

            var plants = (await GetGardens(false));

            if (useCache)
            {
                garden = plants.FirstOrDefault(p => p.GardenId == gardenId);
            }

            if (garden == null)
            {
                var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

                var response = await httpClient.ApiGetAsync<GardenModel>(GardenRoutes.GetGarden.Replace("{gardenId}", gardenId), _logger);

                if (!response.IsSuccess)
                {
                    _toastService.ShowToast("Unable to get Garden details", GardenLogToastLevel.Error);
                    return null;
                }

                garden = response.Response;

                if (garden != null) await AddOrUpdateToGardenList(garden);
            }

            return garden;
        }

        public async Task<ApiObjectResponse<string>> CreateGarden(GardenModel garden)
        {
            var httpClient = _httpClientFactory.CreateClient(GlobalConstants.USERMANAGEMENT_API);

            var response = await httpClient.ApiPostAsync(GardenRoutes.CreateGarden, garden);

            if (response.ValidationProblems != null)
            {
                _toastService.ShowToast($"Unable to create a Garden. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
            }
            else if (!response.IsSuccess)
            {
                _toastService.ShowToast($"Received an invalid response from Garden post: {response.ErrorMessage}", GardenLogToastLevel.Error);
            }
            else
            {
                garden.GardenId = response.Response!;
                garden.Images = new();
                garden.ImageFileName = ImageService.NO_IMAGE;
                garden.ImageLabel = string.Empty;

                await AddOrUpdateToGardenList(garden);

                _toastService.ShowToast($"Garden saved", GardenLogToastLevel.Success);
            }

            return response;
        }

        public async Task<ApiResponse> UpdateGarden(GardenModel harvest)
        {
            var httpClient = _httpClientFactory.CreateClient(GlobalConstants.USERMANAGEMENT_API);

            var response = await httpClient.ApiPutAsync(GardenRoutes.UpdateGarden.Replace("{gardenId}", harvest.GardenId), harvest);

            if (response.ValidationProblems != null)
            {
                _toastService.ShowToast($"Unable to update Garden. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
            }
            else if (!response.IsSuccess)
            {
                _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
            }
            else
            {
                await AddOrUpdateToGardenList(harvest);

                _toastService.ShowToast($"Garden changes successfully saved.", GardenLogToastLevel.Success);
            }

            return response;
        }

        public async Task<ApiResponse> DeleteGarden(string gardenId)
        {
            var httpClient = _httpClientFactory.CreateClient(GlobalConstants.USERMANAGEMENT_API);

            var response = await httpClient.ApiDeleteAsync(GardenRoutes.DeleteGarden.Replace("{gardenId}", gardenId));

            if (response.ValidationProblems != null)
            {
                _toastService.ShowToast($"Unable to delete a Garden. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
            }
            else if (!response.IsSuccess)
            {
                _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
            }
            else
            {
                RemoveFromGardenList(gardenId);

                _toastService.ShowToast($"Garden deleted.", GardenLogToastLevel.Success);
            }
            return response;

        }

        #endregion

        #region Garden Bed Functions
        public async Task<List<GardenBedModel>> GetGardenBeds(string gardenId, bool useCache)
        {
            List<GardenBedModel>? gardenBedList = null;

            if (useCache)
            {
                gardenBedList = GetGardenBedsFromCache(gardenId);
            }

            if (gardenBedList == null)
            {
                gardenBedList = await GetGardenBeds(gardenId);

                if (gardenBedList.Count > 0)
                {
                    AddGardenBedsToCache(gardenId, gardenBedList);
                }
            }

            return gardenBedList;
        }

        public async Task<ApiObjectResponse<string>> CreateGardenBed(GardenBedModel gardenBed)
        {
            var httpClient = _httpClientFactory.CreateClient(GlobalConstants.USERMANAGEMENT_API);

            var response = await httpClient.ApiPostAsync(GardenRoutes.CreateGardenBed, gardenBed);

            if (response.ValidationProblems != null)
            {
                _toastService.ShowToast($"Unable to add Garden Bed to Garden. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
            }
            else if (!response.IsSuccess)
            {
                _toastService.ShowToast($"Received an invalid response from Garden Bed  Post: {response.ErrorMessage}", GardenLogToastLevel.Error);
            }
            else
            {
                gardenBed.GardenBedId = response.Response!;


                AddOrUpdateToGardenBedList(gardenBed);

                _toastService.ShowToast($"Garden Bed  is added to the Garden ", GardenLogToastLevel.Success);
            }

            return response;
        }

        public async Task<ApiResponse> UpdateGardenBed(GardenBedModel gardenBed)
        {
            var httpClient = _httpClientFactory.CreateClient(GlobalConstants.USERMANAGEMENT_API);

            var response = await httpClient.ApiPutAsync(GardenRoutes.UpdateGardenBed.Replace("{gardenId}", gardenBed.GardenId).Replace("{id}", gardenBed.GardenBedId), gardenBed);

            if (response.ValidationProblems != null)
            {
                _toastService.ShowToast($"Unable to update a Garden Bed. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
            }
            else if (!response.IsSuccess)
            {
                _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
            }
            else
            {
                AddOrUpdateToGardenBedList(gardenBed);

                _toastService.ShowToast($"Garden Bed is successfully updated.", GardenLogToastLevel.Success);
            }

            return response;
        }

        public async Task<ApiResponse> DeleteGardenBed(string gardenId, string id)
        {
            var httpClient = _httpClientFactory.CreateClient(GlobalConstants.USERMANAGEMENT_API);

            var response = await httpClient.ApiDeleteAsync(GardenRoutes.DeleteGardenBed.Replace("{gardenId}", gardenId).Replace("{id}", id));

            if (response.ValidationProblems != null)
            {
                _toastService.ShowToast($"Unable to delete garden bed. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
            }
            else if (!response.IsSuccess)
            {
                _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
            }
            else
            {
                RemoveFromGardenBedList(gardenId, id);

                _toastService.ShowToast($"Garden Bed deleted.", GardenLogToastLevel.Success);
            }
            return response;
        }
        #endregion

        #region Private Garden functions
        //private List<GardenViewModel> GetAllGardens()
        //{
        //    return new List<GardenViewModel>() { new GardenViewModel(){
        //        GardenId = "garden1",
        //        Name = "Kravchenko's Garden",
        //        City="Minnetrista",
        //        StateCode = "MN",
        //        UserProfileId="up1",
        //        Latitude=44.97092m,
        //        Longitude=93.66728m

        //    } };
        //}
        //public Garden GetGarden()
        //{
        //    Garden garden = new Garden();
        //    garden.GardenName = "Steve's Garden";
        //    garden.BorderColor = "#585858";
        //    garden.Length = 180;
        //    garden.Width = 240;

        //    return garden;
        //}
        private async Task<List<GardenModel>> GetAllGardens()
        {
            var httpClient = _httpClientFactory.CreateClient(GlobalConstants.USERMANAGEMENT_API);

            var url = GardenRoutes.GetGardens;

            var response = await httpClient.ApiGetAsync<List<GardenModel>>(url, _logger);

            if (!response.IsSuccess)
            {
                _toastService.ShowToast("Unable to get Gardens", GardenLogToastLevel.Error);
                return new List<GardenModel>();
            }

            if (response.Response == null) return new List<GardenModel>();

            return response.Response;
        }

        #endregion

        private async Task AddOrUpdateToGardenList(GardenModel garden)
        {
            List<GardenModel>? gardens;

            if (_cacheService.TryGetValue<List<GardenModel>>(GARDENS_KEY, out gardens))
            {
                var index = gardens!.FindIndex(p => p.GardenId == garden.GardenId);
                if (index > -1)
                {
                    gardens[index] = garden;
                }
                else
                {
                    gardens.Add(garden);
                }
            }
            else
            {
                await GetGardens(true);
            }
        }

        private void RemoveFromGardenList(string gardenId)
        {
            List<GardenModel>? gardens;

            if (!_cacheService.TryGetValue<List<GardenModel>>(GARDENS_KEY, out gardens))
            {

                var index = gardens!.FindIndex(p => p.GardenId == gardenId);
                if (index > -1)
                {
                    gardens.RemoveAt(index);
                }
            }

        }



        #region Private Garden Bed
        private List<GardenBedModel>? GetGardenBedsFromCache(string gardenId)
        {
            string cacheKey = string.Format(GARDEN_BED_KEY, gardenId);

            List<GardenBedModel>? gardenBedList;

            _cacheService.TryGetValue<List<GardenBedModel>>(cacheKey, out gardenBedList);

            return gardenBedList;
        }

        private async Task<List<GardenBedModel>> GetGardenBeds(string gardenId)
        {
            var httpClient = _httpClientFactory.CreateClient(GlobalConstants.USERMANAGEMENT_API);

            var response = await httpClient.ApiGetAsync<List<GardenBedModel>>(GardenRoutes.GetGardenBeds.Replace("{gardenId}", gardenId), _logger);

            if (!response.IsSuccess)
            {
                _toastService.ShowToast("Unable to get Garden Beds", GardenLogToastLevel.Error);
                return new List<GardenBedModel>();
            }

            return response.Response!;

            //List<GardenBedModel> gardenBeds = new();
            //gardenBeds.Add(new GardenBedModel()
            //{
            //    BorderColor = "red",
            //    X = 100,
            //    Y = 50,
            //    Name = "Leeks",
            //    Length = 10 * 12,
            //    Width = 30
            //});

            //gardenBeds.Add(new GardenBedModel()
            //{
            //    BorderColor = "blue",
            //    X = 500,
            //    Y = 50,
            //    Name = "Leeks",
            //    Length = 10 * 12,
            //    Width = 30
            //});

            //return Task.FromResult(gardenBeds);
        }

        private void AddGardenBedsToCache(string gardenId, List<GardenBedModel> gardenBedList)
        {
            string cacheKey = string.Format(GARDEN_BED_KEY, gardenId);

            _cacheService.Set(cacheKey, gardenBedList, DateTime.Now.AddMinutes(10));
        }

        private void AddOrUpdateToGardenBedList(GardenBedModel gardenBed)
        {
            List<GardenBedModel>? beds = null;
            string key = string.Format(GARDEN_BED_KEY, gardenBed.GardenId);

            if (_cacheService.TryGetValue<List<GardenBedModel>>(key, out beds))
            {
                var index = beds!.FindIndex(p => p.GardenBedId == gardenBed.GardenBedId);
                if (index > -1)
                {
                    beds[index] = gardenBed;
                    return;
                }
                beds.Add(gardenBed);
            }


        }
        private void RemoveFromGardenBedList(string gardenId, string id)
        {
            string key = string.Format(GARDEN_BED_KEY, gardenId);
            if (_cacheService.TryGetValue<List<GardenBedModel>>(key, out var beds))
            {
                var index = beds!.FindIndex(p => p.GardenBedId == id);
                if (index > -1)
                {
                    beds.RemoveAt(index);
                }
            }
        }
        #endregion

    }
}
