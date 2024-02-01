using GardenLog.SharedInfrastructure.MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace ImageCatalog.Api.Data;

public class DomainModelsConfigurator : IModelConfigurator
{
    public void OnModelCreating()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Model.Image)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<Model.Image>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("image");
            //p.MapIdMember(c => c.PlantId).SetIdGenerator(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator.Instance);
            //p.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
            p.MapMember(m => m.RelatedEntityType).SetSerializer(new EnumSerializer<RelatedEntityTypEnum>(BsonType.String));
        });


        BsonClassMap.RegisterClassMap<BaseEntity>(p =>
        {
            p.AutoMap();
            //p.MapIdMember(c => c.Id).SetIdGenerator(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator.Instance);
            //p.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
            p.UnmapMember(m => m.DomainEvents);
        });


        BsonClassMap.RegisterClassMap<ImageBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.RelatedEntityType).SetSerializer(new EnumSerializer<RelatedEntityTypEnum>(BsonType.String));
        });

        BsonClassMap.RegisterClassMap<ImageViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.ImageId).SetElementName("_id");
        });
    }
}
