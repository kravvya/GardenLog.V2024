using GardenLog.SharedInfrastructure.MongoDB;
using GrowConditions.Contract.Base;
using GrowConditions.Contract.ViewModels;
using MongoDB.Bson.Serialization;

namespace GrowConditions.Api.Data.Repositories;

public class DomainModelsConfigurator : IModelConfigurator
{
    public void OnModelCreating()
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
