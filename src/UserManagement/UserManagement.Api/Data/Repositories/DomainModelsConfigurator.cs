using GardenLog.SharedInfrastructure.MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using PlantHarvest.Infrastructure.Data.Repositories;
using System.Reflection;
using UserManagement.Contract.ViewModels;

namespace UserManagement.Api.Data.Repositories;

public class DomainModelsConfigurator : IModelConfigurator
{
    public void OnModelCreating()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Garden)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<BaseEntity>(p =>
        {
            p.AutoMap();
            //p.MapIdMember(c => c.Id).SetIdGenerator(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator.Instance);
            //p.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
            p.SetIgnoreExtraElements(true);
            p.UnmapMember(m => m.DomainEvents);
            p.UnmapMember(m => m.IsModified);
        });

        BsonClassMap.RegisterClassMap<Garden>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("garden");

            var dateSerializer = new DateTimeSerializer(dateOnly: true);

            p.MapProperty(m => m.GardenBeds).SetDefaultValue(new List<GardenBed>());
            p.MapProperty(m => m.LastFrostDate).SetSerializer(dateSerializer);
            p.MapProperty(m => m.FirstFrostDate).SetSerializer(dateSerializer);
            p.MapProperty(m => m.WarmSoilDate).SetSerializer(dateSerializer).SetDefaultValue(DateTime.MinValue);
            p.MapProperty(m => m.Length).SetDefaultValue(0);
            p.MapProperty(m => m.Width).SetDefaultValue(0);
            p.UnmapMember(m => m.Weatherstation);

            var nonPublicCtors = p.ClassType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var longestCtor = nonPublicCtors.OrderByDescending(ctor => ctor.GetParameters().Length).FirstOrDefault();
            p.MapConstructor(longestCtor, p.DeclaredMemberMaps.Where(c => c.ElementName != "Id").Select(c => c.ElementName).ToArray());
        });

        BsonClassMap.RegisterClassMap<GardenBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);

        });

        BsonClassMap.RegisterClassMap<GardenViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.GardenId).SetElementName("_id");
        });

        BsonClassMap.RegisterClassMap<GardenBed>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.Type).SetSerializer(new EnumSerializer<GardenBedTypeEnum>(BsonType.String));
        });

        BsonClassMap.RegisterClassMap<GardenBedViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.GardenBedId).SetElementName("_id");
        });

        BsonClassMap.RegisterClassMap<GardenBedBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.Type).SetSerializer(new EnumSerializer<GardenBedTypeEnum>(BsonType.String));
        });

        BsonClassMap.RegisterClassMap<GardenBedViewModelProjection>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
        });

        BsonClassMap.RegisterClassMap<UserProfile>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("user-profile");

        });

        BsonClassMap.RegisterClassMap<UserProfileBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);

        });

        BsonClassMap.RegisterClassMap<UserProfileViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            //p.MapMember(m => m.UserProfileId).SetElementName("_id");
        });

        BsonClassMap.RegisterClassMap<WeatherstationBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
        });

        BsonClassMap.RegisterClassMap<WeatherstationViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapIdMember(m => m.WeatherstationId);
        });

        BsonClassMap.RegisterClassMap<Weatherstation>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
        });
    }
}
