using PlantCatalog.Contract;
using PlantHarvest.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PlantHarvest.UnitTest
{
    internal class HttpClientsHelper
    {
        public const string PLANT_CATALOG_URL = "http://PlantCatalog.Api";
        public const string USER_MANAGEMENT_URL = "http://UserManagement.Api";

        public static HttpClient GetPlantCatalogHttpClientForGrowInstructions(PlantCatalog.Contract.Enum.PlantingMethodEnum plantingMethod)
        {
            List<KeyValuePair<string, string>> expectedresponses = new();

            expectedresponses.Add(new KeyValuePair<string, string>(
                    Routes.GetPlantGrowInstruction.Replace("{plantId}", PlantsHelper.PLANT_ID).Replace("{id}", PlantsHelper.GROW_INSTRUCTION_ID), PlantsHelper.GetGrowInstructionAsSerializedString(plantingMethod)));

            expectedresponses.Add(new KeyValuePair<string, string>(
                    Routes.GetPlantVariety.Replace("{plantId}", PlantsHelper.PLANT_ID).Replace("{id}", PlantsHelper.PLANT_VARIETY_ID), PlantsHelper.GetPlantVariety()));

            expectedresponses.Add(new KeyValuePair<string, string>(
                    Routes.GetPlantById.Replace("{id}", PlantsHelper.PLANT_ID), PlantsHelper.GetPlant()));


            return HttpClientTestHelper.GetMockedHttpClient(HttpStatusCode.OK, expectedresponses, new Uri(PLANT_CATALOG_URL));
        }

        public static HttpClient GetUserManagementHttpClientForGarden()
        {
            return HttpClientTestHelper.GetMockedHttpClient(HttpStatusCode.OK, UserManagementHelper.GetGardenAsString(), new Uri(USER_MANAGEMENT_URL));
        }
    }
}
