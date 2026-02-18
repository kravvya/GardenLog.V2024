using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel.Enum;
using GardenLog.SharedKernel.Interfaces;
using MongoDB.Driver;
using PlantHarvest.Contract.Query;

namespace PlantHarvest.Infrastructure.Data.Repositories;

public class WorkLogRepository : BaseRepository<WorkLog>, IWorkLogRepository
{
    private const string WORKLOG_COLLECTION_NAME = "WorkLog-Collection";
    private readonly ILogger<WorkLogRepository> _logger;

    public WorkLogRepository(IUnitOfWork unitOfWork, ILogger<WorkLogRepository> logger)
        : base(unitOfWork, logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<WorkLogViewModel>> GetWorkLogsByEntity(RelatedEntityTypEnum entityType, string entityId, string userProfileId)
    {
        List<FilterDefinition<WorkLog>> filters = new();
        filters.Add(Builders<WorkLog>.Filter.Eq("RelatedEntities.EntityType", entityType));
        filters.Add(Builders<WorkLog>.Filter.Eq("RelatedEntities.EntityId", entityId));
        filters.Add(Builders<WorkLog>.Filter.Eq("UserProfileId", userProfileId));

        var data = await Collection
          .Find<WorkLog>(Builders<WorkLog>.Filter.And(filters))
          .As<WorkLogViewModel>()
          .ToListAsync();

        return data;
    }

    public async Task<IReadOnlyCollection<WorkLogViewModel>> SearchWorkLogsForUser(WorkLogSearch search, string userProfileId)
    {
        List<FilterDefinition<WorkLog>> filters = new();
        var builder = Builders<WorkLog>.Filter;

        filters.Add(builder.Eq("UserProfileId", userProfileId));

        if (search.StartDate.HasValue)
        {
            filters.Add(builder.Gte("EventDateTime", search.StartDate.Value));
        }

        if (search.EndDate.HasValue)
        {
            filters.Add(builder.Lte("EventDateTime", search.EndDate.Value));
        }

        if (search.Reason.HasValue)
        {
            filters.Add(builder.Eq("Reason", search.Reason.Value.ToString()));
        }

        int boundedLimit = search.Limit.HasValue && search.Limit.Value > 0
            ? Math.Min(search.Limit.Value, 500)
            : 100;

        var data = await Collection
            .Find<WorkLog>(builder.And(filters))
            .SortByDescending(w => w.EventDateTime)
            .Limit(boundedLimit)
            .As<WorkLogViewModel>()
            .ToListAsync();

        return data;
    }

    protected override IMongoCollection<WorkLog> GetCollection()
    {
        return _unitOfWork.GetCollection<IMongoCollection<WorkLog>, WorkLog>(WORKLOG_COLLECTION_NAME);
    }
}