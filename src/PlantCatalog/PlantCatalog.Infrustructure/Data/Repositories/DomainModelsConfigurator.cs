using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using PlantCatalog.Contract.Base;
using PlantCatalog.Contract.ViewModels;
using PlantCatalog.Domain.PlantAggregate;
using System.Reflection;

namespace PlantCatalog.Infrustructure.Data.Repositories;

public class DomainModelsConfigurator : IModelConfigurator
{
    public void OnModelCreating()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Plant)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<Plant>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("plant");
            //p.MapIdMember(c => c.PlantId).SetIdGenerator(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator.Instance);
            //p.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));

            p.MapMember(m => m.Lifecycle).SetSerializer(new EnumSerializer<PlantLifecycleEnum>(BsonType.String));
            p.MapMember(m => m.Type).SetSerializer(new EnumSerializer<PlantTypeEnum>(BsonType.String));
            p.MapMember(m => m.MoistureRequirement).SetSerializer(new EnumSerializer<MoistureRequirementEnum>(BsonType.String));
            p.MapMember(m => m.LightRequirement).SetSerializer(new EnumSerializer<LightRequirementEnum>(BsonType.String));
            // p.MapMember(m => m.GrowTolerance).SetSerializer(new EnumSerializer<GrowToleranceEnum>(BsonType.String));
            p.MapMember(m => m.GrowTolerance).SetSerializer(new EnumToStringArraySerializer<GrowToleranceEnum>());
            p.MapProperty(m => m.Tags).SetDefaultValue(new List<string>());
            p.MapProperty(m => m.VarietyColors).SetDefaultValue(new List<string>());
            p.MapProperty(m => m.GrowInstructions).SetDefaultValue(new List<PlantGrowInstruction>());
            p.MapProperty(m => m.GrowInstructionsCount).SetDefaultValue(0);
            p.MapProperty(m => m.VarietyCount).SetDefaultValue(0);
            p.MapMember(m => m.HarvestSeason).SetSerializer(new EnumToStringArraySerializer<HarvestSeasonEnum>()).SetDefaultValue(HarvestSeasonEnum.Unspecified);
            p.MapProperty(m => m.DaysToMaturityMin).SetDefaultValue(0);
            p.MapProperty(m => m.DaysToMaturityMax).SetDefaultValue(0);

            var nonPublicCtors = p.ClassType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var longestCtor = nonPublicCtors.OrderByDescending(ctor => ctor.GetParameters().Length).FirstOrDefault();
            p.MapConstructor(longestCtor, p.ClassType.GetProperties().Where(c => c.Name != "Id").Select(c => c.Name).ToArray());

        });


        BsonClassMap.RegisterClassMap<BaseEntity>(p =>
        {
            p.AutoMap();
            //p.MapIdMember(c => c.Id).SetIdGenerator(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator.Instance);
            //p.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
            p.UnmapMember(m => m.DomainEvents);
        });


        BsonClassMap.RegisterClassMap<PlantBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.Lifecycle).SetSerializer(new EnumSerializer<PlantLifecycleEnum>(BsonType.String));
            p.MapMember(m => m.Type).SetSerializer(new EnumSerializer<PlantTypeEnum>(BsonType.String));
            p.MapMember(m => m.MoistureRequirement).SetSerializer(new EnumSerializer<MoistureRequirementEnum>(BsonType.String));
            p.MapMember(m => m.LightRequirement).SetSerializer(new EnumSerializer<LightRequirementEnum>(BsonType.String));
            p.MapMember(m => m.GrowTolerance).SetSerializer(new EnumToStringArraySerializer<GrowToleranceEnum>());
            p.MapMember(m => m.HarvestSeason).SetSerializer(new EnumToStringArraySerializer<HarvestSeasonEnum>()).SetDefaultValue(HarvestSeasonEnum.Unspecified);
        });

        BsonClassMap.RegisterClassMap<PlantViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.PlantId).SetElementName("_id");
        });

        BsonClassMap.RegisterClassMap<PlantNameOnlyViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.PlantId).SetElementName("_id");
        });

        BsonClassMap.RegisterClassMap<PlantGrowInstruction>(g =>
        {
            g.AutoMap();
            g.SetIgnoreExtraElements(true);
            g.MapMember(m => m.PlantingDepthInInches).SetSerializer(new EnumSerializer<PlantingDepthEnum>(BsonType.String));
            g.MapMember(m => m.PlantingMethod).SetSerializer(new EnumSerializer<PlantingMethodEnum>(BsonType.String));
            g.MapMember(m => m.StartSeedAheadOfWeatherCondition).SetSerializer(new EnumSerializer<WeatherConditionEnum>(BsonType.String));
            g.MapMember(m => m.HarvestSeason).SetSerializer(new EnumSerializer<HarvestSeasonEnum>(BsonType.String));
            g.MapMember(m => m.TransplantAheadOfWeatherCondition).SetSerializer(new EnumSerializer<WeatherConditionEnum>(BsonType.String));
            g.MapMember(m => m.FertilizerAtPlanting).SetSerializer(new EnumSerializer<FertilizerEnum>(BsonType.String));
            g.MapMember(m => m.FertilizerForSeedlings).SetSerializer(new EnumSerializer<FertilizerEnum>(BsonType.String));
            g.MapMember(m => m.Fertilizer).SetSerializer(new EnumSerializer<FertilizerEnum>(BsonType.String));
        });


        BsonClassMap.RegisterClassMap<PlantGrowInstructionViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.PlantGrowInstructionId).SetElementName("_id");

        });

        BsonClassMap.RegisterClassMap<PlantGrowInstructionBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);

            p.MapMember(m => m.PlantingDepthInInches).SetSerializer(new EnumSerializer<PlantingDepthEnum>(BsonType.String));
            p.MapMember(m => m.PlantingMethod).SetSerializer(new EnumSerializer<PlantingMethodEnum>(BsonType.String));
            p.MapMember(m => m.StartSeedAheadOfWeatherCondition).SetSerializer(new EnumSerializer<WeatherConditionEnum>(BsonType.String));
            p.MapMember(m => m.HarvestSeason).SetSerializer(new EnumSerializer<HarvestSeasonEnum>(BsonType.String));
            p.MapMember(m => m.TransplantAheadOfWeatherCondition).SetSerializer(new EnumSerializer<WeatherConditionEnum>(BsonType.String));
            p.MapMember(m => m.FertilizerAtPlanting).SetSerializer(new EnumSerializer<FertilizerEnum>(BsonType.String));
            p.MapMember(m => m.FertilizerForSeedlings).SetSerializer(new EnumSerializer<FertilizerEnum>(BsonType.String));
            p.MapMember(m => m.Fertilizer).SetSerializer(new EnumSerializer<FertilizerEnum>(BsonType.String));
        });

        BsonClassMap.RegisterClassMap<PlantGrowInstructionViewModelProjection>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);

        });

        BsonClassMap.RegisterClassMap<PlantVariety>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("plantVariety");

            p.MapMember(m => m.MoistureRequirement).SetSerializer(new EnumSerializer<MoistureRequirementEnum>(BsonType.String));
            p.MapMember(m => m.LightRequirement).SetSerializer(new EnumSerializer<LightRequirementEnum>(BsonType.String));
            p.MapMember(m => m.GrowTolerance).SetSerializer(new EnumToStringArraySerializer<GrowToleranceEnum>());
            p.MapProperty(m => m.Tags).SetDefaultValue(new List<string>());
            p.MapProperty(m => m.Colors).SetDefaultValue(new List<string>());
            p.MapProperty(m => m.Sources).SetDefaultValue(new List<string>());

            var nonPublicCtors = p.ClassType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var longestCtor = nonPublicCtors.OrderByDescending(ctor => ctor.GetParameters().Length).FirstOrDefault();
            p.MapConstructor(longestCtor, p.ClassType.GetProperties().Where(c => c.Name != "Id").Select(c => c.Name).ToArray());
        });

        BsonClassMap.RegisterClassMap<PlantVarietyViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.PlantVarietyId).SetElementName("_id");

        });

        BsonClassMap.RegisterClassMap<PlantVarietyBase>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);

            p.MapMember(m => m.MoistureRequirement).SetSerializer(new EnumSerializer<MoistureRequirementEnum>(BsonType.String));
            p.MapMember(m => m.LightRequirement).SetSerializer(new EnumSerializer<LightRequirementEnum>(BsonType.String));
            p.MapMember(m => m.GrowTolerance).SetSerializer(new EnumToStringArraySerializer<GrowToleranceEnum>());
            p.MapProperty(m => m.Tags).SetDefaultValue(new List<string>());
            p.MapProperty(m => m.Colors).SetDefaultValue(new List<string>());
            p.MapProperty(m => m.Sources).SetDefaultValue(new List<string>());
        });
    }
}
