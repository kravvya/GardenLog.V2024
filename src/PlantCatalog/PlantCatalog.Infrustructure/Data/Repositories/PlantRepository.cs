using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using PlantCatalog.Contract.ViewModels;
using PlantCatalog.Domain.PlantAggregate;

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
}

public record PlantGrowInstructionViewModelProjection(string _id, List<PlantGrowInstructionViewModel> GrowInstructions);