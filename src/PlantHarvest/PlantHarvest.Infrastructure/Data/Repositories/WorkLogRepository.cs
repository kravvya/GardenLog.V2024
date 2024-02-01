using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel;
using GardenLog.SharedKernel.Enum;
using GardenLog.SharedKernel.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

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

    protected override IMongoCollection<WorkLog> GetCollection()
    {
        return _unitOfWork.GetCollection<IMongoCollection<WorkLog>, WorkLog>(WORKLOG_COLLECTION_NAME);
    }
}