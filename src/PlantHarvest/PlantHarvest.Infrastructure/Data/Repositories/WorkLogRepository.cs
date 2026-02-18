using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel.Enum;
using GardenLog.SharedKernel.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PlantHarvest.Contract.Query;

namespace PlantHarvest.Infrastructure.Data.Repositories;

public class WorkLogRepository : BaseRepository<WorkLog>, IWorkLogRepository
{
    private const string WORKLOG_COLLECTION_NAME = "WorkLog-Collection";
    private const string PLANT_HARVEST_COLLECTION_NAME = "PlantHarvestCycle-Collection";
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
        if (string.IsNullOrWhiteSpace(search.PlantId))
        {
            throw new ArgumentException("plantId is required.", nameof(search.PlantId));
        }

        int boundedLimit = search.Limit.HasValue && search.Limit.Value > 0
            ? Math.Min(search.Limit.Value, 500)
            : 100;

        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var workLogSerializer = serializerRegistry.GetSerializer<WorkLog>();

        var matchFilters = new List<FilterDefinition<WorkLog>>
        {
            Builders<WorkLog>.Filter.Eq("UserProfileId", userProfileId)
        };

        if (search.StartDate.HasValue)
        {
            matchFilters.Add(Builders<WorkLog>.Filter.Gte("EventDateTime", search.StartDate.Value));
        }

        if (search.EndDate.HasValue)
        {
            matchFilters.Add(Builders<WorkLog>.Filter.Lte("EventDateTime", search.EndDate.Value));
        }

        if (search.Reason.HasValue)
        {
            matchFilters.Add(Builders<WorkLog>.Filter.Eq("Reason", search.Reason.Value.ToString()));
        }

        var renderedFilters = matchFilters
            .Select(filter => filter.Render(new RenderArgs<WorkLog>(workLogSerializer, serializerRegistry)))
            .ToArray();

        var pipeline = new List<BsonDocument>
        {
            new("$match", new BsonDocument("$and", new BsonArray(renderedFilters)))
        };

        pipeline.Add(
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", PLANT_HARVEST_COLLECTION_NAME },
                { "let", new BsonDocument("relatedEntityIds", "$RelatedEntities.EntityId") },
                {
                    "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$in", new BsonArray { "$_id", "$$relatedEntityIds" }))),
                        new BsonDocument("$match", new BsonDocument("PlantId", search.PlantId))
                    }
                },
                { "as", "MatchedPlantCycles" }
            }));

        pipeline.Add(new BsonDocument("$match", new BsonDocument("MatchedPlantCycles.0", new BsonDocument("$exists", true))));

        pipeline.Add(new BsonDocument("$sort", new BsonDocument("EventDateTime", -1)));
        pipeline.Add(new BsonDocument("$limit", boundedLimit));
        pipeline.Add(new BsonDocument("$project", new BsonDocument("MatchedPlantCycles", 0)));

        var data = await GetBsonCollection()
            .Aggregate<BsonDocument>(pipeline)
            .ToListAsync();

        return data
            .Select(document => BsonSerializer.Deserialize<WorkLogViewModel>(document))
            .ToList();
    }

    protected override IMongoCollection<WorkLog> GetCollection()
    {
        return _unitOfWork.GetCollection<IMongoCollection<WorkLog>, WorkLog>(WORKLOG_COLLECTION_NAME);
    }
}