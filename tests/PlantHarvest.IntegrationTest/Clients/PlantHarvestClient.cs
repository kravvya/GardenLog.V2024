using GardenLog.SharedInfrastructure.Extensions;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using PlantCatalog.Contract;
using PlantHarvest.Contract.Commands;
using PlantHarvest.Contract.ViewModels;
using PlantHarvest.Domain.HarvestAggregate;
using System.Text.Json.Serialization;
using System.Text.Json;
using Xunit.Abstractions;
using System.Net.Http.Json;
using PlantHarvest.Contract;

namespace PlantHarvest.IntegrationTest.Clients
{
    public class PlantHarvestClient
    {
        private readonly Uri _baseUrl;
        private readonly HttpClient _httpClient;

        public PlantHarvestClient(Uri baseUrl, HttpClient httpClient)
        {
            _baseUrl = baseUrl;
            _httpClient = httpClient;
        }

        #region Harvest Cycle
        public async Task<HttpResponseMessage> GetHarvestCycleIdByHarvestCycleName(string name)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.GetIdByHarvestCycleName}";
            return await this._httpClient.GetAsync(url.Replace("{name}", name));
        }

        public async Task<HttpResponseMessage> CreateHarvestCycle(string name)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.CreateHarvestCycle}/";

            var createHarvestCycleCommand = PopulateCreateHarvestCycleCommand(name);

            using var requestContent = createHarvestCycleCommand.ToJsonStringContent();

            return await this._httpClient.PostAsync(url, requestContent);

        }

        public async Task<HttpResponseMessage> UpdateHarvestCycle(HarvestCycleViewModel harvest)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.UpdateHarvestCycle}";

            using var requestContent = harvest.ToJsonStringContent();

            return await this._httpClient.PutAsync(url.Replace("{id}", harvest.HarvestCycleId), requestContent);
        }

        public async Task<HttpResponseMessage> DeleteHarvestCycle(string id)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.DeleteHarvestCycle}";

            return await this._httpClient.DeleteAsync(url.Replace("{id}", id));
        }

        public async Task<HttpResponseMessage> GetAllHarvestCycles()
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.GetAllHarvestCycles}/";
            return await this._httpClient.GetAsync(url);
        }

        public async Task<HttpResponseMessage> GetHarvestCycle(string id)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.GetHarvestCycleById}";
            return await this._httpClient.GetAsync(url.Replace("{id}", id));
        }

        private static CreateHarvestCycleCommand PopulateCreateHarvestCycleCommand(string name)
        {
            return new CreateHarvestCycleCommand()
            {
                HarvestCycleName = name,
                StartDate = DateTime.Now,
                GardenId = "39131fc5-61f5-43e2-9243-a537a75487b1",
                Notes = "Integration test Harvest Cycle"
            };
        }
        #endregion

        #region Plant Harvest Cycle 

        public async Task<HttpResponseMessage> CreatePlantHarvestCycle(string harvestId, string plantId, string plantVarietyId)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.CreatePlantHarvestCycle}";

            var createPlantHarvestCycleCommand = PopulateCreatePlantHarvestCycleCommand(harvestId, plantId, plantVarietyId);

            using var requestContent = createPlantHarvestCycleCommand.ToJsonStringContent();

            return await this._httpClient.PostAsync(url, requestContent);

        }

        public async Task<HttpResponseMessage> UpdatePlantHarvestCycle(PlantHarvestCycleViewModel HarvestCycle)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.UpdatePlantHarvestCycle}";

            using var requestContent = HarvestCycle.ToJsonStringContent();

            return await this._httpClient.PutAsync(url.Replace("{harvestId}", HarvestCycle.HarvestCycleId).Replace("{id}", HarvestCycle.PlantHarvestCycleId), requestContent);
        }

        public async Task<HttpResponseMessage> DeletePlantHarvestCycle(string harvestId, string id)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.DeletePlantHarvestCycle}";

            return await this._httpClient.DeleteAsync(url.Replace("{harvestId}", harvestId).Replace("{id}", id));
        }

        public async Task<HttpResponseMessage> GetPlantHarvestCycles(string harvestId)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.GetPlantHarvestCycles}";
            return await this._httpClient.GetAsync(url.Replace("{harvestId}", harvestId));
        }

        public async Task<HttpResponseMessage> GetPlantHarvestCyclesByPlantId(string plantId)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.GetPlantHarvestCyclesByPlant}";
            return await this._httpClient.GetAsync(url.Replace("{plantId}", plantId));
        }

        public async Task<HttpResponseMessage> GetPlantHarvestCycle(string harvestId, string id)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.GetPlantHarvestCycle}";
            return await this._httpClient.GetAsync(url.Replace("{harvestId}", harvestId).Replace("{id}", id));
        }

        private static CreatePlantHarvestCycleCommand PopulateCreatePlantHarvestCycleCommand(string harvestId, string plantId, string plantVarietyId)
        {
            return new CreatePlantHarvestCycleCommand()
            {
                HarvestCycleId = harvestId,
                PlantId = plantId,
                PlantName = "Test Plant",
                PlantVarietyId = plantVarietyId,
                PlantVarietyName = "Test Variety",
                SeedVendorName = null,
                SeedVendorId = null,
                PlantGrowthInstructionId = "b295d016-35ed-4650-9e62-ab8b5791c4a9",
                PlantGrowthInstructionName = "Test Growth Instruction",
                PlantingMethod = Contract.Enum.PlantingMethodEnum.SeedIndoors,
                Notes = "Created by Integration test",
                SeedingDate = DateTime.Now
            };
        }
        #endregion

        #region Plant Schedule

        public async Task<HttpResponseMessage> CreatePlantSchedule(string harvestId, string plantHarvestId)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.CreatePlantSchedule}";

            var createPlantHarvestCycleCommand = PopulateCreatePlantScheduleCommand(harvestId, plantHarvestId);

            using var requestContent = createPlantHarvestCycleCommand.ToJsonStringContent();

            return await this._httpClient.PostAsync(url, requestContent);

        }

        public async Task<HttpResponseMessage> UpdatePlantSchedule(PlantScheduleViewModel HarvestCycle)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.UpdatePlantSchedule}";

            using var requestContent = HarvestCycle.ToJsonStringContent();

            return await this._httpClient.PutAsync(url.Replace("{harvestId}", HarvestCycle.HarvestCycleId).Replace("{plantHarvestId}", HarvestCycle.PlantHarvestCycleId).Replace("{id}", HarvestCycle.PlantScheduleId), requestContent);
        }

        public async Task<HttpResponseMessage> DeletePlantSchedule(string harvestId, string plantHarvestId, string id)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.DeletePlantSchedule}";

            return await this._httpClient.DeleteAsync(url.Replace("{harvestId}", harvestId).Replace("{plantHarvestId}", plantHarvestId).Replace("{id}", id));
        }

        private static CreatePlantScheduleCommand PopulateCreatePlantScheduleCommand(string harvestId, string plantHarvestId)
        {
            return new CreatePlantScheduleCommand()
            {
                HarvestCycleId = harvestId,
                PlantHarvestCycleId = plantHarvestId,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(6),
                IsSystemGenerated = true,
                TaskType = Contract.Enum.WorkLogReasonEnum.Information,
                Notes = "Created by Integration test"
            };
        }
        #endregion

        #region Garden Bed Layout

        public async Task<HttpResponseMessage> CreateGardenBedPlantHarvestCycle(string harvestId, string plantHarvestId)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.CreateGardenBedPlantHarvestCycle}";

            var createPlantHarvestCycleCommand = PopulateCreateGardenBedPlantHarvestCycleCommand(harvestId, plantHarvestId);

            using var requestContent = createPlantHarvestCycleCommand.ToJsonStringContent();

            return await this._httpClient.PostAsync(url, requestContent);

        }

        public async Task<HttpResponseMessage> UpdateGardenBedPlantHarvestCycle(GardenBedPlantHarvestCycleViewModel HarvestCycle)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.UpdateGardenBedPlantHarvestCycle}";

            using var requestContent = HarvestCycle.ToJsonStringContent();

            return await this._httpClient.PutAsync(url.Replace("{harvestId}", HarvestCycle.HarvestCycleId).Replace("{plantHarvestId}", HarvestCycle.PlantHarvestCycleId).Replace("{id}", HarvestCycle.GardenBedPlantHarvestCycleId), requestContent);
        }

        public async Task<HttpResponseMessage> DeleteGardenBedPlantHarvestCycle(string harvestId, string plantHarvestId, string id)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.DeleteGardenBedPlantHarvestCycle}";

            return await this._httpClient.DeleteAsync(url.Replace("{harvestId}", harvestId).Replace("{plantHarvestId}", plantHarvestId).Replace("{id}", id));
        }

        private static CreateGardenBedPlantHarvestCycleCommand PopulateCreateGardenBedPlantHarvestCycleCommand(string harvestId, string plantHarvestId)
        {
            return new CreateGardenBedPlantHarvestCycleCommand()
            {
                HarvestCycleId = harvestId,
                PlantHarvestCycleId = plantHarvestId,
                GardenBedId = Guid.NewGuid().ToString(),
                GardenId = Guid.NewGuid().ToString(),
                Length = 1,
                NumberOfPlants = 1,
                PatternLength = 1,
                PatternWidth = 1,
                PlantId = Guid.NewGuid().ToString(),
                PlantName = Guid.NewGuid().ToString(),
                PlantsPerFoot = 1,
                PlantVarietyId = Guid.NewGuid().ToString(),
                PlantVarietyName = Guid.NewGuid().ToString(),
                Width = 1,
                X = 1,
                Y = 1
            };
        }
        #endregion

        #region Shared Functions
        public async Task<string> GetHarvestCycleIdToWorkWith(string harvestName)
        {
            var response = await this.GetHarvestCycleIdByHarvestCycleName(harvestName);
            var harvestId = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(harvestId))
            {
                //_output.WriteLine($"GetHarvestCycleIdToWorkWith - Harvest Cycle is not found. Will create new one");
                response = await this.CreateHarvestCycle(harvestName);

                harvestId = await response.Content.ReadAsStringAsync();

                //_output.WriteLine($"GetHarvestCycleIdToWorkWith - Service to create harvest cycle responded with {response.StatusCode} code and {harvestId} message");
            }
            else
            {
                //_output.WriteLine($"GetHarvestCycleIdToWorkWith - Harvest Cycle was found with service responded with {response.StatusCode} code and {harvestId} message");
            }

            //_output.WriteLine($"GetHarvestCycleIdToWorkWith - Found  {harvestId} harvest to work with.");
            return harvestId;
        }

        public async Task<PlantHarvestCycleViewModel> GetPlantHarvestCycleToWorkWith(string harvestId, string plantId, string plantVarietyId)
        {
            var response = await this.GetPlantHarvestCycles(harvestId);

            //_output.WriteLine($"GetPlantHarvestCycleToWorkWith - Service to get all plant harvest cycle responded with {response.StatusCode} code");

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new JsonStringEnumConverter(),
                },
            };
            var plants = await response.Content.ReadFromJsonAsync<List<PlantHarvestCycleViewModel>>(options);
            PlantHarvestCycleViewModel? plant;

            if (plants == null || plants.Count == 0 || plants.FirstOrDefault(p => p.PlantId == plantId) == null)
            {
                var plantHarvestCycleId = await CreatePlantHarvestCycleToWorkWith(harvestId, plantId, plantVarietyId);
                response = await this.GetPlantHarvestCycle(harvestId, plantHarvestCycleId);

                //_output.WriteLine($"Service to getplant harvest cycle responded with {response.StatusCode} code");

                plant = await response.Content.ReadFromJsonAsync<PlantHarvestCycleViewModel>(options);
            }
            else
            {
                plant = plants.First(p => p.PlantVarietyId == plantVarietyId);
            }

            return plant!;
        }

        public async Task<string> CreatePlantHarvestCycleToWorkWith(string harvestId, string plantId, string plantVarietyId)
        {
            var response = await this.CreatePlantHarvestCycle(harvestId, plantId, plantVarietyId);
            var returnString = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Assert.NotEmpty(returnString);
                Assert.True(Guid.TryParse(returnString, out var plantHarvestCycleId));
            }
            else
            {
                Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
                returnString = await response.Content.ReadAsStringAsync();
                Assert.NotEmpty(returnString);
                Assert.Contains("This plant is already a part of this plan", returnString);
            }
            return returnString;
        }
        #endregion
    }
}
