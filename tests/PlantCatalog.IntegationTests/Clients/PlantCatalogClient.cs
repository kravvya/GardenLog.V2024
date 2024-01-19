using GardenLog.SharedInfrastructure.Extensions;
using PlantCatalog.Contract;
using PlantCatalog.Contract.Commands;

namespace PlantCatalog.IntegrationTest.Clients
{
    public class PlantCatalogClient
    {
        private readonly Uri _baseUrl;
        private readonly HttpClient _httpClient;

        public PlantCatalogClient(Uri baseUrl, HttpClient httpClient)
        {
            _baseUrl = baseUrl;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("RequestUser", "86377291-980f-4af2-8608-39dbbf7e09e1");
        }

        #region Plant
        public async Task<HttpResponseMessage> GetPlantIdByPlantName(string name)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.GetIdByPlantName}";
            return await this._httpClient.GetAsync(url.Replace("{name}", name));
        }

        public async Task<HttpResponseMessage> CreatePlant(string name)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.CreatePlant}/";

            var createPlantCommand = PopulateCreatePlantCommand(name);

            using var requestContent = createPlantCommand.ToJsonStringContent();

            return await this._httpClient.PostAsync(url, requestContent);

        }

        public async Task<HttpResponseMessage> UpdatePlant(PlantViewModel plant)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.UpdatePlant}";

            using var requestContent = plant.ToJsonStringContent();

            return await this._httpClient.PutAsync(url.Replace("{id}", plant.PlantId), requestContent);

        }

        public async Task<HttpResponseMessage> DeletePLant(string id)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.DeletePlant}";

            return await this._httpClient.DeleteAsync (url.Replace("{id}",id));
        }

        public async Task<HttpResponseMessage> GetAllPlants()
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.GetAllPlants}/";
           return await this._httpClient.GetAsync(url);           
        }

        public async Task<HttpResponseMessage> GetAllPlantNames()
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.GetAllPlantNames}";
            return await this._httpClient.GetAsync(url);
        }

        public async Task<HttpResponseMessage> GetPlant(string id)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.GetPlantById}";
            return await this._httpClient.GetAsync(url.Replace("{id}", id));
        }

        private static CreatePlantCommand PopulateCreatePlantCommand(string name)
        {
            return new CreatePlantCommand()
            {
                Color = "Black",
                Description = "Black Vegetable",
                GardenTip = "Only grows at night",
                GrowTolerance = Contract.Enum.GrowToleranceEnum.LightFrost | Contract.Enum.GrowToleranceEnum.HardFrost,
                Lifecycle = Contract.Enum.PlantLifecycleEnum.Cool,
                LightRequirement = Contract.Enum.LightRequirementEnum.FullShade,
                MoistureRequirement = Contract.Enum.MoistureRequirementEnum.DroughtTolerant,
                Name = name,
                SeedViableForYears = 10,
                Type = Contract.Enum.PlantTypeEnum.Vegetable,
                Tags= new List<string>() { "Bush", "Pole"},
                VarietyColors= new List<string>() { "Black"},
                DaysToMaturityMin = 10,
                DaysToMaturityMax = 20
            };
        }
        #endregion

        #region Plant Grow Instruction

        public async Task<HttpResponseMessage> CreatePlantGrowInstruction(string plantId, string name)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.CreatePlantGrowInstruction}";

            var createPlantGrowInstructionCommand = PopulateCreatePlantGrowInstructionCommand(plantId, name);

            using var requestContent = createPlantGrowInstructionCommand.ToJsonStringContent();

            return await this._httpClient.PostAsync(url, requestContent);

        }

        public async Task<HttpResponseMessage> UpdatePlantGrowInstruction(PlantGrowInstructionViewModel grow)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.UpdatePlantGrowInstructions}";

            using var requestContent = grow.ToJsonStringContent();

            return await this._httpClient.PutAsync(url.Replace("{plantId}", grow.PlantId).Replace("{id}", grow.PlantGrowInstructionId), requestContent);
        }

        public async Task<HttpResponseMessage> DeletePlantGrowInstruction(string plantId,string id)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.DeletePlantGrowInstructions}";

            return await this._httpClient.DeleteAsync(url.Replace("{plantId}", plantId).Replace("{id}", id));
        }

        public async Task<HttpResponseMessage> GetPlantGrowInstructions(string plantId)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.GetPlantGrowInstructions}";
            return await this._httpClient.GetAsync(url.Replace("{plantId}", plantId));
        }

        public async Task<HttpResponseMessage> GetPlantGrowInstruction(string plantId, string id)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.GetPlantGrowInstruction}";
            return await this._httpClient.GetAsync(url.Replace("{plantId}", plantId).Replace("{id}", id));
        }

        private static CreatePlantGrowInstructionCommand PopulateCreatePlantGrowInstructionCommand (string plantId, string name)
        {
            return new CreatePlantGrowInstructionCommand()
            {
                PlantId = plantId,
                DaysToSproutMax = 7,
                DaysToSproutMin = 1,
                FertilizerFrequencyForSeedlingsInWeeks = 1,
                FertilizeFrequencyInWeeks = 4,
                Fertilizer = Contract.Enum.FertilizerEnum.AllPurpose,
                FertilizerAtPlanting = Contract.Enum.FertilizerEnum.Nitrogen,
                FertilizerForSeedlings = Contract.Enum.FertilizerEnum.Nitrogen,
                GrowingInstructions = "Should be easy. grows by itself.",
                HarvestInstructions = "Pick one at a time",
                HarvestSeason = Contract.Enum.HarvestSeasonEnum.Summer,
                Name = name,
                PlantingDepthInInches = Contract.Enum.PlantingDepthEnum.Depth8th,
                PlantingMethod = Contract.Enum.PlantingMethodEnum.SeedIndoors,
                SpacingInInches = 24,
                StartSeedAheadOfWeatherCondition = Contract.Enum.WeatherConditionEnum.BeforeLastFrost,
                StartSeedInstructions = "One per cell",
                StartSeedWeeksAheadOfWeatherCondition = 2,
                StartSeedWeeksRange = 4,
                TransplantAheadOfWeatherCondition = Contract.Enum.WeatherConditionEnum.AfterDangerOfFrost,
                TransplantWeeksAheadOfWeatherCondition = 0,
                TransplantWeeksRange = 4,
                PlantsPerFoot = 0.5
            };
        }

        #endregion

        #region Plant Variety

        public async Task<HttpResponseMessage> CreatePlantVariety(string plantId, string name)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.CreatePlantVariety}";

            var createPlantVarietyCommand = PopulateCreatePlantVarietyCommand(plantId, name);

            using var requestContent = createPlantVarietyCommand.ToJsonStringContent();

            return await this._httpClient.PostAsync(url, requestContent);

        }

        public async Task<HttpResponseMessage> UpdatePlantVariety(PlantVarietyViewModel variety)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.UpdatePlantVariety}";

            using var requestContent = variety.ToJsonStringContent();

            return await this._httpClient.PutAsync(url.Replace("{plantId}", variety.PlantId).Replace("{id}", variety.PlantVarietyId), requestContent);
        }

        public async Task<HttpResponseMessage> DeletePlantVariety(string plantId, string id)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.DeletePlantVariety}";

            return await this._httpClient.DeleteAsync(url.Replace("{plantId}", plantId).Replace("{id}", id));
        }

        public async Task<HttpResponseMessage> GetPlantVarieties()
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.GetAllPlantVarieties}";
            return await this._httpClient.GetAsync(url);
        }

        public async Task<HttpResponseMessage> GetPlantVarieties(string plantId)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.GetPlantVarieties}";
            return await this._httpClient.GetAsync(url.Replace("{plantId}", plantId));
        }

        public async Task<HttpResponseMessage> GetPlantVariety(string plantId, string id)
        {
            var url = $"{this._baseUrl.OriginalString}{Routes.GetPlantVariety}";
            return await this._httpClient.GetAsync(url.Replace("{plantId}", plantId).Replace("{id}", id));
        }

        private static CreatePlantVarietyCommand PopulateCreatePlantVarietyCommand(string plantId, string name)
        {
            return new CreatePlantVarietyCommand()
            {
                PlantId = plantId,
                Name = name,
                Colors= new List<string> (){"Black"},
                DaysToMaturityMax= 100,
                DaysToMaturityMin= 1,
                Description ="Black new Variety",
                GrowTolerance = Contract.Enum.GrowToleranceEnum.LightFrost,
                HeightInInches= 100,
                IsHeirloom= true,
                LightRequirement=Contract.Enum.LightRequirementEnum.FullShade,
                MoistureRequirement = Contract.Enum.MoistureRequirementEnum.DroughtTolerant,
                Tags= new List<string>() { "Dark"},
                Title = "Very Black Variety"
            };
        }
        #endregion
    }
}
