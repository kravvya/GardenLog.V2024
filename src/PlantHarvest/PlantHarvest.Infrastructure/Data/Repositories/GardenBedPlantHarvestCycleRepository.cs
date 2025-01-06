using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PlantHarvest.Infrastructure.Data.Repositories;

public class GardenBedPlantHarvestCycleRepository : BaseRepository<GardenBedPlantHarvestCycle>, IGardenBedPlantHarvestCycleRepository
{
    private const string GARDEN_BED_USAGE_COLLECTION_NAME = "GardenBedUsage-Collection";
    private readonly ILogger<GardenBedPlantHarvestCycleRepository> _logger;

    public GardenBedPlantHarvestCycleRepository(IUnitOfWork unitOfWork, ILogger<GardenBedPlantHarvestCycleRepository> logger)
        : base(unitOfWork, logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<GardenBedPlantHarvestCycle>> GetGardenBedsByHarvestCycleId(string harvestCycleId)
    {
        var data = await Collection
          .FindAsync<GardenBedPlantHarvestCycle>(Builders<GardenBedPlantHarvestCycle>.Filter.Eq("HarvestCycleId", harvestCycleId));

        return data.ToList();
    }

    public async Task<IReadOnlyCollection<GardenBedPlantHarvestCycle>> GetGardenBedsByHarvestCycleId(string harvestCycleId, string? plantHarvestCycleId)
    {
        var filter = Builders<GardenBedPlantHarvestCycle>.Filter.And(
                     Builders<GardenBedPlantHarvestCycle>.Filter.Eq("HarvestCycleId", harvestCycleId),
                                Builders<GardenBedPlantHarvestCycle>.Filter.Eq("PlantHarvestCycleId", plantHarvestCycleId));

        var data = await Collection
           .FindAsync<GardenBedPlantHarvestCycle>(filter);

        return data.ToList();
    }


    public async Task<IReadOnlyCollection<GardenBedPlantHarvestCycleViewModel>> GetGardenBedViewsByPlantHarvestCycleId(string harvestCycleId, string plantHarvestCycleId)
    {
        var filter = Builders<GardenBedPlantHarvestCycle>.Filter.And(
                     Builders<GardenBedPlantHarvestCycle>.Filter.Eq("HarvestCycleId", harvestCycleId),
                                Builders<GardenBedPlantHarvestCycle>.Filter.Eq("PlantHarvestCycleId", plantHarvestCycleId));

        var beds = await Collection
         .Find<GardenBedPlantHarvestCycle>(filter)
         .As<GardenBedPlantHarvestCycleViewModel>()
         .ToListAsync();

        return beds;
    }

    public async Task<IReadOnlyCollection<GardenBedPlantHarvestCycleViewModel>> GetGardenBedViewsByHarvestCycleId(string harvestCycleId)
    {
        var beds = await Collection
         .Find<GardenBedPlantHarvestCycle>(Builders<GardenBedPlantHarvestCycle>.Filter.Eq("HarvestCycleId", harvestCycleId))
         .As<GardenBedPlantHarvestCycleViewModel>()
         .ToListAsync();

        return beds;
    }

    public async Task<IReadOnlyCollection<GardenBedPlantHarvestCycleViewModel>> GetGardenBedViewsByGardenBedId(string gardenId, string gardenBedId)
    {
        var filter = Builders<GardenBedPlantHarvestCycle>.Filter.And(
                   Builders<GardenBedPlantHarvestCycle>.Filter.Eq("GardenId", gardenId),
                              Builders<GardenBedPlantHarvestCycle>.Filter.Eq("GardenBedId", gardenBedId));

        var beds = await Collection
         .Find<GardenBedPlantHarvestCycle>(filter)
         .As<GardenBedPlantHarvestCycleViewModel>()
         .ToListAsync();

        return beds;
    }


    public void AddGardenBedPlantHarvestCycle(string gardenBedPlantHarvestCycleId, string plantHarvestCyclceId, HarvestCycle harvestCyclce)
    {
        var gardenBed = harvestCyclce.Plants.First(g => g.Id == plantHarvestCyclceId).GardenBedLayout.First(b => b.Id == gardenBedPlantHarvestCycleId);
        var bsonDocument = gardenBed.ToBsonDocument();
        bsonDocument.Set("HarvestCycleId", harvestCyclce.Id);

        this.Add(bsonDocument);
    }

    public void DeleteGardenBedPlantHarvestCycle(string harvestCyclceId)
    {
        AddCommand(() => Collection.DeleteManyAsync(Builders<GardenBedPlantHarvestCycle>.Filter.Eq("HarvestCycleId", harvestCyclceId)));
    }

    public void DeleteGardenBedPlantHarvestCycle(string plantHarvestCyclceId, HarvestCycle harvestCyclce)
    {
        AddCommand(() => Collection.DeleteManyAsync(Builders<GardenBedPlantHarvestCycle>.Filter.Eq("PlantHarvestCycleId", plantHarvestCyclceId)));
    }

    public void DeleteGardenBedPlantHarvestCycle(string gardenBedPlantHarvestCycleId, string plantHarvestCyclceId, HarvestCycle harvestCyclce)
    {
        this.Delete(gardenBedPlantHarvestCycleId);
    }

    public void UpdateGardenBedPlantHarvestCycle(string gardenBedPlantHarvestCycleId, string plantHarvestCyclceId, HarvestCycle harvestCyclce)
    {
        var gardenBed = harvestCyclce.Plants.First(g => g.Id == plantHarvestCyclceId).GardenBedLayout.First(b => b.Id == gardenBedPlantHarvestCycleId);
        var bsonDocument = gardenBed.ToBsonDocument();
        bsonDocument.Set("HarvestCycleId", harvestCyclce.Id);

        
        this.Update(bsonDocument);
    }


    protected override IMongoCollection<GardenBedPlantHarvestCycle> GetCollection()
    {
        return _unitOfWork.GetCollection<IMongoCollection<GardenBedPlantHarvestCycle>, GardenBedPlantHarvestCycle>(GARDEN_BED_USAGE_COLLECTION_NAME);
    }

   
}