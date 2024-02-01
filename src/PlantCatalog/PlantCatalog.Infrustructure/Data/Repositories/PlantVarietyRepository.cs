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
    }

}
