using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel.Interfaces;
using MongoDB.Driver;

namespace PlantHarvest.Infrastructure.Data.Repositories;

public class HarvestCycleRepository : BaseRepository<HarvestCycle>, IHarvestCycleRepository
{
    private const string HARVEST_COLLECTION_NAME = "HarvestCycle-Collection";
    private readonly IPlantHarvestCycleRepository _plantHarvestCycleRepository;
    private readonly IGardenBedPlantHarvestCycleRepository _gardenBedPlantHarvestCycleRepository;
    private readonly ILogger<HarvestCycleRepository> _logger;

    public HarvestCycleRepository(IUnitOfWork unitOfWork, IPlantHarvestCycleRepository plantHarvestCycleRepository, IGardenBedPlantHarvestCycleRepository gardenBedPlantHarvestCycleRepository, ILogger<HarvestCycleRepository> logger)
        : base(unitOfWork, logger)
    {
        _plantHarvestCycleRepository = plantHarvestCycleRepository;
        _gardenBedPlantHarvestCycleRepository = gardenBedPlantHarvestCycleRepository;
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

    public async Task<HarvestCycle> ReadHarvestCycle(string harvestCycleId, string plantHarvestCycleId, string userProfileId)
    {
        var filter = Builders<HarvestCycle>.Filter.And(
                      Builders<HarvestCycle>.Filter.Eq("UserProfileId", userProfileId),
                                 Builders<HarvestCycle>.Filter.Eq("_id", harvestCycleId));

        var harvestTask = Collection.Find(filter).SingleOrDefaultAsync();

        var plantsTask = plantHarvestCycleId != null ?
                _plantHarvestCycleRepository.GetPlantHarvestCyclesByHarvestCyleId(harvestCycleId)
                : GetPlantHarvestCycle(plantHarvestCycleId!);

        var gardenBedsTask =  plantHarvestCycleId != null ? 
                _gardenBedPlantHarvestCycleRepository.GetGardenBedsByHarvestCycleId(harvestCycleId)
                : _gardenBedPlantHarvestCycleRepository.GetGardenBedsByHarvestCycleId(harvestCycleId, plantHarvestCycleId);

        await Task.WhenAll(harvestTask, plantsTask, gardenBedsTask);

        var harvestCycle = harvestTask.Result;
        var plants = plantsTask.Result;

        plants.ForEach(plant =>
        {
            var beds = gardenBedsTask.Result.Where(g => g.PlantHarvestCycleId == plant.Id).ToList();
            plant.RehidrateGardenBedPlantHarvestCycles(beds);
        });

        harvestCycle.RehidratePlants(plants);

        return harvestCycle;
    }

    private async Task<List<PlantHarvestCycle>> GetPlantHarvestCycle(string plantHarvestCycleId)
    {
        var plant = await _plantHarvestCycleRepository.GetByIdAsync(plantHarvestCycleId);

        return new List<PlantHarvestCycle> { plant };
    }

    public async Task<HarvestCycle> ReadHarvestCycle(string harvestCycleId, string userProfileId)
    {
        return await ReadHarvestCycle(harvestCycleId, string.Empty, userProfileId); 
    }

    protected override IMongoCollection<HarvestCycle> GetCollection()
    {
        return _unitOfWork.GetCollection<IMongoCollection<HarvestCycle>, HarvestCycle>(HARVEST_COLLECTION_NAME);
    }

}
