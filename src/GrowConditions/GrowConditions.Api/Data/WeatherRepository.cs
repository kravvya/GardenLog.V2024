using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel.Interfaces;
using GrowConditions.Contract.Base;
using GrowConditions.Contract.ViewModels;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace GrowConditions.Api.Data
{
    public interface IWeatherRepository: IRepository<WeatherUpdate>
    {
        Task<WeatherUpdateViewModel> GetLastWeatherUpdate(string gardenId);
        Task<IList<WeatherUpdateViewModel>> GetHistoryOfWeatherUpdates(string gardenId, int numberOfDays);
        
    }

    public class WeatherRepository : BaseRepository<WeatherUpdate>, IWeatherRepository
    {
        private const string WEATHER_COLLECTION_NAME = "Weather-Collection";
        private readonly ILogger<WeatherRepository> _logger;

        public WeatherRepository(IUnitOfWork unitOfWork, ILogger<WeatherRepository> logger) 
            : base(unitOfWork, logger)
        {
            _logger = logger;
        }
               

        public async Task<IList<WeatherUpdateViewModel>> GetHistoryOfWeatherUpdates(string gardenId, int numberOfDays)
        {
            DateTime date = DateTime.UtcNow.AddDays(-1 * numberOfDays);

            List<FilterDefinition<WeatherUpdate>> filters = new();
            filters.Add(Builders<WeatherUpdate>.Filter.Gte("UpdatedDateUtc", date));
            filters.Add(Builders<WeatherUpdate>.Filter.Eq("GardenId", gardenId));

            var data = await Collection
                .Find<WeatherUpdate>(Builders<WeatherUpdate>.Filter.And(filters))
                .As<WeatherUpdateViewModel>()
                .ToListAsync();

            return data;

        }

        public async Task<WeatherUpdateViewModel> GetLastWeatherUpdate(string gardenId)
        {
            var data = await Collection
                .Find<WeatherUpdate>(Builders<WeatherUpdate>.Filter.Eq("GardenId", gardenId))
                .SortByDescending(f => f.UpdatedDateUtc)
                .Limit(1)
                .As<WeatherUpdateViewModel>()
                .FirstOrDefaultAsync();

            return data;
        }

        protected override IMongoCollection<WeatherUpdate> GetCollection()
        {
            return _unitOfWork.GetCollection<IMongoCollection<WeatherUpdate>, WeatherUpdate>(WEATHER_COLLECTION_NAME);
        }

        protected override void OnModelCreating()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(WeatherUpdate)))
            {
                return;
            }

            BsonClassMap.RegisterClassMap<WeatherUpdate>(p =>
            {
                p.AutoMap();
                //ignore elements not int he document 
                p.SetIgnoreExtraElements(true);
                p.SetDiscriminator("weather");             

            });

            BsonClassMap.RegisterClassMap<WeatherUpdateBase>(p =>
            {
                p.AutoMap();
                //ignore elements not int he document 
                p.SetIgnoreExtraElements(true);

            });

            BsonClassMap.RegisterClassMap<WeatherUpdateViewModel>(p =>
            {
                p.AutoMap();
                //ignore elements not int he document 
                p.SetIgnoreExtraElements(true);
                p.MapMember(m => m.WeatherId).SetElementName("_id");

            });
        }
    }
}
