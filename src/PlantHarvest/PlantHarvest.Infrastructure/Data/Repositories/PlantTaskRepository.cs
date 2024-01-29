using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel;
using GardenLog.SharedKernel.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using PlantHarvest.Contract.Query;

namespace PlantHarvest.Infrastructure.Data.Repositories;

public class PlantTaskRepository : BaseRepository<PlantTask>, IPlantTaskRepository
{
    private const string TASK_COLLECTION_NAME = "PlantTask-Collection";
    private readonly ILogger<PlantTaskRepository> _logger;

    public PlantTaskRepository(IUnitOfWork unitOfWork, ILogger<PlantTaskRepository> logger)
        : base(unitOfWork, logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<PlantTaskViewModel>> GetActivePlantTasksForUser(string userProfileId)
    {
        List<FilterDefinition<PlantTask>> filters = new();
        filters.Add(Builders<PlantTask>.Filter.Eq("CompletedDateTime", BsonNull.Value));
        filters.Add(Builders<PlantTask>.Filter.Eq("UserProfileId", userProfileId));

        var data = await Collection
        .Find<PlantTask>(Builders<PlantTask>.Filter.And(filters))
        .As<PlantTaskViewModel>()
        .ToListAsync();

        return data;

    }

    public async Task<IReadOnlyCollection<PlantTaskViewModel>> GetPlantTasksForUser(string userProfileId)
    {
        var data = await Collection
        .Find<PlantTask>(Builders<PlantTask>.Filter.Eq("UserProfileId", userProfileId))
        .As<PlantTaskViewModel>()
        .ToListAsync();

        return data;
    }

    public async Task<long> GetNumberOfCompletedTasksForUser(string userProfileId, string harvestCycelId)
    {
        List<FilterDefinition<PlantTask>> filters = new();
        filters.Add(Builders<PlantTask>.Filter.Ne("CompletedDateTime", BsonNull.Value));
        filters.Add(Builders<PlantTask>.Filter.Eq("HarvestCycleId", harvestCycelId));
        filters.Add(Builders<PlantTask>.Filter.Eq("UserProfileId", userProfileId));

        var documentCount = await Collection
       .Find<PlantTask>(Builders<PlantTask>.Filter.And(filters))
       .CountDocumentsAsync();

        return documentCount;
    }

    public async Task<IReadOnlyCollection<PlantTaskViewModel>> SearchPlantTasksForUser(PlantTaskSearch search, string userProfileId)
    {
        List<FilterDefinition<PlantTask>> filters = new();
        var builder = Builders<PlantTask>.Filter;

        if (!string.IsNullOrEmpty(search.PlantHarvestCycleId))
        {
            filters.Add(builder.Eq("PlantHarvestCycleId", search.PlantHarvestCycleId));
        }
        if (search.Reason.HasValue)
        {
            filters.Add(builder.Eq("Type", search.Reason.Value.ToString()));
        }
       
        if (search.IsPastDue)
        {
            filters.Add(builder.Lte("TargetDateStart", DateTime.Now));
            filters.Add(builder.Eq("CompletedDateTime", BsonNull.Value));
        }
        else
        {
            if (!search.IncludeResolvedTasks)
            {
                filters.Add(builder.Eq("CompletedDateTime", BsonNull.Value));
            }
            if (search.DueInNumberOfDays.HasValue)
            {
                filters.Add(builder.Lte("TargetDateStart", DateTime.Now.AddDays(search.DueInNumberOfDays.Value)));
            }
        }


        filters.Add(builder.Eq("UserProfileId", userProfileId));

        var data = await Collection
        .Find<PlantTask>(builder.And(filters))
        .As<PlantTaskViewModel>()
        .ToListAsync();

        return data;

    }

    protected override IMongoCollection<PlantTask> GetCollection()
    {
        return _unitOfWork.GetCollection<IMongoCollection<PlantTask>, PlantTask>(TASK_COLLECTION_NAME);
    }

    protected override void OnModelCreating()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(PlantTask)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<PlantTask>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("plant_task");

            p.MapMember(m => m.Type).SetSerializer(new EnumSerializer<WorkLogReasonEnum>(BsonType.String));
        });

        if (!BsonClassMap.IsClassMapRegistered(typeof(BaseEntity)))
        {
            BsonClassMap.RegisterClassMap<BaseEntity>(p =>
            {
                p.AutoMap();
                //p.MapIdMember(c => c.Id).SetIdGenerator(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator.Instance);
                //p.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                p.SetIgnoreExtraElements(true);
                p.UnmapMember(m => m.DomainEvents);
            });
        }

        BsonClassMap.RegisterClassMap<PlantTaskBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.Type).SetSerializer(new EnumSerializer<WorkLogReasonEnum>(BsonType.String));
        });

        BsonClassMap.RegisterClassMap<PlantTaskViewModel>(p =>
        {
            p.AutoMap();
            p.MapIdField(m => m.PlantTaskId);
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            //p.MapMember(m => m.PlantTaskId).SetElementName("_id");
            p.MapIdField(m =>  m.PlantTaskId);
        });
    }

}