using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel;
using GardenLog.SharedKernel.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PlantCatalog.Contract.Base;
using PlantCatalog.Contract.ViewModels;
using PlantCatalog.Domain.PlantAggregate;
using System.Reflection;

namespace PlantCatalog.Infrustructure.Data.Repositories;

public class PlantRepository : BaseRepository<Plant>, IPlantRepository
{
    private const string PLANT_COLLECTION_NAME = "PlantCatalog-Collection";
    private readonly ILogger<PlantRepository> _logger;

    public PlantRepository(IUnitOfWork unitOfWork, ILogger<PlantRepository> logger)
        : base(unitOfWork, logger)
    {
        _logger = logger;
    }

    public async Task<Plant> GetByNameAsync(string plantName)
    {
        var data = await Collection.FindAsync<Plant>(Builders<Plant>.Filter.Eq("Name", plantName));
        return data.FirstOrDefault();
    }

    public async Task<string> GetIdByNameAsync(string plantName)
    {
        var idOnlyProjection = Builders<Plant>.Projection.Include(p => p.Id);

        var data = await Collection
            .Find<Plant>(Builders<Plant>.Filter.Eq("Name", plantName))
            .Project(idOnlyProjection)
            .FirstOrDefaultAsync();

        if (data != null)
        {
            if (data.TryGetValue("_id", out var id))
                return id.ToString()!;
        }
        return string.Empty;
    }

    public async Task<bool> ExistsAsync(string plantId)
    {
        var data = await Collection
            .Find<Plant>(Builders<Plant>.Filter.Eq("_id", plantId))
            .CountDocumentsAsync();
        return data == 1;
    }

    public async Task<IReadOnlyCollection<PlantViewModel>> GetAllPlants()
    {
        var data = await Collection
           .Find<Plant>(Builders<Plant>.Filter.Empty)
           .As<PlantViewModel>()
           .ToListAsync();

        return data;
    }

    public async Task<IReadOnlyCollection<PlantNameOnlyViewModel>> GetAllPlantNames()
    {
        var projection = Builders<Plant>.Projection.Include("Name").Include("Color");

        var data = await Collection
           .Find<Plant>(Builders<Plant>.Filter.Empty)
           .Project(projection)
           .As<PlantNameOnlyViewModel>()
           .ToListAsync();

        return data;
    }

    public async Task<IReadOnlyCollection<PlantGrowInstructionViewModel>> GetPlantGrowInstractions(string plantId)
    {
        var data = await Collection
           .Find<Plant>(Builders<Plant>.Filter.Eq("_id", plantId))
           .Project(Builders<Plant>.Projection.Include(p => p.GrowInstructions))
           .As<PlantGrowInstructionViewModelProjection>()
           .FirstAsync();

        if (data.GrowInstructions?.Count() > 0)
        {
            data.GrowInstructions.ForEach(g => g.PlantId = data._id);

            return data.GrowInstructions;
        }
        else
        {
            return new List<PlantGrowInstructionViewModel>();
        }
    }

    public async Task<PlantGrowInstructionViewModel> GetPlantGrowInstraction(string plantId, string id)
    {
        //var filter = Builders<Plant>.Filter.Eq(x => x.Id, plantId)  
        //    & Builders<Plant>.Filter.ElemMatch(x => x.GrowInstructions, Builders<PlantGrowInstruction>.Filter.Eq(g => g.Id, id));


        var data = await Collection
           .Find<Plant>(Builders<Plant>.Filter.Eq("_id", plantId))
          .Project(Builders<Plant>.Projection.Include(p => p.GrowInstructions))
          .As<PlantGrowInstructionViewModelProjection>()
          .FirstAsync();

        data.GrowInstructions.ForEach(g => g.PlantId = data._id);

        return data.GrowInstructions.First(g => g.PlantGrowInstructionId == id);
    }

    public void AddPlantGrowInstruction(string plantGrowInstructionId, Plant plant)
    {
        var plantFilter = Builders<Plant>.Filter.Eq("_id", plant.Id);
        var update = Builders<Plant>.Update.Push<PlantGrowInstruction>("GrowInstructions", plant.GrowInstructions.First(g => g.Id == plantGrowInstructionId))
            .Set(p => p.GrowInstructionsCount, plant.GrowInstructionsCount)
            .Set(p => p.HarvestSeason, plant.HarvestSeason);

        AddCommand(() => Collection.UpdateOneAsync(plantFilter, update));
    }

    public void UpdatePlantGrowInstruction(string plantGrowInstructionId, Plant plant)
    {
        var filter = Builders<Plant>.Filter.Eq(p => p.Id, plant.Id);
        var update = Builders<Plant>.Update
            .Set("GrowInstructions.$[f]", plant.GrowInstructions.First(g => g.Id == plantGrowInstructionId))
            .Set(p => p.HarvestSeason, plant.HarvestSeason);
        var options = new UpdateOptions()
        {
            ArrayFilters = new List<ArrayFilterDefinition<BsonValue>>()
            {
                new BsonDocument("f._id",
                new BsonDocument("$eq", plantGrowInstructionId))
            }
        };

        AddCommand(() => Collection.UpdateOneAsync(filter, update, options));
    }

    public void DeletePlantGrowInstruction(string plantGrowInstructionid, Plant plant)
    {
        var filter = Builders<Plant>.Filter.Eq(p => p.Id, plant.Id);
        var update = Builders<Plant>.Update.Set(p => p.GrowInstructionsCount, plant.GrowInstructionsCount)
            .PullFilter(p => p.GrowInstructions, Builders<PlantGrowInstruction>.Filter.Eq(p => p.Id, plantGrowInstructionid))
            .Set(p => p.HarvestSeason, plant.HarvestSeason);

        AddCommand(() => Collection.UpdateOneAsync(filter, update));
    }

    protected override IMongoCollection<Plant> GetCollection()
    {
        return _unitOfWork.GetCollection<IMongoCollection<Plant>, Plant>(PLANT_COLLECTION_NAME);
    }

    protected override void OnModelCreating()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Plant)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<Plant>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("plant");
            //p.MapIdMember(c => c.PlantId).SetIdGenerator(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator.Instance);
            //p.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));

            p.MapMember(m => m.Lifecycle).SetSerializer(new EnumSerializer<PlantLifecycleEnum>(BsonType.String));
            p.MapMember(m => m.Type).SetSerializer(new EnumSerializer<PlantTypeEnum>(BsonType.String));
            p.MapMember(m => m.MoistureRequirement).SetSerializer(new EnumSerializer<MoistureRequirementEnum>(BsonType.String));
            p.MapMember(m => m.LightRequirement).SetSerializer(new EnumSerializer<LightRequirementEnum>(BsonType.String));
            // p.MapMember(m => m.GrowTolerance).SetSerializer(new EnumSerializer<GrowToleranceEnum>(BsonType.String));
            p.MapMember(m => m.GrowTolerance).SetSerializer(new EnumToStringArraySerializer<GrowToleranceEnum>());
            p.MapProperty(m => m.Tags).SetDefaultValue(new List<string>());
            p.MapProperty(m => m.VarietyColors).SetDefaultValue(new List<string>());
            p.MapProperty(m => m.GrowInstructions).SetDefaultValue(new List<PlantGrowInstruction>());
            p.MapProperty(m => m.GrowInstructionsCount).SetDefaultValue(0);
            p.MapProperty(m => m.VarietyCount).SetDefaultValue(0);
            p.MapMember(m => m.HarvestSeason).SetSerializer(new EnumToStringArraySerializer<HarvestSeasonEnum>()).SetDefaultValue(HarvestSeasonEnum.Unspecified);
            p.MapProperty(m => m.DaysToMaturityMin).SetDefaultValue(0);
            p.MapProperty(m => m.DaysToMaturityMax).SetDefaultValue(0);

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
                p.UnmapMember(m => m.DomainEvents);
            });
        }

        BsonClassMap.RegisterClassMap<PlantBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.Lifecycle).SetSerializer(new EnumSerializer<PlantLifecycleEnum>(BsonType.String));
            p.MapMember(m => m.Type).SetSerializer(new EnumSerializer<PlantTypeEnum>(BsonType.String));
            p.MapMember(m => m.MoistureRequirement).SetSerializer(new EnumSerializer<MoistureRequirementEnum>(BsonType.String));
            p.MapMember(m => m.LightRequirement).SetSerializer(new EnumSerializer<LightRequirementEnum>(BsonType.String));
            p.MapMember(m => m.GrowTolerance).SetSerializer(new EnumToStringArraySerializer<GrowToleranceEnum>());
            p.MapMember(m => m.HarvestSeason).SetSerializer(new EnumToStringArraySerializer<HarvestSeasonEnum>()).SetDefaultValue(HarvestSeasonEnum.Unspecified);
        });

        BsonClassMap.RegisterClassMap<PlantViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.PlantId).SetElementName("_id");
        });

        BsonClassMap.RegisterClassMap<PlantNameOnlyViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.PlantId).SetElementName("_id");
        });

        BsonClassMap.RegisterClassMap<PlantGrowInstruction>(g =>
        {
            g.AutoMap();
            g.SetIgnoreExtraElements(true);
            g.MapMember(m => m.PlantingDepthInInches).SetSerializer(new EnumSerializer<PlantingDepthEnum>(BsonType.String));
            g.MapMember(m => m.PlantingMethod).SetSerializer(new EnumSerializer<PlantingMethodEnum>(BsonType.String));
            g.MapMember(m => m.StartSeedAheadOfWeatherCondition).SetSerializer(new EnumSerializer<WeatherConditionEnum>(BsonType.String));
            g.MapMember(m => m.HarvestSeason).SetSerializer(new EnumSerializer<HarvestSeasonEnum>(BsonType.String));
            g.MapMember(m => m.TransplantAheadOfWeatherCondition).SetSerializer(new EnumSerializer<WeatherConditionEnum>(BsonType.String));
            g.MapMember(m => m.FertilizerAtPlanting).SetSerializer(new EnumSerializer<FertilizerEnum>(BsonType.String));
            g.MapMember(m => m.FertilizerForSeedlings).SetSerializer(new EnumSerializer<FertilizerEnum>(BsonType.String));
            g.MapMember(m => m.Fertilizer).SetSerializer(new EnumSerializer<FertilizerEnum>(BsonType.String));
        });


        BsonClassMap.RegisterClassMap<PlantGrowInstructionViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.PlantGrowInstructionId).SetElementName("_id");

        });

        BsonClassMap.RegisterClassMap<PlantGrowInstructionBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);

            p.MapMember(m => m.PlantingDepthInInches).SetSerializer(new EnumSerializer<PlantingDepthEnum>(BsonType.String));
            p.MapMember(m => m.PlantingMethod).SetSerializer(new EnumSerializer<PlantingMethodEnum>(BsonType.String));
            p.MapMember(m => m.StartSeedAheadOfWeatherCondition).SetSerializer(new EnumSerializer<WeatherConditionEnum>(BsonType.String));
            p.MapMember(m => m.HarvestSeason).SetSerializer(new EnumSerializer<HarvestSeasonEnum>(BsonType.String));
            p.MapMember(m => m.TransplantAheadOfWeatherCondition).SetSerializer(new EnumSerializer<WeatherConditionEnum>(BsonType.String));
            p.MapMember(m => m.FertilizerAtPlanting).SetSerializer(new EnumSerializer<FertilizerEnum>(BsonType.String));
            p.MapMember(m => m.FertilizerForSeedlings).SetSerializer(new EnumSerializer<FertilizerEnum>(BsonType.String));
            p.MapMember(m => m.Fertilizer).SetSerializer(new EnumSerializer<FertilizerEnum>(BsonType.String));
        });

        BsonClassMap.RegisterClassMap<PlantGrowInstructionViewModelProjection>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);

        });
    }
}

public record PlantGrowInstructionViewModelProjection(string _id, List<PlantGrowInstructionViewModel> GrowInstructions);