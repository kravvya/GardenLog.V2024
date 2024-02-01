using GardenLog.SharedInfrastructure.MongoDB;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Reflection;
using System.Xml.Linq;

namespace PlantHarvest.Infrastructure.Data.Repositories;

public class GardenRepository : BaseRepository<Garden>, IGardenRepository
{
    private const string GARDEN_COLLECTION_NAME = "Garden-Collection";
    private readonly ILogger<GardenRepository> _logger;

    public GardenRepository(IUnitOfWork unitOfWork, ILogger<GardenRepository> logger)
        : base(unitOfWork, logger)
    {
        _logger = logger;
    }

    public async Task<GardenViewModel> GetGarden(string gardenId)
    {
        var data = await Collection
            .Find<Garden>(Builders<Garden>.Filter.Eq("_id", gardenId))
            .As<GardenViewModel>()
            .FirstOrDefaultAsync();

        return data;

    }

    public async Task<GardenViewModel> GetGardenByName(string gardenName, string userProfileId)
    {
        List<FilterDefinition<Garden>> filters = [];
        filters.Add(Builders<Garden>.Filter.Eq("Name", gardenName));
        filters.Add(Builders<Garden>.Filter.Eq("UserProfileId", userProfileId));

        var data = await Collection
            .Find<Garden>(Builders<Garden>.Filter.And(filters))
            .As<GardenViewModel>()
            .FirstOrDefaultAsync();


        return data;
    }


    public async Task<IReadOnlyCollection<GardenViewModel>> GetGardens(string userProfileId)
    {
        var data = await Collection
           .Find<Garden>(Builders<Garden>.Filter.Eq("UserProfileId", userProfileId))
           .As<GardenViewModel>()
           .ToListAsync();

        return data;
    }

    public async Task<IReadOnlyCollection<GardenViewModel>> GetAllGardens()
    {
        var data = await Collection
           .Find<Garden>(_ => true)
           .As<GardenViewModel>()
           .ToListAsync();

        return data;
    }

    public async Task<string> GetIdByNameAsync(string name, string userProfileId)
    {
        var idOnlyProjection = Builders<Garden>.Projection.Include(p => p.Id);
        List<FilterDefinition<Garden>> filters = [];
        filters.Add(Builders<Garden>.Filter.Eq("Name", name));
        filters.Add(Builders<Garden>.Filter.Eq("UserProfileId", userProfileId));

        var data = await Collection
            .Find<Garden>(Builders<Garden>.Filter.And(filters))
            .Project(idOnlyProjection)
            .FirstOrDefaultAsync();

        if (data != null)
        {
            if (data.TryGetValue("_id", out var id))
                return id.ToString()!;
        }
        return string.Empty;
    }


    #region Garden Bed

    public void AddGardenBed(string gardenBedId, Garden harvestCyclce)
    {
        var gardenFilter = Builders<Garden>.Filter.Eq("_id", harvestCyclce.Id);
        var update = Builders<Garden>.Update.Push<GardenBed>("GardenBeds", harvestCyclce.GardenBeds.First(g => g.Id == gardenBedId));

        AddCommand(() => Collection.UpdateOneAsync(gardenFilter, update));
    }

    public void DeleteGardenBed(string gardenBedId, Garden garden)
    {
        var gardenFilter = Builders<Garden>.Filter.Eq("_id", garden.Id);
        var update = Builders<Garden>.Update.PullFilter(p => p.GardenBeds, Builders<GardenBed>.Filter.Eq(p => p.Id, gardenBedId));

        AddCommand(() => Collection.UpdateOneAsync(gardenFilter, update));
    }

    public async Task<GardenBedViewModel?> GetGardenBed(string gardenId, string id)
    {
        var data = await Collection
           .Find<Garden>(Builders<Garden>.Filter.Eq("_id", gardenId))
           .Project(Builders<Garden>.Projection.Include(g => g.GardenBeds))
           .As<GardenBedViewModelProjection>()
           .FirstOrDefaultAsync();

        if (data != null)
        {
            data.GardenBeds.ForEach(g => g.GardenId = gardenId);


            return data.GardenBeds.FirstOrDefault(b => b.GardenBedId == id);
        }
        return null;
    }

    public async Task<IReadOnlyCollection<GardenBedViewModel>> GetGardenBeds(string gardenId)
    {
        var data = await Collection
           .Find<Garden>(Builders<Garden>.Filter.Eq("_id", gardenId))
           .Project(Builders<Garden>.Projection.Include(g => g.GardenBeds))
           .As<GardenBedViewModelProjection>()
           .FirstOrDefaultAsync();

        if (data != null)
        {
            data.GardenBeds.ForEach(g => g.GardenId = gardenId);

            return data.GardenBeds;
        }
        else
        {
            return new List<GardenBedViewModel>();
        }
    }

    public void UpdateGardenBed(string gardenBedId, Garden garden)
    {
        var gardenFilter = Builders<Garden>.Filter.Eq("_id", garden.Id);
        var update = Builders<Garden>.Update.Set("GardenBeds.$[f]", garden.GardenBeds.First(g => g.Id == gardenBedId));
        var options = new UpdateOptions()
        {
            ArrayFilters = new List<ArrayFilterDefinition<BsonValue>>()
            {
                new BsonDocument("f._id",
                new BsonDocument("$eq", gardenBedId))
            }
        };

        AddCommand(() => Collection.UpdateOneAsync(gardenFilter, update, options));
    }

    #endregion

    protected override IMongoCollection<Garden> GetCollection()
    {
        return _unitOfWork.GetCollection<IMongoCollection<Garden>, Garden>(GARDEN_COLLECTION_NAME);
    }
}

public record GardenBedViewModelProjection(string Id, List<GardenBedViewModel> GardenBeds);