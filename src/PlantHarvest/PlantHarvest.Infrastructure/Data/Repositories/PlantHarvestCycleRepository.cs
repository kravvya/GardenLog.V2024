using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PlantHarvest.Infrastructure.Data.Repositories;

public class PlantHarvestCycleRepository : BaseRepository<PlantHarvestCycle>, IPlantHarvestCycleRepository
{
    private const string PLANT_HARVEST_COLLECTION_NAME = "PlantHarvestCycle-Collection";
    private readonly ILogger<PlantHarvestCycleRepository> _logger;

    public PlantHarvestCycleRepository(IUnitOfWork unitOfWork, ILogger<PlantHarvestCycleRepository> logger)
        : base(unitOfWork, logger)
    {
        _logger = logger;
    }

    #region Plant Harvest Cycle
    public void AddPlantHarvestCycle(string plantHarvestCyclceId, HarvestCycle harvestCyclce)
    {
        var plantHarvestCycle = harvestCyclce.Plants.First(g => g.Id == plantHarvestCyclceId);

        var bsonDocument = plantHarvestCycle.ToBsonDocument();
        bsonDocument.Set("HarvestCycleId", harvestCyclce.Id);
       
        this.Add(bsonDocument);
    }

    public void DeletePlantHarvestCycle(string harvestCyclceId)
    {
        AddCommand(() => Collection.DeleteManyAsync(Builders<PlantHarvestCycle>.Filter.Eq("HarvestCycleId", harvestCyclceId)));
    }

    public void DeletePlantHarvestCycle(string plantHarvestCyclceId, HarvestCycle harvestCyclce)
    {
        this.Delete(plantHarvestCyclceId);
    }

    public void UpdatePlantHarvestCycle(string plantHarvestCyclceId, HarvestCycle harvestCyclce)
    {
        var plantHarvestCycle = harvestCyclce.Plants.First(g => g.Id == plantHarvestCyclceId);
        var bsonDocument = plantHarvestCycle.ToBsonDocument();
        bsonDocument.Set("HarvestCycleId", harvestCyclce.Id);
        this.Update(bsonDocument);
    }

    public async Task<List<PlantHarvestCycle>> GetPlantHarvestCyclesByHarvestCyleId(string harvestCycleId)
    {
        var data = await Collection
          .FindAsync<PlantHarvestCycle>(Builders<PlantHarvestCycle>.Filter.Eq("HarvestCycleId", harvestCycleId));

        return data.ToList();
    }

    public async Task<PlantHarvestCycleViewModel> GetPlantHarvestCycleView(string harvestCycleId, string id)
    {
        var filter = Builders<PlantHarvestCycle>.Filter.And(
                      Builders<PlantHarvestCycle>.Filter.Eq("HarvestCycleId", harvestCycleId),
                                 Builders<PlantHarvestCycle>.Filter.Eq("_id", id));

        var plant = await Collection
         .Find<PlantHarvestCycle>(filter)
         .As<PlantHarvestCycleViewModel>()
         .FirstAsync();

        plant.PlantCalendar.ForEach(p =>
        {
            p.HarvestCycleId = harvestCycleId;
            p.PlantHarvestCycleId = id;
        });

        return plant;
    }

    public async Task<IReadOnlyCollection<PlantHarvestCycleViewModel>> GetPlantHarvestCycleViews(string harvestCycleId)
    {

        var plants = await Collection
         .Find<PlantHarvestCycle>(Builders<PlantHarvestCycle>.Filter.Eq("HarvestCycleId", harvestCycleId))
         .As<PlantHarvestCycleViewModel>()
         .ToListAsync();

        plants.ForEach(plant =>
        {
            plant.PlantCalendar.ForEach(p =>
            {
                p.HarvestCycleId = harvestCycleId;
                p.PlantHarvestCycleId = plant.PlantHarvestCycleId;
            });
        });

        return plants;
    }


    public async Task<IReadOnlyCollection<PlantHarvestCycleIdentityOnlyViewModel>> GetPlantHarvestCyclesByPlantId(string plantId)
    {
       var response = new List<PlantHarvestCycleIdentityOnlyViewModel>();

        var projection = Builders<BsonDocument>.Projection
            .Include("HarvestCycleId")
            .Include("_id");

        var cursor = await GetBsonCollection()
       .Find<BsonDocument>(Builders<BsonDocument>.Filter.Eq("PlantId", plantId))
       .Project(projection).ToCursorAsync();

        await cursor.ForEachAsync(document =>
        {
            var plantHarvestCycle = new PlantHarvestCycleIdentityOnlyViewModel
            {
                HarvestCycleId = document["HarvestCycleId"].AsString,
                PlantHarvestCycleId = document["_id"].AsString
            };
            response.Add(plantHarvestCycle);
        });

        return response;
    }


    #endregion

    #region Plant Schedule
    public void AddPlantSchedule(string plantCalendarId, string plantHarvestCycleId, HarvestCycle harvestCyclce)
    {
        var schedule = harvestCyclce.Plants.First(g => g.Id == plantHarvestCycleId).PlantCalendar.First(p => p.Id == plantCalendarId);
        var plantFilter = Builders<PlantHarvestCycle>.Filter.Eq("_id", plantHarvestCycleId);
        var update = Builders<PlantHarvestCycle>.Update.Push<PlantSchedule>("PlantCalendar", schedule);

        AddCommand(() => Collection.UpdateOneAsync(plantFilter, update));
    }

    public void DeletePlantSchedule(string plantCalendarId, string plantHarvestCycleId, HarvestCycle harvestCyclce)
    {
        var plantFilter = Builders<PlantHarvestCycle>.Filter.Eq("_id", plantHarvestCycleId);
        var update = Builders<PlantHarvestCycle>.Update.PullFilter(p => p.PlantCalendar, Builders<PlantSchedule>.Filter.Eq(p => p.Id, plantCalendarId));

        AddCommand(() => Collection.UpdateOneAsync(plantFilter, update));
    }

    public void UpdatePlantSchedule(string plantCalendarId, string plantHarvestCycleId, HarvestCycle harvestCyclce)
    {
        var schedule = harvestCyclce.Plants.First(g => g.Id == plantHarvestCycleId).PlantCalendar.First(p => p.Id == plantCalendarId);
        var plantFilter = Builders<PlantHarvestCycle>.Filter.Eq("_id", plantHarvestCycleId);
        var update = Builders<PlantHarvestCycle>.Update.Set("PlantCalendar.$[f]", schedule);
        var options = new UpdateOptions()
        {
            ArrayFilters = new List<ArrayFilterDefinition<BsonValue>>()
            {
                new BsonDocument("f._id",
                new BsonDocument("$eq", plantCalendarId))
            }
        };

        AddCommand(() => Collection.UpdateOneAsync(plantFilter, update, options));
    }

    #endregion

    protected override IMongoCollection<PlantHarvestCycle> GetCollection()
    {
        return _unitOfWork.GetCollection<IMongoCollection<PlantHarvestCycle>, PlantHarvestCycle>(PLANT_HARVEST_COLLECTION_NAME);
    }
}
