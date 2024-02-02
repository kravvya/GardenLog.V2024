using System.Net.Http;
using System;
using UserManagement.Contract.Base;
using GardenLog.SharedKernel;

namespace GardenLogWeb.Services;

public interface IHarvestCycleService
{
    Task<IList<HarvestCycleModel>> GetHarvestList(bool forceRefresh);
    Task<HarvestCycleModel> GetHarvest(string harvestCycleId, bool useCache);
    Task<ApiObjectResponse<string>> CreateHarvest(HarvestCycleModel harvest);
    Task<ApiResponse> UpdateHarvest(HarvestCycleModel harvest);
    Task<ApiResponse> DeleteHarvest(string id);
    Task<List<PlantHarvestCycleModel>> GetPlantHarvests(string harvestCycleId, bool forceRefresh);
    Task<List<PlantHarvestCycleIdentityOnlyViewModel>> GetPlantHarvestsByPLantId(string plantId);
    Task<ApiObjectResponse<string>> CreatePlantHarvest(PlantHarvestCycleModel plant);
    Task<ApiResponse> UpdatePlantHarvest(PlantHarvestCycleModel plant);
    Task<ApiResponse> DeletePlantHarvest(string harvestId, string id);
    Task<PlantHarvestCycleModel?> GetPlantHarvest(string harvestCycleId, string plantHarvestCycleId, bool forceRefresh);
    Task<ApiObjectResponse<string>> CreateGardenBedPlantHarvestCycle(GardenBedPlantHarvestCycleModel gardenBedPlant);
    Task<ApiResponse> UpdateGardenBedPlantHarvestCycle(GardenBedPlantHarvestCycleModel gardenBedPlant);
    Task<ApiResponse> DeleteGardenBedPlantHarvestCycle(string harvestId, string plantHarvestId, string id);
    Task<List<RelatedEntity>> BuildRelatedEntities(RelatedEntityTypEnum entityType, string entityId, string harvestCycleId);
    Task<HarvestCycleModel?> GetActiveHarvestCycle();
}

public class HarvestCycleService : IHarvestCycleService
{
    private readonly ILogger<PlantService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICacheService _cacheService;
    private readonly IGardenLogToastService _toastService;
    private readonly IImageService _imageService;
    private readonly IGardenService _gardenService;
    private readonly int _cacheDuration;
    private const string HARVESTS_KEY = "HarvestCycles";
    private const string PLANT_HARVESTS_KEY = "PlantHarvestCycles_{0}";

    public HarvestCycleService(ILogger<PlantService> logger, IHttpClientFactory clientFactory, ICacheService cacheService, IGardenLogToastService toastService, IImageService imageService, IGardenService gardenService, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = clientFactory;
        _cacheService = cacheService;
        _toastService = toastService;
        _imageService = imageService;
        _gardenService = gardenService;
        if (!int.TryParse(configuration[GlobalConstants.GLOBAL_CACHE_DURATION], out _cacheDuration)) _cacheDuration = 10;
    }

    #region Public Harvest Cycle Functions

    public async Task<HarvestCycleModel?> GetActiveHarvestCycle()
    {
        var harvests = await GetHarvestList(false);
        if (harvests != null && harvests.Count > 0)
        {
            var harvest = harvests.OrderByDescending(h => h.StartDate).FirstOrDefault(h => h.EndDate == null);
            return harvest ?? null;
        }

        return null;
    }

    public async Task<IList<HarvestCycleModel>> GetHarvestList(bool forceRefresh)
    {

        if (forceRefresh || !_cacheService.TryGetValue<IList<HarvestCycleModel>>(HARVESTS_KEY, out IList<HarvestCycleModel>? harvests))
        {
            _logger.LogInformation("Harvests not in cache or forceRefresh");

            var harvestTask = GetAllHarvests();
            var gardenTask = _gardenService.GetGardens(false);

            await Task.WhenAll(harvestTask, gardenTask);

            harvests = harvestTask.Result;
            var gardens = gardenTask.Result;

            if (harvests.Count > 0)
            {
                foreach (var harvest in harvests)
                {
                    var garden = gardens.FirstOrDefault(g => g.GardenId == harvest.GardenId);
                    if (garden != null)
                    {
                        harvest.GardenName = garden.Name;
                        harvest.CanShowLayout = garden.Length > 0 && garden.Width > 0;
                    }
                }
                // Save data in cache.
                _cacheService.Set(HARVESTS_KEY, harvests, DateTime.Now.AddMinutes(_cacheDuration));
            }
        }
        else
        {
            _logger.LogInformation($"Harvests are in cache. Found {harvests!.Count}");
        }

        return harvests;
    }

    public async Task<HarvestCycleModel> GetHarvest(string harvestCycleId, bool useCache)
    {
        HarvestCycleModel? harvest = null;


        if (useCache)
        {
            if (_cacheService.TryGetValue<IList<HarvestCycleModel>>(HARVESTS_KEY, out var harvests))
            {
                harvest = harvests?.FirstOrDefault(p => p.HarvestCycleId == harvestCycleId);
            }
        }

        if (harvest == null)
        {
            var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

            var response = await httpClient.ApiGetAsync<HarvestCycleModel>(HarvestRoutes.GetHarvestCycleById.Replace("{id}", harvestCycleId), _logger);

            if (!response.IsSuccess)
            {
                _toastService.ShowToast("Unable to get Harvest Cycle details", GardenLogToastLevel.Error);
                return new HarvestCycleModel();
            }

            harvest = response.Response;
            harvest!.GardenName = (await _gardenService.GetGarden(harvest.GardenId, true))?.Name;

            AddOrUpdateToHarvestCycleList(harvest);
        }

        return harvest;
    }

    public async Task<ApiObjectResponse<string>> CreateHarvest(HarvestCycleModel harvest)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiPostAsync(HarvestRoutes.CreateHarvestCycle, harvest);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to create a Garden Plan. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response from Garden Plan Post: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            harvest.HarvestCycleId = response.Response!;

            AddOrUpdateToHarvestCycleList(harvest);

            _toastService.ShowToast($"Garden Plan {harvest.HarvestCycleName} is created", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> UpdateHarvest(HarvestCycleModel harvest)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiPutAsync(HarvestRoutes.UpdateHarvestCycle.Replace("{id}", harvest.HarvestCycleId), harvest);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to update a Garden Plan. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            AddOrUpdateToHarvestCycleList(harvest);

            _toastService.ShowToast($"{harvest.HarvestCycleName} is successfully updated.", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> DeleteHarvest(string id)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiDeleteAsync(HarvestRoutes.DeleteHarvestCycle.Replace("{id}", id));

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to delete a Garden Plan. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            RemoveFromHarvestCycleList(id);

            _toastService.ShowToast($"Garden Plan deleted.", GardenLogToastLevel.Success);
        }
        return response;
    }

    public async Task<List<RelatedEntity>> BuildRelatedEntities(RelatedEntityTypEnum entityType, string entityId, string harvestCycleId)
    {
        List<RelatedEntity> relatedEntities = new();

        switch (entityType)
        {
            case RelatedEntityTypEnum.HarvestCycle:
                var harvest = await GetHarvest(entityId, true);
                relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.HarvestCycle, entityId, harvest.HarvestCycleName));
                break;
            case RelatedEntityTypEnum.PlantHarvestCycle:
                var plantHarvest = await GetPlantHarvest(harvestCycleId, entityId, false);
                harvest = await GetHarvest(harvestCycleId, true);

                relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.HarvestCycle, harvest.HarvestCycleId, harvest.HarvestCycleName));
                if (plantHarvest != null) relatedEntities.Add(new RelatedEntity(RelatedEntityTypEnum.PlantHarvestCycle, plantHarvest.PlantHarvestCycleId, plantHarvest.GetPlantName()));
                break;
        }

        return relatedEntities;
    }
    #endregion

    #region Public Plant Harvest Cycle Functions
    public async Task<List<PlantHarvestCycleModel>> GetPlantHarvests(string harvestCycleId, bool forceRefresh)
    {
        string key = string.Format(PLANT_HARVESTS_KEY, harvestCycleId);

        if (forceRefresh || !_cacheService.TryGetValue<List<PlantHarvestCycleModel>>(key, out List<PlantHarvestCycleModel>? plants))
        {
            _logger.LogDebug($"PlantHarvestCycles for {harvestCycleId} not in cache or forceRefresh");

            var harvestPlantsTask = GetPlantHarvestCycles(harvestCycleId);
            var imagesTask = _imageService.GetImages(RelatedEntityTypEnum.Plant, false);
            var imagesVarietyTask = _imageService.GetImages(RelatedEntityTypEnum.PlantVariety, false);
            await Task.WhenAll(harvestPlantsTask, imagesTask, imagesVarietyTask);

            plants = harvestPlantsTask.Result;
            var images = imagesTask.Result;
            var imagesVariety = imagesVarietyTask.Result;

            if (plants.Count > 0)
            {
                foreach (var plant in plants)
                {
                    if (!string.IsNullOrEmpty(plant.PlantVarietyId))
                    {
                        //if variety is selected - get that image.
                        plant.Images = imagesVariety.Where(i => i.RelatedEntityId == plant.PlantVarietyId).ToList();
                        var image = plant.Images.FirstOrDefault();
                        if (image != null)
                        {
                            plant.ImageFileName = image.FileName;
                            plant.ImageLabel = image.Label;
                        }
                    }

                    //if image is not found (may be variety was not found or variety has no image)
                    if (string.IsNullOrWhiteSpace(plant.ImageFileName))
                    {
                        plant.Images = images.Where(i => i.RelatedEntityId == plant.PlantId).ToList();

                        var image = plant.Images.FirstOrDefault();
                        if (image != null)
                        {
                            plant.ImageFileName = image.FileName;
                            plant.ImageLabel = image.Label;
                        }
                    }

                    //if plant image is still not found - dafault
                    if (string.IsNullOrWhiteSpace(plant.ImageFileName))
                    {
                        plant.ImageFileName = ImageService.NO_IMAGE;
                        plant.ImageLabel = "Add image";
                    }

                    plant.GardenBedLayout.ForEach(g =>
                    {
                        g.ImageFileName = plant.ImageFileName;
                        g.ImageLabel = plant.ImageLabel;
                    });
                }
                // Save data in cache.
                _cacheService.Set(key, plants, DateTime.Now.AddMinutes(_cacheDuration));
            }
        }
        else
        {
            _logger.LogDebug($"PlantHarvestCycles for {harvestCycleId} are in cache. Found {plants!.Count}");
        }

        return plants;
    }

    public async Task<List<PlantHarvestCycleIdentityOnlyViewModel>> GetPlantHarvestsByPLantId(string plantId)
    {
        List<PlantHarvestCycleIdentityOnlyViewModel> plants;

        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiGetAsync<List<PlantHarvestCycleIdentityOnlyViewModel>>(HarvestRoutes.GetPlantHarvestCyclesByPlant.Replace("{plantId}", plantId), _logger);

        if (!response.IsSuccess)
        {
            _toastService.ShowToast("Unable to get Garden Plan examples for this plant ", GardenLogToastLevel.Error);
            return new List<PlantHarvestCycleIdentityOnlyViewModel>();
        }

        plants = response.Response!;

        return plants;
    }

    public async Task<PlantHarvestCycleModel?> GetPlantHarvest(string harvestCycleId, string plantHarvestCycleId, bool forceRefresh)
    {
        PlantHarvestCycleModel plantHarvest;

        string key = string.Format(PLANT_HARVESTS_KEY, harvestCycleId);

        if (forceRefresh || !_cacheService.TryGetValue<List<PlantHarvestCycleModel>>(key, out List<PlantHarvestCycleModel>? plantHarvests))
        {
            System.Console.WriteLine($"PlantHarvestCycles for {harvestCycleId} not in cache or forceRefresh");

            var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

            var response = await httpClient.ApiGetAsync<PlantHarvestCycleModel>(HarvestRoutes.GetPlantHarvestCycle.Replace("{harvestId}", harvestCycleId).Replace("{id}", plantHarvestCycleId), _logger);

            if (!response.IsSuccess)
            {
                _toastService.ShowToast("Unable to get Garden Plan deatils ", GardenLogToastLevel.Error);
                return null;
            }

            plantHarvest = response.Response!;

            if (plantHarvest != null)
            {
                AddOrUpdateToPlantHarvestCycleList(plantHarvest);
            }
        }
        else
        {
            _logger.LogDebug($"PlantHarvestCycles for {harvestCycleId} are in cache. Found {plantHarvests!.Count}");
            plantHarvest = plantHarvests.First(p => p.PlantHarvestCycleId == plantHarvestCycleId);
        }

        return plantHarvest;
    }

    public async Task<ApiObjectResponse<string>> CreatePlantHarvest(PlantHarvestCycleModel plant)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiPostAsync(HarvestRoutes.CreatePlantHarvestCycle, plant);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to add Plant to Garden Plan. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response from Garden Plan Post: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            plant.PlantHarvestCycleId = response.Response!;

            //forse refresh to get plant schedule into cache
            await GetPlantHarvest(plant.HarvestCycleId, plant.PlantHarvestCycleId, true);

            //AddOrUpdateToPlantHarvestCycleList(newPlant);

            _toastService.ShowToast($"Plant is added to the Garden Plan", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> UpdatePlantHarvest(PlantHarvestCycleModel plant)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiPutAsync(HarvestRoutes.UpdatePlantHarvestCycle.Replace("{harvestId}", plant.HarvestCycleId).Replace("{id}", plant.PlantHarvestCycleId), plant);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to update a Garden Plan. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            await GetPlantHarvest(plant.HarvestCycleId, plant.PlantHarvestCycleId, true);
            // AddOrUpdateToPlantHarvestCycleList(plant);

            _toastService.ShowToast($"Garden Plan is successfully updated.", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> DeletePlantHarvest(string harvestId, string id)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiDeleteAsync(HarvestRoutes.DeletePlantHarvestCycle.Replace("{harvestId}", harvestId).Replace("{id}", id));

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to change Garden Plan. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            RemoveFromPlantHarvestCycleList(harvestId, id);

            _toastService.ShowToast($"Garden Plan changed.", GardenLogToastLevel.Success);
        }
        return response;
    }
    #endregion



    #region Private Harvest Cycle Functions
    private async Task<List<HarvestCycleModel>> GetAllHarvests()
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiGetAsync<List<HarvestCycleModel>>(HarvestRoutes.GetAllHarvestCycles, _logger);

        if (!response.IsSuccess)
        {
            _toastService.ShowToast("Unable to get Garden Plans", GardenLogToastLevel.Error);
            return new List<HarvestCycleModel>();
        }

        return response.Response!;
    }

    private void AddOrUpdateToHarvestCycleList(HarvestCycleModel harvest)
    {

        if (_cacheService.TryGetValue<List<HarvestCycleModel>>(HARVESTS_KEY, out List<HarvestCycleModel>? harvests))
        {
            var index = harvests!.FindIndex(p => p.HarvestCycleId == harvest.HarvestCycleId);
            if (index > -1)
            {
                harvests[index] = harvest;
                return;
            }
        }
        else
        {
            harvests = new List<HarvestCycleModel>();
            _cacheService.Set(HARVESTS_KEY, harvests, DateTime.Now.AddMinutes(_cacheDuration));
        }
        harvests.Add(harvest);

    }

    private void RemoveFromHarvestCycleList(string harvestId)
    {
        if (_cacheService.TryGetValue<List<HarvestCycleModel>>(HARVESTS_KEY, out var harvests))
        {
            var index = harvests!.FindIndex(p => p.HarvestCycleId == harvestId);
            if (index > -1)
            {
                harvests.RemoveAt(index);
            }
        }
    }
    #endregion

    #region Public Garden Bed Plant Harvest Cycle Functions
    public async Task<ApiObjectResponse<string>> CreateGardenBedPlantHarvestCycle(GardenBedPlantHarvestCycleModel gardenBedPlant)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);
        var url = HarvestRoutes.CreateGardenBedPlantHarvestCycle;

        var response = await httpClient.ApiPostAsync(url.Replace("harvestrId", gardenBedPlant.HarvestCycleId).Replace("plantHarvestId", gardenBedPlant.PlantHarvestCycleId), gardenBedPlant);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to add Plant to Garden Layout. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response from Garden Layout Post: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            gardenBedPlant.GardenBedPlantHarvestCycleId = response.Response!;

            var harvestPlant = await GetPlantHarvest(gardenBedPlant.HarvestCycleId, gardenBedPlant.PlantHarvestCycleId, false);
            if (harvestPlant != null)
            {
                if (harvestPlant.GardenBedLayout.FirstOrDefault(g => g.GardenBedPlantHarvestCycleId == gardenBedPlant.GardenBedPlantHarvestCycleId) == null)
                {
                    harvestPlant.GardenBedLayout.Add(gardenBedPlant);
                }
            }

            _toastService.ShowToast($"Plant is added to the Garden Layout", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> UpdateGardenBedPlantHarvestCycle(GardenBedPlantHarvestCycleModel gardenBedPlant)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var url = HarvestRoutes.UpdateGardenBedPlantHarvestCycle;

        var response = await httpClient.ApiPutAsync(url.Replace("{harvestId}", gardenBedPlant.HarvestCycleId).Replace("{plantHarvestId}", gardenBedPlant.PlantHarvestCycleId).Replace("{id}", gardenBedPlant.GardenBedPlantHarvestCycleId), gardenBedPlant);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to update a Garden Layout. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            _toastService.ShowToast($"Garden Layout is successfully updated.", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> DeleteGardenBedPlantHarvestCycle(string harvestId, string plantHarvestId, string gardenBedPlantId)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);
        var url = HarvestRoutes.DeleteGardenBedPlantHarvestCycle.Replace("{harvestId}", harvestId).Replace("{plantHarvestId}", plantHarvestId).Replace("{id}", gardenBedPlantId);
        var response = await httpClient.ApiDeleteAsync(url);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to change Garden Layout. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            _toastService.ShowToast($"Garden Layout deleted.", GardenLogToastLevel.Success);
        }
        return response;
    }
    #endregion

    #region Private Plant Harvest Cycle
    private async Task<List<PlantHarvestCycleModel>> GetPlantHarvestCycles(string harvestCycleId)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTHARVEST_API);

        var response = await httpClient.ApiGetAsync<List<PlantHarvestCycleModel>>(HarvestRoutes.GetPlantHarvestCycles.Replace("{harvestId}", harvestCycleId), _logger);

        if (!response.IsSuccess)
        {
            _toastService.ShowToast("Unable to get Garden Plan deatils ", GardenLogToastLevel.Error);
            return new List<PlantHarvestCycleModel>();
        }

        return response.Response!;
    }
    private void AddOrUpdateToPlantHarvestCycleList(PlantHarvestCycleModel plant)
    {
        string key = string.Format(PLANT_HARVESTS_KEY, plant.HarvestCycleId);

        if (_cacheService.TryGetValue<List<PlantHarvestCycleModel>>(key, out List<PlantHarvestCycleModel>? plants))
        {
            var index = plants!.FindIndex(p => p.PlantHarvestCycleId == plant.PlantHarvestCycleId);
            if (index > -1)
            {
                plants[index] = plant;
                return;
            }
            plants.Add(plant);
        }
        //else
        //{
        //    plants = new List<PlantHarvestCycleModel>();
        //    _cacheService.Set(key, plants, DateTime.Now.AddMinutes(CACHE_DURATION));
        //}


    }
    private void RemoveFromPlantHarvestCycleList(string harvestId, string id)
    {
        string key = string.Format(PLANT_HARVESTS_KEY, harvestId);
        if (_cacheService.TryGetValue<List<PlantHarvestCycleModel>>(key, out var plants))
        {
            var index = plants!.FindIndex(p => p.PlantHarvestCycleId == id);
            if (index > -1)
            {
                plants.RemoveAt(index);
            }
        }
    }

    #endregion

}
