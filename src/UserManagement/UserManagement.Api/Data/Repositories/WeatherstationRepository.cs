using GardenLog.SharedInfrastructure.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PlantHarvest.Infrastructure.Data.Repositories;

public class WeatherstationRepository : BaseRepository<Weatherstation>, IWeatherstationRepository
{
    private const string WEATHERSTATION_COLLECTION_NAME = "Weatherstation-Collection";
    private readonly ILogger<WeatherstationRepository> _logger;

    public WeatherstationRepository(IUnitOfWork unitOfWork, ILogger<WeatherstationRepository> logger)
        : base(unitOfWork, logger)
    {
        _logger = logger;
    }


    public void AddWeatherstation(Garden garden)
    {
        var weatherstation = garden.Weatherstation;

        var bsonDocument = weatherstation.ToBsonDocument();
        bsonDocument.Set("GardenId", garden.Id);

        this.Add(bsonDocument);
    }

    public void DeleteWeatherstation(string gardenId)
    {
        AddCommand(() => Collection.DeleteManyAsync(Builders<Weatherstation>.Filter.Eq("GardenId", gardenId)));
    }


    public void UpdateWeatherstation(Garden garden)
    {
        var weatherstation = garden.Weatherstation;

        var bsonDocument = weatherstation.ToBsonDocument();
        bsonDocument.Set("GardenId", garden.Id);

        this.Update(bsonDocument);
    }

    protected override IMongoCollection<Weatherstation> GetCollection()
    {
        return _unitOfWork.GetCollection<IMongoCollection<Weatherstation>, Weatherstation>(WEATHERSTATION_COLLECTION_NAME);
    }

    public async Task<Weatherstation> ReadWeatherstation(string gardenId)
    {
        var data = await Collection
           .Find<Weatherstation>(Builders<Weatherstation>.Filter.Eq("GardenId", gardenId))
           .FirstOrDefaultAsync();

       return data;
    }

    public async Task<WeatherstationViewModel> GetWeatherstation(string gardenId)
    {
        var data = await Collection
          .Find<Weatherstation>(Builders<Weatherstation>.Filter.Eq("GardenId", gardenId))
           .As<WeatherstationViewModel>()
          .FirstOrDefaultAsync();

        return data;
    }
}
