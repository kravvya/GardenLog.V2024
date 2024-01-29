using GardenLog.SharedInfrastructure.Extensions;
using GardenLog.SharedKernel;
using GardenLog.SharedKernel.Enum;
using PlantCatalog.Contract;
using PlantHarvest.Contract;
using PlantHarvest.Contract.Commands;
using PlantHarvest.Contract.Enum;
using PlantHarvest.Contract.ViewModels;

namespace PlantHarvest.IntegrationTest.Clients
{
    public class WorkLogClient
    {
        private readonly Uri _baseUrl;
        private readonly HttpClient _httpClient;

        public WorkLogClient(Uri baseUrl, HttpClient httpClient)
        {
            _baseUrl = baseUrl;
            _httpClient = httpClient;               
        }

        #region Work Log
        
        public async Task<HttpResponseMessage> CreateWorkLog(RelatedEntityTypEnum entityType, string entityId)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.CreateWorkLog}/";

            var createWorkLogCommand = PopulateCreateWorkLogCommand(entityType, entityId);

            using var requestContent = createWorkLogCommand.ToJsonStringContent();

            return await this._httpClient.PostAsync(url, requestContent);

        }

        public async Task<HttpResponseMessage> UpdateWorkLog(WorkLogViewModel workLog)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.UpdateWorkLog}";

            using var requestContent = workLog.ToJsonStringContent();

            return await this._httpClient.PutAsync(url.Replace("{id}", workLog.WorkLogId), requestContent);
        }

        public async Task<HttpResponseMessage> DeleteWorkLog(string id)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.DeleteWorkLog}";

            return await this._httpClient.DeleteAsync(url.Replace("{id}", id));
        }

        public async Task<HttpResponseMessage> GetWorkLogs(RelatedEntityTypEnum entityType, string entityId)
        {
            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.GetWorkLogs}/";
            return await this._httpClient.GetAsync(url.Replace("{entityType}", entityType.ToString()).Replace("{entityId}", entityId));
        }

        private static CreateWorkLogCommand PopulateCreateWorkLogCommand(RelatedEntityTypEnum entityType, string entityId)
        {

            var relatedEntities = new List<RelatedEntity>();
            if(!string.IsNullOrEmpty(entityId))
            {
                relatedEntities.Add(new RelatedEntity(entityType, entityId, "Test HarvestCycle"));
            }

            return new CreateWorkLogCommand()
            {
                Log = "Created by Integration test",
                EventDateTime = DateTime.Now,
                EnteredDateTime = DateTime.Now,
                Reason = Contract.Enum.WorkLogReasonEnum.Information,
                RelatedEntities = relatedEntities
            };
        }
        #endregion
    }
}
