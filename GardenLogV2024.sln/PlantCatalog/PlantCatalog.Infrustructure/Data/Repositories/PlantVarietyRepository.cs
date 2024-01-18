using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PlantCatalog.Contract.Base;
using PlantCatalog.Contract.ViewModels;
using PlantCatalog.Domain.PlantAggregate;
using System.Reflection;

namespace PlantCatalog.Infrustructure.Data.Repositories
{
    public class PlantVarietyRepository : BaseRepository<PlantVariety>, IPlantVarietyRepository
    {
        private const string PLANT_VARIETY_COLLECTION_NAME = "PlantVarietyCatalog-Collection";
        private readonly ILogger<PlantVarietyRepository> _logger;

        public PlantVarietyRepository(IUnitOfWork unitOfWork, ILogger<PlantVarietyRepository> logger)
            : base(unitOfWork, logger)
        {
            _logger = logger;
        }

        public async Task<PlantVariety> GetByNameAsync(string plantId, string plantName)
        {
            var builder = Builders<PlantVariety>.Filter;
            var filter = builder.Eq("Name", plantName) & builder.Eq("PlantId", plantId);

            var data = await Collection.FindAsync<PlantVariety>(filter);
            return data.FirstOrDefault();
        }

        public async Task<string> GetIdByNameAsync(string plantId, string plantName)
        {
            var idOnlyProjection = Builders<PlantVariety>.Projection.Include(p => p.Id);
            var builder = Builders<PlantVariety>.Filter;
            var filter = builder.Eq("Name", plantName) & builder.Eq("PlantId", plantId);

            var data = await Collection
                .Find<PlantVariety>(filter)
                .Project(idOnlyProjection)
                .FirstOrDefaultAsync();

            if (data != null)
            {
                if (data.TryGetValue("_id", out var id))
                    return id.ToString()!;
            }
            return string.Empty;
        }

        public async Task<IReadOnlyCollection<PlantVarietyViewModel>> GetPlantVarieties(string plantId)
        {
            var data = await Collection
               .Find<PlantVariety>(Builders<PlantVariety>.Filter.Eq("PlantId", plantId))
               .As<PlantVarietyViewModel>()
               .ToListAsync();

            return data;
        }

        public async Task<IReadOnlyCollection<PlantVarietyViewModel>> GetPlantVarieties()
        {
            var data = await Collection
               .Find<PlantVariety>(Builders<PlantVariety>.Filter.Empty)
               .As<PlantVarietyViewModel>()
               .ToListAsync();

            return data;
        }

        public async Task<long> GetCountOfPlantVarieties(string plantId)
        {
            var data = await Collection
               .Find<PlantVariety>(Builders<PlantVariety>.Filter.Eq("PlantId", plantId))
                .CountDocumentsAsync();

            return data;
        }

        protected override IMongoCollection<PlantVariety> GetCollection()
        {
            return _unitOfWork.GetCollection<IMongoCollection<PlantVariety>, PlantVariety>(PLANT_VARIETY_COLLECTION_NAME);
        }

        protected override void OnModelCreating()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(PlantVariety)))
            {
                return;
            }

            BsonClassMap.RegisterClassMap<PlantVariety>(p =>
            {
                p.AutoMap();
                //ignore elements not int he document 
                p.SetIgnoreExtraElements(true);
                p.SetDiscriminator("plantVariety");

                p.MapMember(m => m.MoistureRequirement).SetSerializer(new EnumSerializer<MoistureRequirementEnum>(BsonType.String));
                p.MapMember(m => m.LightRequirement).SetSerializer(new EnumSerializer<LightRequirementEnum>(BsonType.String));
                p.MapMember(m => m.GrowTolerance).SetSerializer(new EnumToStringArraySerializer<GrowToleranceEnum>());
                p.MapProperty(m => m.Tags).SetDefaultValue(new List<string>());
                p.MapProperty(m => m.Colors).SetDefaultValue(new List<string>());
                p.MapProperty(m => m.Sources).SetDefaultValue(new List<string>());

                var nonPublicCtors = p.ClassType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                var longestCtor = nonPublicCtors.OrderByDescending(ctor => ctor.GetParameters().Length).FirstOrDefault();
                p.MapConstructor(longestCtor, p.ClassType.GetProperties().Where(c => c.Name != "Id").Select(c => c.Name).ToArray());
            });

            BsonClassMap.RegisterClassMap<PlantVarietyViewModel>(p =>
            {
                p.AutoMap();
                //ignore elements not in the document 
                p.SetIgnoreExtraElements(true);
                p.MapMember(m => m.PlantVarietyId).SetElementName("_id");

            });

            BsonClassMap.RegisterClassMap<PlantVarietyBase>(p =>
            {
                p.AutoMap();
                //ignore elements not int he document 
                p.SetIgnoreExtraElements(true);

                p.MapMember(m => m.MoistureRequirement).SetSerializer(new EnumSerializer<MoistureRequirementEnum>(BsonType.String));
                p.MapMember(m => m.LightRequirement).SetSerializer(new EnumSerializer<LightRequirementEnum>(BsonType.String));
                p.MapMember(m => m.GrowTolerance).SetSerializer(new EnumToStringArraySerializer<GrowToleranceEnum>());
                p.MapProperty(m => m.Tags).SetDefaultValue(new List<string>());
                p.MapProperty(m => m.Colors).SetDefaultValue(new List<string>());
                p.MapProperty(m => m.Sources).SetDefaultValue(new List<string>());
            });
        }

    }

}
