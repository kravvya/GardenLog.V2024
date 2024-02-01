using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel;
using GardenLog.SharedKernel.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Reflection;

namespace PlantHarvest.Infrastructure.Data.Repositories;

public  class DomainModelsConfigurator: IModelConfigurator
{
    public void OnModelCreating()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(HarvestCycle)))
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
        });


        #region Harvest Cycle
        BsonClassMap.RegisterClassMap<HarvestCycle>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("harvest-cycle");

            // p.MapProperty(m => m.Plants).SetDefaultValue(new List<PlantHarvestCycle>());
            p.UnmapMember(m => m.Plants);

            var nonPublicCtors = p.ClassType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var longestCtor = nonPublicCtors.OrderByDescending(ctor => ctor.GetParameters().Length).FirstOrDefault();
            p.MapConstructor(longestCtor, p.ClassType.GetProperties().Where(c => c.Name != "Id" && c.Name != "Plants").Select(c => c.Name).ToArray());

        });

        BsonClassMap.RegisterClassMap<HarvestCycleBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);

        });

        BsonClassMap.RegisterClassMap<HarvestCycleViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapIdMember(m => m.HarvestCycleId);
        });
        #endregion

        #region Plant Harvest Cycle
        BsonClassMap.RegisterClassMap<PlantHarvestCycle>(g =>
        {
            g.AutoMap();
            g.UnmapMember(m => m.GardenBedLayout);
            g.SetIgnoreExtraElements(true);
            g.MapMember(m => m.PlantingMethod).SetSerializer(new EnumSerializer<PlantingMethodEnum>(BsonType.String));

            g.MapProperty(m => m.PlantCalendar).SetDefaultValue(new List<PlantSchedule>());
            //g.MapProperty(m => m.GardenBedLayout).SetDefaultValue(new List<GardenBedPlantHarvestCycle>());
            g.MapProperty(m => m.SpacingInInches).SetDefaultValue(0);
            g.MapProperty(m => m.PlantsPerFoot).SetDefaultValue(0.0);


            var nonPublicCtors = g.ClassType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var longestCtor = nonPublicCtors.OrderByDescending(ctor => ctor.GetParameters().Length).FirstOrDefault();
            g.MapConstructor(longestCtor, g.ClassType.GetProperties().Where(c => c.Name != "Id" && c.Name != "GardenBedLayout").Select(c => c.Name).ToArray());

        });


        BsonClassMap.RegisterClassMap<PlantHarvestCycleViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.PlantHarvestCycleId).SetElementName("_id");

        });

        BsonClassMap.RegisterClassMap<PlantHarvestCycleBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.PlantingMethod).SetSerializer(new EnumSerializer<PlantingMethodEnum>(BsonType.String));
        });
        #endregion

        #region Plant Schedule
        BsonClassMap.RegisterClassMap<PlantSchedule>(g =>
        {
            g.AutoMap();
            g.SetIgnoreExtraElements(true);
            g.MapMember(m => m.TaskType).SetSerializer(new EnumSerializer<WorkLogReasonEnum>(BsonType.String));
        });


        BsonClassMap.RegisterClassMap<PlantScheduleViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.PlantScheduleId).SetElementName("_id");

        });

        BsonClassMap.RegisterClassMap<PlantScheduleBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.TaskType).SetSerializer(new EnumSerializer<WorkLogReasonEnum>(BsonType.String));
        });


        #endregion

        #region Garden Bed Layout
        BsonClassMap.RegisterClassMap<GardenBedPlantHarvestCycle>(g =>
        {
            g.AutoMap();
            g.SetIgnoreExtraElements(true);
        });


        BsonClassMap.RegisterClassMap<GardenBedPlantHarvestCycleViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.GardenBedPlantHarvestCycleId).SetElementName("_id");
        });

        BsonClassMap.RegisterClassMap<GardenBedPlantHarvestCycleBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
        });


        #endregion

        #region PlantTask
        BsonClassMap.RegisterClassMap<PlantTask>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("plant_task");

            p.MapMember(m => m.Type).SetSerializer(new EnumSerializer<WorkLogReasonEnum>(BsonType.String));
        });

        BsonClassMap.RegisterClassMap<PlantTaskBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.Type).SetSerializer(new EnumSerializer<WorkLogReasonEnum>(BsonType.String));
        });

        BsonClassMap.RegisterClassMap<PlantTaskViewModel>(p =>
        {
            p.AutoMap();
            p.MapIdField(m => m.PlantTaskId);
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            //p.MapMember(m => m.PlantTaskId).SetElementName("_id");
            p.MapIdField(m => m.PlantTaskId);
        });
        #endregion

        #region WorkLog
        BsonClassMap.RegisterClassMap<WorkLog>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("work-log");

            p.MapMember(m => m.Reason).SetSerializer(new EnumSerializer<WorkLogReasonEnum>(BsonType.String));
        });


        BsonClassMap.RegisterClassMap<WorkLogBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.Reason).SetSerializer(new EnumSerializer<WorkLogReasonEnum>(BsonType.String));
        });

        BsonClassMap.RegisterClassMap<WorkLogViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.WorkLogId).SetElementName("_id");
        });

        BsonClassMap.RegisterClassMap<RelatedEntity>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            p.MapMember(m => m.EntityType).SetSerializer(new EnumSerializer<RelatedEntityTypEnum>(BsonType.String));
        });
        #endregion

    }
}