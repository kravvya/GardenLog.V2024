﻿using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel.Interfaces;
using GrowConditions.Contract.Base;
using GrowConditions.Contract.ViewModels;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace GrowConditions.Api.Data.Repositories
{
    public interface IWeatherRepository : IRepository<WeatherUpdate>
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
                .Find(Builders<WeatherUpdate>.Filter.And(filters))
                .As<WeatherUpdateViewModel>()
                .ToListAsync();

            return data;

        }

        public async Task<WeatherUpdateViewModel> GetLastWeatherUpdate(string gardenId)
        {
            var data = await Collection
                .Find(Builders<WeatherUpdate>.Filter.Eq("GardenId", gardenId))
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
    }
}
