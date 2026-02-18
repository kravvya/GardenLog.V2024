using GardenLog.SharedInfrastructure.Extensions;
using GardenLog.SharedKernel;
using GardenLog.SharedKernel.Enum;
using PlantCatalog.Contract;
using PlantHarvest.Contract;
using PlantHarvest.Contract.Commands;
using PlantHarvest.Contract.Enum;
using PlantHarvest.Contract.Query;
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

        public async Task<HttpResponseMessage> SearchWorkLogs(WorkLogSearch search)
        {
            if (string.IsNullOrWhiteSpace(search.PlantId))
            {
                throw new ArgumentException("plantId is required.", nameof(search));
            }

            var queryParams = new List<string>();

            queryParams.Add($"plantId={Uri.EscapeDataString(search.PlantId)}");

            if (search.StartDate.HasValue)
            {
                queryParams.Add($"startDate={Uri.EscapeDataString(search.StartDate.Value.ToString("o"))}");
            }

            if (search.EndDate.HasValue)
            {
                queryParams.Add($"endDate={Uri.EscapeDataString(search.EndDate.Value.ToString("o"))}");
            }

            if (search.Reason.HasValue)
            {
                queryParams.Add($"reason={Uri.EscapeDataString(search.Reason.Value.ToString())}");
            }

            if (search.Limit.HasValue)
            {
                queryParams.Add($"limit={search.Limit.Value}");
            }

            var url = $"{this._baseUrl.OriginalString}{HarvestRoutes.SearchWorkLogs}";
            if (queryParams.Count > 0)
            {
                url = $"{url}?{string.Join("&", queryParams)}";
            }

            return await this._httpClient.GetAsync(url);
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
