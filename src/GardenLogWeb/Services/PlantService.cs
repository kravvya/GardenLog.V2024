using ImageCatalog.Contract.Queries;

namespace GardenLogWeb.Services;

public interface IPlantService
{
    Task<List<PlantModel>> GetPlants(bool forceRefresh);
    Task<PlantModel?> GetPlant(string plantId, bool useCache);
    Task<ApiObjectResponse<string>> CreatePlant(PlantModel plant);
    Task<ApiResponse> UpdatePlant(PlantModel plant);
    Task<ApiResponse> DeletePlant(string id);


    Task<List<PlantVarietyModel>> GetAllPlantVarieties(bool useCache);
    Task<List<PlantVarietyModel>> GetPlantVarieties(string plantId, bool useCache);
    Task<PlantVarietyModel?> GetPlantVariety(string plantId, string plantVarietyId);
    Task<ApiObjectResponse<string>> CreatePlantVariety(PlantVarietyModel variety);
    Task<ApiResponse> UpdatePlantVariety(PlantVarietyModel variety);
    Task<ApiResponse> DeletePlantVariety(string plantId, string id);

    Task<List<PlantGrowInstructionViewModel>> GetPlantGrowInstructions(string plantId, bool useCache);
    Task<PlantGrowInstructionViewModel?> GetPlantGrowInstruction(string plantId, string growInstructionId);
    Task<ApiObjectResponse<string>> CreatePlantGrowInstruction(PlantGrowInstructionViewModel plantGrowInstruction);
    Task<ApiResponse> UpdatePlantGrowInstruction(PlantGrowInstructionViewModel plantGrowInstruction);
    Task<ApiResponse> DeletePlantGrowInstruction(string plantId, string id);

    string GetRandomPlantColor();
    Task<List<PlantNameModel>> GetPlantNames(bool forceRefresh);
}

public class PlantService : IPlantService
{
    private readonly ILogger<PlantService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICacheService _cacheService;
    private readonly IGardenLogToastService _toastService;
    private readonly IImageService _imageService;

    private readonly int _cacheDuration;
    private readonly Random _random = new();
    private const string PLANTS_KEY = "Plants";
    private const string PLANT_NAMES_KEY = "PlantNames";
    private const string PLANT_VARIETY_KEY = "Plant_{0}_Variety";
    private const string PLANT_GROW_INSTRUCTION_KEY = "Plant_{0}_GrowInstruction";


    public PlantService(ILogger<PlantService> logger, IHttpClientFactory clientFactory, ICacheService cacheService, IGardenLogToastService _toastService, IImageService imageService, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = clientFactory;
        _cacheService = cacheService;
        this._toastService = _toastService;
        _imageService = imageService;
        if (!int.TryParse(configuration[GlobalConstants.GLOBAL_CACHE_DURATION], out _cacheDuration)) _cacheDuration = 10;
    }

    #region Public Plant Functions
    public string GetRandomPlantColor()
    {
        var color = System.Drawing.Color.FromArgb(_random.Next(256), _random.Next(256), _random.Next(256));
        return $"#{color.R:X2}{color.G:X2)}{color.B:X2)}";
    }  

    public async Task<List<PlantModel>> GetPlants(bool forceRefresh)
    {

        if (forceRefresh || !_cacheService.TryGetValue<List<PlantModel>>(PLANTS_KEY, out List<PlantModel>? plants))
        {
            _logger.LogInformation("Plants not in cache or forceRefresh");

            var plantsTask = GetAllPlants();

            var imagesTask = _imageService.GetImages(RelatedEntityTypEnum.Plant, false);

            await Task.WhenAll(plantsTask, imagesTask);

            plants = plantsTask.Result;
            var images = imagesTask.Result;

            plants ??= new List<PlantModel>();

            if (plants.Count > 0)
            {

                foreach (var plant in plants)
                {
                    plant.Images = images.Where(i => i.RelatedEntityId == plant.PlantId).ToList();

                    var image = plant.Images.FirstOrDefault();
                    if (image != null)
                    {
                        plant.ImageFileName = image.FileName;
                        plant.ImageLabel = image.Label;
                    }
                    else
                    {
                        plant.ImageFileName = ImageService.NO_IMAGE;
                        plant.ImageLabel = "Add image";
                    }
                }

                // Save data in cache.
                _cacheService.Set(PLANTS_KEY, plants, DateTime.Now.AddMinutes(_cacheDuration));
            }
        }

        else
        {
            _logger.LogInformation($"Plants are in cache. Found {plants!.Count}");
        }

        return plants;
    }

    public async Task<List<PlantNameModel>> GetPlantNames(bool forceRefresh)
    {

        if (forceRefresh || !_cacheService.TryGetValue<List<PlantNameModel>>(PLANT_NAMES_KEY, out List<PlantNameModel>? plantNames))
        {
            _logger.LogInformation("Plant names are not in cache or forceRefresh");

            plantNames = await GetAllPlantNames();

            // Save data in cache.
            _cacheService.Set(PLANT_NAMES_KEY, plantNames, DateTime.Now.AddMinutes(_cacheDuration));
        }
        else
        {
            _logger.LogInformation($"Plant names are in cache. Found {plantNames!.Count}");
        }

        return plantNames;
    }

    public async Task<PlantModel?> GetPlant(string plantId, bool useCache)
    {
        PlantModel? plant = null;

        var plants = (await GetPlants(false));

        if (useCache)
        {
            plant = plants.FirstOrDefault(p => p.PlantId == plantId);
        }

        if (plant == null)
        {
            var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

            var response = await httpClient.ApiGetAsync<PlantModel>(Routes.GetPlantById.Replace("{id}", plantId), _logger);

            if (!response.IsSuccess)
            {
                _toastService.ShowToast("Unable to get Plant details", GardenLogToastLevel.Error);
                return null;
            }

            plant = response.Response;

            if (plant != null) AddOrUpdateToPlantList(plant);
        }

        return plant;
    }

    public async Task<ApiObjectResponse<string>> CreatePlant(PlantModel plant)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        var response = await httpClient.ApiPostAsync(Routes.CreatePlant, plant);


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
            plant.PlantId = response.Response!;
            plant.Images = new();
            plant.ImageFileName = ImageService.NO_IMAGE;
            plant.ImageLabel = string.Empty;
            AddOrUpdateToPlantList(plant);

            _toastService.ShowToast($"Plant created. Plant id is {plant.PlantId}", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> UpdatePlant(PlantModel plant)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        var response = await httpClient.ApiPutAsync(Routes.UpdatePlant.Replace("{id}", plant.PlantId), plant);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to update a Plant. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            AddOrUpdateToPlantList(plant);

            _toastService.ShowToast($"{plant.Name} is successfully updated.", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> DeletePlant(string id)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        var response = await httpClient.ApiDeleteAsync(Routes.DeletePlant.Replace("{id}", id));

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to delete a Plant. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            RemoveFromPlantList(id);

            _toastService.ShowToast($"Plant deleted. Plant id was {id}", GardenLogToastLevel.Success);
        }
        return response;
    }

    #endregion

    #region Public Plant Variety Functions
    public async Task<List<PlantVarietyModel>> GetAllPlantVarieties(bool useCache)
    {
        List<PlantVarietyModel>? plantVarietyList = null;

        if (useCache)
        {
            plantVarietyList = GetPlantVarietiesFromCache(string.Empty);
        }

        if (plantVarietyList == null)
        {
            plantVarietyList = await GetPlantVarieties();

            if (plantVarietyList.Count > 0)
            {
                List<GetImagesByRelatedEntity> relatedEntities = new();
                foreach (var variety in plantVarietyList)
                {
                    relatedEntities.Add(new GetImagesByRelatedEntity(RelatedEntityTypEnum.PlantVariety, variety.PlantVarietyId, false));
                }
                var images = await _imageService.GetImagesInBulk(relatedEntities);

                foreach (var variety in plantVarietyList)
                {
                    variety.Images = images.Where(i => i.RelatedEntityId == variety.PlantVarietyId).ToList();

                    var image = variety.Images.FirstOrDefault();
                    if (image != null)
                    {
                        variety.ImageFileName = image.FileName;
                        variety.ImageLabel = image.Label;
                    }
                    else
                    {
                        variety.ImageFileName = ImageService.NO_IMAGE;
                        variety.ImageLabel = "Add image";
                    }
                }
            }

            AddPlantVarietiesToCache(string.Empty, plantVarietyList);
        }

        return plantVarietyList;
    }

    public async Task<List<PlantVarietyModel>> GetPlantVarieties(string plantId, bool useCache)
    {
        List<PlantVarietyModel>? plantVarietyList = null;

        if (useCache)
        {
            plantVarietyList = GetPlantVarietiesFromCache(plantId);
        }

        if (plantVarietyList == null)
        {
            plantVarietyList = await GetPlantVarieties(plantId);

            plantVarietyList ??= new List<PlantVarietyModel>();

            if (plantVarietyList.Count > 0)
            {
                List<GetImagesByRelatedEntity> relatedEntities = new();
                foreach (var variety in plantVarietyList)
                {
                    relatedEntities.Add(new GetImagesByRelatedEntity(RelatedEntityTypEnum.PlantVariety, variety.PlantVarietyId, false));
                }
                var images = await _imageService.GetImagesInBulk(relatedEntities);

                foreach (var variety in plantVarietyList)
                {
                    variety.Images = images.Where(i => i.RelatedEntityId == variety.PlantVarietyId).ToList();

                    var image = variety.Images.FirstOrDefault();
                    if (image != null)
                    {
                        variety.ImageFileName = image.FileName;
                        variety.ImageLabel = image.Label;
                    }
                    else
                    {
                        variety.ImageFileName = ImageService.NO_IMAGE;
                        variety.ImageLabel = "Add image";
                    }
                }
            }

            AddPlantVarietiesToCache(plantId, plantVarietyList);
        }

        return plantVarietyList;
    }

    public async Task<PlantVarietyModel?> GetPlantVariety(string plantId, string plantVerietyId)
    {
        PlantVarietyModel? plantVariety;

        var plantVarietyTask = GetPlantVarietyFromServer(plantId, plantVerietyId);
        var imagesTask = _imageService.GetImages(RelatedEntityTypEnum.PlantVariety, plantVerietyId, false);

        await Task.WhenAll(plantVarietyTask, imagesTask);

        plantVariety = plantVarietyTask.Result;
        var images = imagesTask.Result;

        if (plantVariety == null) return plantVariety;

        plantVariety.Images = images;
        var image = plantVariety.Images.FirstOrDefault();
        if (image != null)
        {
            plantVariety.ImageFileName = image.FileName;
            plantVariety.ImageLabel = image.Label;
        }
        else
        {
            plantVariety.ImageFileName = ImageService.NO_IMAGE;
            plantVariety.ImageLabel = "Add image";
            plantVariety.Images = new();
        }

        AddOrUpdateToPlantVarietyList(plantVariety);

        return plantVariety;
    }

    public async Task<ApiObjectResponse<string>> CreatePlantVariety(PlantVarietyModel variety)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        var response = await httpClient.ApiPostAsync(Routes.CreatePlantVariety.Replace("{plantId}", variety.PlantId), variety);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to create a Plant Variety. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response from Plant Variety Post: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            variety.PlantVarietyId = response.Response!;
            AddOrUpdateToPlantVarietyList(variety);
            IncrementVarietyCountInCache(variety.PlantId);
            _toastService.ShowToast($"Plant Variety created. Plant Variety id is {variety.PlantVarietyId}", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> UpdatePlantVariety(PlantVarietyModel variety)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        string route = Routes.UpdatePlantVariety.Replace("{plantId}", variety.PlantId).Replace("{id}", variety.PlantVarietyId);

        var response = await httpClient.ApiPutAsync(route, variety);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to update a Plant Variety. Please resolve validation errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            AddOrUpdateToPlantVarietyList(variety);

            _toastService.ShowToast($"{variety.Name} is successfully updated.", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> DeletePlantVariety(string plantId, string id)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        string route = Routes.DeletePlantVariety.Replace("{plantId}", plantId).Replace("{id}", id);

        var response = await httpClient.ApiDeleteAsync(route);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to delete a Variety. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            RemoveFromPlantVarietyList(plantId, id);

            _toastService.ShowToast($"Variety deleted. Variety id was {id}", GardenLogToastLevel.Success);
        }
        return response;
    }
    #endregion

    #region Public Plant Grow Instruction Functions

    public async Task<List<PlantGrowInstructionViewModel>> GetPlantGrowInstructions(string plantId, bool useCache)
    {
        List<PlantGrowInstructionViewModel>? plantGrowInstructionsList = null;

        string cacheKey = string.Format(PLANT_GROW_INSTRUCTION_KEY, plantId);

        if (useCache)
        {
            _cacheService.TryGetValue<List<PlantGrowInstructionViewModel>>(cacheKey, out plantGrowInstructionsList);
        }

        if (plantGrowInstructionsList == null)
        {
            var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);
            var url = Routes.GetPlantGrowInstructions.Replace("{plantId}", plantId);

            var response = await httpClient.ApiGetAsync<List<PlantGrowInstructionViewModel>>(url, _logger);

            if (!response.IsSuccess)
            {
                _toastService.ShowToast("Unable to get Plant Grow Instructions", GardenLogToastLevel.Error);
                return new List<PlantGrowInstructionViewModel>();
            }

            plantGrowInstructionsList = response.Response;

            if (plantGrowInstructionsList != null)
            {
                AddPlanGrowInstructionsToCache(plantId, plantGrowInstructionsList);
            }
            else
            {
                plantGrowInstructionsList = new List<PlantGrowInstructionViewModel>();
            }
        }

        return plantGrowInstructionsList;
    }

    public async Task<PlantGrowInstructionViewModel?> GetPlantGrowInstruction(string plantId, string growInstructionId)
    {
        PlantGrowInstructionViewModel? growInstruction;

        string route = Routes.GetPlantGrowInstruction.Replace("{plantId}", plantId).Replace("{id}", growInstructionId);

        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        var response = await httpClient.ApiGetAsync<PlantGrowInstructionViewModel>(route, _logger);

        if (!response.IsSuccess)
        {
            _toastService.ShowToast("Unable to get Plant Grow Instruction", GardenLogToastLevel.Error);
            return null;
        }

        growInstruction = response.Response;

        if (growInstruction!= null) await AddOrUpdateToPlantGrowInstructionList(growInstruction);

        return growInstruction;
    }

    public async Task<ApiObjectResponse<string>> CreatePlantGrowInstruction(PlantGrowInstructionViewModel plantGrowInstruction)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        var route = Routes.CreatePlantGrowInstruction.Replace("{plantId}", plantGrowInstruction.PlantId);
        var response = await httpClient.ApiPostAsync(route, plantGrowInstruction);


        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to add Grow Instructions. Please resolve validatione errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response from Grow Instruction Post: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            plantGrowInstruction.PlantGrowInstructionId = response.Response!;

            await AddOrUpdateToPlantGrowInstructionList(plantGrowInstruction);
            _toastService.ShowToast($"Grow Instructions added. Instruction id is {plantGrowInstruction.PlantGrowInstructionId}", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> UpdatePlantGrowInstruction(PlantGrowInstructionViewModel plantGrowInstruction)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        string route = Routes.UpdatePlantGrowInstructions.Replace("{plantId}", plantGrowInstruction.PlantId).Replace("{id}", plantGrowInstruction.PlantGrowInstructionId);

        var response = await httpClient.ApiPutAsync(route, plantGrowInstruction);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to update a Plant Grow Instruction. Please resolve validation errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            await AddOrUpdateToPlantGrowInstructionList(plantGrowInstruction);
            IncrementGrowCountInCache(plantGrowInstruction.PlantId);
            _toastService.ShowToast($"{plantGrowInstruction.Name} is successfully updated.", GardenLogToastLevel.Success);
        }

        return response;
    }

    public async Task<ApiResponse> DeletePlantGrowInstruction(string plantId, string id)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        string route = Routes.DeletePlantGrowInstructions.Replace("{plantId}", plantId).Replace("{id}", id);

        var response = await httpClient.ApiDeleteAsync(route);

        if (response.ValidationProblems != null)
        {
            _toastService.ShowToast($"Unable to delete a Instructions. Please resolve validation errors and try again.", GardenLogToastLevel.Error);
        }
        else if (!response.IsSuccess)
        {
            _toastService.ShowToast($"Received an invalid response: {response.ErrorMessage}", GardenLogToastLevel.Error);
        }
        else
        {
            RemoveFromPlanGrowInstructionList(plantId, id);

            _toastService.ShowToast($"Instructions removed. Instruction id was {id}", GardenLogToastLevel.Success);
        }
        return response;
    }
    #endregion

    #region Private Plant Functions
    private async Task<List<PlantModel>?> GetAllPlants()
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        var response = await httpClient.ApiGetAsync<List<PlantModel>>(Routes.GetAllPlants, _logger);

        if (!response.IsSuccess)
        {
            _toastService.ShowToast("Unable to get Plants", GardenLogToastLevel.Error);
            return null;
        }

        return response.Response;
    }

    private async Task<List<PlantNameModel>> GetAllPlantNames()
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        var response = await httpClient.ApiGetAsync<List<PlantNameModel>>(Routes.GetAllPlantNames, _logger);

        if (!response.IsSuccess)
        {
            _toastService.ShowToast("Unable to get Plant Names", GardenLogToastLevel.Error);
            return new List<PlantNameModel>();
        }

        return response.Response!;
    }


    private void AddOrUpdateToPlantList(PlantModel plant)
    {

        if (_cacheService.TryGetValue<List<PlantModel>>(PLANTS_KEY, out List<PlantModel>? plants))
        {
            var index = plants!.FindIndex(p => p.PlantId == plant.PlantId);
            if (index > -1)
            {
                plant.Images = plants[index].Images;
                plant.ImageFileName = plants[index].ImageFileName;
                plant.ImageLabel = plants[index].ImageLabel;
                plants[index] = plant;
            }
            else
            {
                plants.Add(plant);
            }
        }

        if (_cacheService.TryGetValue<List<PlantNameModel>>(PLANT_NAMES_KEY, out List<PlantNameModel>? plantNames))
        {
            var index = plantNames!.FindIndex(p => p.PlantId == plant.PlantId);
            if (index > -1)
            {
                plantNames[index].Name = plant.Name;
                plantNames[index].Color = plant.Color;
            }
            else
            {
                plantNames.Add(new PlantNameModel() { PlantId = plant.PlantId, Name = plant.Name, Color = plant.Color });
            }
        }
    }

    private void RemoveFromPlantList(string plantId)
    {

        if (_cacheService.TryGetValue<List<PlantModel>>(PLANTS_KEY, out List<PlantModel>? plants))
        {
            var index = plants!.FindIndex(p => p.PlantId == plantId);
            if (index > -1)
            {
                plants.RemoveAt(index);
            }
        }

        if (_cacheService.TryGetValue<List<PlantNameModel>>(PLANT_NAMES_KEY, out List<PlantNameModel>? plantNames))
        {
            var index = plantNames!.FindIndex(p => p.PlantId == plantId);
            if (index > -1)
            {
                plantNames.RemoveAt(index);
            }
        }
    }

    private void IncrementVarietyCountInCache(string plantId)
    {

        if (_cacheService.TryGetValue<List<PlantModel>>(PLANTS_KEY, out List<PlantModel>? plants))
        {
            var plant = plants!.Where(p => p.PlantId == plantId).FirstOrDefault();
            if (plant != null) plant.VarietyCount++;
        }
    }

    private void IncrementGrowCountInCache(string plantId)
    {

        if (_cacheService.TryGetValue<List<PlantModel>>(PLANTS_KEY, out List<PlantModel>? plants))
        {
            var plant = plants!.Where(p => p.PlantId == plantId).FirstOrDefault();
            if (plant != null) plant.GrowInstructionsCount++;
        }
    }
    #endregion

    #region Private Plant Variety Fucntions
    private async Task<List<PlantVarietyModel>> GetPlantVarieties()
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        var response = await httpClient.ApiGetAsync<List<PlantVarietyModel>>(Routes.GetAllPlantVarieties, _logger);

        if (!response.IsSuccess)
        {
            _toastService.ShowToast("Unable to get Plant Varieties", GardenLogToastLevel.Error);
            return new List<PlantVarietyModel>();
        }

        if (response.Response == null) return new List<PlantVarietyModel>();

        return response.Response;
    }

    private async Task<List<PlantVarietyModel>?> GetPlantVarieties(string plantId)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        var response = await httpClient.ApiGetAsync<List<PlantVarietyModel>>(Routes.GetPlantVarieties.Replace("{plantId}", plantId), _logger);

        if (!response.IsSuccess)
        {
            _toastService.ShowToast("Unable to get Plant Varieties", GardenLogToastLevel.Error);
            return null;
        }

        return response.Response;
    }

    private async Task<PlantVarietyModel?> GetPlantVarietyFromServer(string plantId, string plantVerietyId)
    {
        var httpClient = _httpClientFactory.CreateClient(GlobalConstants.PLANTCATALOG_API);

        var response = await httpClient.ApiGetAsync<PlantVarietyModel>(Routes.GetPlantVariety.Replace("{plantId}", plantId).Replace("{id}", plantVerietyId), _logger);

        if (!response.IsSuccess)
        {
            _toastService.ShowToast("Unable to get Plant variety", GardenLogToastLevel.Error);
            return null;
        }

        return response.Response;
    }


    private void AddOrUpdateToPlantVarietyList(PlantVarietyModel variety)
    {
        var varieties = GetPlantVarietiesFromCache(variety.PlantId);

        if (varieties == null) return;

        var index = varieties.FindIndex(p => p.PlantVarietyId == variety.PlantVarietyId);
        if (index > -1)
        {
            varieties[index] = variety;
        }
        else
        {
            varieties.Add(variety);
        }
    }

    private List<PlantVarietyModel>? GetPlantVarietiesFromCache(string plantId)
    {
        string cacheKey = string.Format(PLANT_VARIETY_KEY, plantId);

        _cacheService.TryGetValue<List<PlantVarietyModel>>(cacheKey, out List<PlantVarietyModel>? plantVarietyList);

        return plantVarietyList;
    }

    private void AddPlantVarietiesToCache(string plantId, List<PlantVarietyModel> plantVarietyList)
    {
        string cacheKey = string.Format(PLANT_VARIETY_KEY, plantId);

        _cacheService.Set(cacheKey, plantVarietyList, DateTime.Now.AddMinutes(_cacheDuration));
    }

    private void RemoveFromPlantVarietyList(string plantId, string plantVarietyId)
    {
        var plantVarieties = GetPlantVarietiesFromCache(plantId);

        if (plantVarieties == null) return;

        var index = plantVarieties.FindIndex(p => p.PlantVarietyId == plantVarietyId);
        if (index > -1)
        {
            plantVarieties.RemoveAt(index);
        }
    }
    #endregion

    #region Private Plant Grow Instructions

    private async Task AddOrUpdateToPlantGrowInstructionList(PlantGrowInstructionViewModel growInstructionModel)
    {
        var plantGrowInstructions = await GetPlantGrowInstructions(growInstructionModel.PlantId, true);

        if (plantGrowInstructions == null) return;

        var index = plantGrowInstructions.FindIndex(p => p.PlantGrowInstructionId == growInstructionModel.PlantGrowInstructionId);
        if (index > -1)
        {
            plantGrowInstructions[index] = growInstructionModel;
        }
        else
        {
            plantGrowInstructions.Add(growInstructionModel);
        }
    }

    private List<PlantGrowInstructionViewModel>? GetPlantGrowInstructionsFromCache(string plantId)
    {
        string cacheKey = string.Format(PLANT_GROW_INSTRUCTION_KEY, plantId);

        _cacheService.TryGetValue<List<PlantGrowInstructionViewModel>>(cacheKey, out List<PlantGrowInstructionViewModel>? plantGrowInstructions);

        return plantGrowInstructions;
    }

    private void AddPlanGrowInstructionsToCache(string plantId, List<PlantGrowInstructionViewModel> plantGrowInstructions)
    {
        string cacheKey = string.Format(PLANT_GROW_INSTRUCTION_KEY, plantId);

        _cacheService.Set(cacheKey, plantGrowInstructions, DateTime.Now.AddMinutes(_cacheDuration));
    }

    private void RemoveFromPlanGrowInstructionList(string plantId, string plantGrowInstructionId)
    {
        var growInstructions = GetPlantGrowInstructionsFromCache(plantId);

        if (growInstructions == null) return;

        var index = growInstructions.FindIndex(p => p.GrowingInstructions == plantGrowInstructionId);
        if (index > -1)
        {
            growInstructions.RemoveAt(index);
        }
    }

    #endregion

}