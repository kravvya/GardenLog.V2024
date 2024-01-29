using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel;
using GardenLog.SharedKernel.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PlantHarvest.Contract.Enum;
using System.Reflection;

namespace PlantHarvest.Infrastructure.Data.Repositories;

public class HarvestCycleRepository : BaseRepository<HarvestCycle>, IHarvestCycleRepository
{
    private const string HARVEST_COLLECTION_NAME = "PlantHarvest-Collection";
    private readonly ILogger<HarvestCycleRepository> _logger;

    public HarvestCycleRepository(IUnitOfWork unitOfWork, ILogger<HarvestCycleRepository> logger)
        : base(unitOfWork, logger)
    {
        _logger = logger;
    }

    public async Task<string> GetIdByNameAsync(string harvestCycleName, string userProfileId)
    {
        var idOnlyProjection = Builders<HarvestCycle>.Projection.Include(p => p.Id);

        List<FilterDefinition<HarvestCycle>> filters = new();
        filters.Add(Builders<HarvestCycle>.Filter.Eq("HarvestCycleName", harvestCycleName));
        filters.Add(Builders<HarvestCycle>.Filter.Eq("UserProfileId", userProfileId));

        var data = await Collection
            .Find<HarvestCycle>(Builders<HarvestCycle>.Filter.And(filters))
            .Project(idOnlyProjection)
            .FirstOrDefaultAsync();

        if (data != null)
        {
            if (data.TryGetValue("_id", out var id))
                return id.ToString()!;
        }
        return string.Empty;
    }

    public async Task<IReadOnlyCollection<HarvestCycleViewModel>> GetAllHarvestCycles(string userProfileId)
    {
        var data = await Collection
          .Find<HarvestCycle>(Builders<HarvestCycle>.Filter.Eq("UserProfileId", userProfileId))
          .As<HarvestCycleViewModel>()
          .ToListAsync();

        return data;
    }

    #region Plant Harvest Cycle
    public void AddPlantHarvestCycle(string plantHarvestCyclceId, HarvestCycle harvestCyclce)
    {
        var harvestFilter = Builders<HarvestCycle>.Filter.Eq("_id", harvestCyclce.Id);
        var update = Builders<HarvestCycle>.Update.Push<PlantHarvestCycle>("Plants", harvestCyclce.Plants.First(g => g.Id == plantHarvestCyclceId));

        AddCommand(() => Collection.UpdateOneAsync(harvestFilter, update));
    }

    public void DeletePlantHarvestCycle(string plantHarvestCyclceId, HarvestCycle harvestCyclce)
    {
        var harvestFilter = Builders<HarvestCycle>.Filter.Eq("_id", harvestCyclce.Id);
        var update = Builders<HarvestCycle>.Update.PullFilter(p => p.Plants, Builders<PlantHarvestCycle>.Filter.Eq(p => p.Id, plantHarvestCyclceId));

        AddCommand(() => Collection.UpdateOneAsync(harvestFilter, update));
    }

    public void UpdatePlantHarvestCycle(string plantHarvestCyclceId, HarvestCycle harvestCyclce)
    {
        var harvestFilter = Builders<HarvestCycle>.Filter.Eq("_id", harvestCyclce.Id);
        var update = Builders<HarvestCycle>.Update.Set("Plants.$[f]", harvestCyclce.Plants.First(g => g.Id == plantHarvestCyclceId));
        var options = new UpdateOptions()
        {
            ArrayFilters = new List<ArrayFilterDefinition<BsonValue>>()
            {
                new BsonDocument("f._id",
                new BsonDocument("$eq", plantHarvestCyclceId))
            }
        };

        AddCommand(() => Collection.UpdateOneAsync(harvestFilter, update, options));
    }

    public async Task<PlantHarvestCycleViewModel> GetPlantHarvestCycle(string harvestCycleId, string id)
    {
        var data = await Collection
          .Find<HarvestCycle>(Builders<HarvestCycle>.Filter.Eq("_id", harvestCycleId))
         .Project(Builders<HarvestCycle>.Projection.Include(p => p.Plants))
         .As<PlantHarvestCycleViewModelProjection>()
         .FirstAsync();

        data.Plants.ForEach(g =>
            {
                g.HarvestCycleId = data._id;
                g.PlantCalendar.ForEach(p =>
                {
                    p.HarvestCycleId = data._id;
                    p.PlantHarvestCycleId = g.PlantHarvestCycleId;
                });
                g.GardenBedLayout.ForEach(p =>
                {
                    p.HarvestCycleId = data._id;
                    p.PlantHarvestCycleId = g.PlantHarvestCycleId;
                });
            }
        );

        return data.Plants.First(p => p.PlantHarvestCycleId == id);
    }

    public async Task<IReadOnlyCollection<PlantHarvestCycleIdentityOnlyViewModel>> GetPlantHarvestCyclesByPlantId(string plantId)
    {
        List<PlantHarvestCycleIdentityOnlyViewModel> response = new();

        var filter = Builders<HarvestCycle>.Filter.Eq("Plants.PlantId", plantId);
        var projection = Builders<HarvestCycle>.Projection.Include("Plants._id");

        var data = await Collection
        .Find<HarvestCycle>(filter)
         .Project<HarvestCycle>(projection)
         .As<PlantHarvestCycleViewModelProjection>()
         .ToListAsync();

        foreach (var item in data)
        {
            foreach (var p in item.Plants.Where(p => p.PlantId == plantId))
            {
                response.Add(new PlantHarvestCycleIdentityOnlyViewModel()
                {
                    HarvestCycleId = item._id,
                    PlantHarvestCycleId = p.PlantHarvestCycleId,
                });
            }
        }

        return response;
    }

    public async Task<IReadOnlyCollection<PlantHarvestCycleViewModel>> GetPlantHarvestCycles(string harvestCycleId)
    {
        var data = await Collection
       .Find<HarvestCycle>(Builders<HarvestCycle>.Filter.Eq("_id", harvestCycleId))
       .Project(Builders<HarvestCycle>.Projection.Include(p => p.Plants))
       .As<PlantHarvestCycleViewModelProjection>()
       .FirstAsync();

        data.Plants.ForEach(g =>
            {
                g.HarvestCycleId = data._id;
                g.PlantCalendar.ForEach(p =>
                {
                    p.HarvestCycleId = data._id;
                    p.PlantHarvestCycleId = g.PlantHarvestCycleId;
                });
                g.GardenBedLayout.ForEach(p =>
                {
                    p.HarvestCycleId = data._id;
                    p.PlantHarvestCycleId = g.PlantHarvestCycleId;
                });
            });

        return data.Plants;
    }
    #endregion

    protected override IMongoCollection<HarvestCycle> GetCollection()
    {
        return _unitOfWork.GetCollection<IMongoCollection<HarvestCycle>, HarvestCycle>(HARVEST_COLLECTION_NAME);
    }

    protected override void OnModelCreating()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(HarvestCycle)))
        {
            return;
        }

        #region Harvest Cycle
        BsonClassMap.RegisterClassMap<HarvestCycle>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("harvest-cycle");

            p.MapProperty(m => m.Plants).SetDefaultValue(new List<PlantHarvestCycle>());

            var nonPublicCtors = p.ClassType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var longestCtor = nonPublicCtors.OrderByDescending(ctor => ctor.GetParameters().Length).FirstOrDefault();
            p.MapConstructor(longestCtor, p.ClassType.GetProperties().Where(c => c.Name != "Id").Select(c => c.Name).ToArray());

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

        BsonClassMap.RegisterClassMap<HarvestCycleBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);

        });

        BsonClassMap.RegisterClassMap<HarvestCycleViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapIdMember(m => m.HarvestCycleId);
        });

        #endregion

        #region Plant Harvest Cycle
        BsonClassMap.RegisterClassMap<PlantHarvestCycle>(g =>
        {
            g.AutoMap();
            g.SetIgnoreExtraElements(true);
            g.MapMember(m => m.PlantingMethod).SetSerializer(new EnumSerializer<PlantingMethodEnum>(BsonType.String));

            g.MapProperty(m => m.PlantCalendar).SetDefaultValue(new List<PlantSchedule>());
            g.MapProperty(m => m.GardenBedLayout).SetDefaultValue(new List<GardenBedPlantHarvestCycle>());
            g.MapProperty(m => m.SpacingInInches).SetDefaultValue(0);
            g.MapProperty(m => m.PlantsPerFoot).SetDefaultValue(0.0);

            var nonPublicCtors = g.ClassType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var longestCtor = nonPublicCtors.OrderByDescending(ctor => ctor.GetParameters().Length).FirstOrDefault();
            g.MapConstructor(longestCtor, g.ClassType.GetProperties().Where(c => c.Name != "Id").Select(c => c.Name).ToArray());

        });


        BsonClassMap.RegisterClassMap<PlantHarvestCycleViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.PlantHarvestCycleId).SetElementName("_id");

        });

        BsonClassMap.RegisterClassMap<PlantHarvestCycleBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.PlantingMethod).SetSerializer(new EnumSerializer<PlantingMethodEnum>(BsonType.String));
        });

        BsonClassMap.RegisterClassMap<PlantHarvestCycleViewModelProjection>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);

        });
        #endregion

        #region Plant Schedule
        BsonClassMap.RegisterClassMap<PlantSchedule>(g =>
        {
            g.AutoMap();
            g.SetIgnoreExtraElements(true);
            g.MapMember(m => m.TaskType).SetSerializer(new EnumSerializer<WorkLogReasonEnum>(BsonType.String));
        });


        BsonClassMap.RegisterClassMap<PlantScheduleViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.PlantScheduleId).SetElementName("_id");

        });

        BsonClassMap.RegisterClassMap<PlantScheduleBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.TaskType).SetSerializer(new EnumSerializer<WorkLogReasonEnum>(BsonType.String));
        });


        #endregion

        #region Garden Bed Layout
        BsonClassMap.RegisterClassMap<GardenBedPlantHarvestCycle>(g =>
        {
            g.AutoMap();
            g.SetIgnoreExtraElements(true);
        });


        BsonClassMap.RegisterClassMap<GardenBedPlantHarvestCycleViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.GardenBedPlantHarvestCycleId).SetElementName("_id");
        });

        BsonClassMap.RegisterClassMap<GardenBedPlantHarvestCycleBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
        });


        #endregion
    }


}

public record PlantHarvestCycleViewModelProjection(string _id, List<PlantHarvestCycleViewModel> Plants);