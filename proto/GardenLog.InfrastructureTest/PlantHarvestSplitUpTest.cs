using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PlantHarvest.Contract.Enum;
using System.Reflection;
using Xunit.Abstractions;

namespace GardenLog.InfrastructureTest;

public class PlantHarvestSplitUpTest
{
    private readonly ITestOutputHelper _output;

    public PlantHarvestSplitUpTest(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void ReadPlantHarvestFromStore()
    {
        var client = new MongoClient("mongodb+srv://<user>:<password>Mplant123@gardenlog2023-cluster.rln8w5k.mongodb.net/");
        var database = client.GetDatabase("GardenLog-Db");

        BsonClassMap.RegisterClassMap<BaseEntity>(p =>
        {
            p.AutoMap();
            p.SetIgnoreExtraElements(true);
        });

        BsonClassMap.RegisterClassMap<HarvestCycle>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("harvest-cycle");
        });

        BsonClassMap.RegisterClassMap<PlantHarvestCycle>(g =>
        {
            g.AutoMap();
          
            g.SetIgnoreExtraElements(true);
            g.MapMember(m => m.PlantingMethod).SetSerializer(new EnumSerializer<PlantingMethodEnum>(BsonType.String));

            g.MapProperty(m => m.PlantCalendar).SetDefaultValue(new List<PlantSchedule>());
            g.MapProperty(m => m.SpacingInInches).SetDefaultValue(0);
            g.MapProperty(m => m.PlantsPerFoot).SetDefaultValue(0.0);

        });

        BsonClassMap.RegisterClassMap<PlantSchedule>(g =>
        {
            g.AutoMap();
            g.SetIgnoreExtraElements(true);
            g.MapMember(m => m.TaskType).SetSerializer(new EnumSerializer<WorkLogReasonEnum>(BsonType.String));
        });

        BsonClassMap.RegisterClassMap<GardenBedPlantHarvestCycle>(g =>
        {
            g.AutoMap();
            g.SetIgnoreExtraElements(true);
        });

        BsonClassMap.RegisterClassMap<HarvestCycleDocument>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("harvest-cycle");
        });

        BsonClassMap.RegisterClassMap<PlantHarvestCycleDocument>(g =>
        {
            g.AutoMap();

            g.SetIgnoreExtraElements(true);
            g.MapMember(m => m.PlantingMethod).SetSerializer(new EnumSerializer<PlantingMethodEnum>(BsonType.String));

            g.MapProperty(m => m.PlantCalendar).SetDefaultValue(new List<PlantSchedule>());
            g.MapProperty(m => m.SpacingInInches).SetDefaultValue(0);
            g.MapProperty(m => m.PlantsPerFoot).SetDefaultValue(0.0);

        });

        // Get references to collections
        var gardenCollection = database.GetCollection<GardenBedPlantHarvestCycle>("GardenBedUsage-Collection");
        var plantCollection = database.GetCollection<PlantHarvestCycleDocument>("PlantHarvestCycle-Collection");
        var harvestCollection = database.GetCollection<HarvestCycleDocument>("HarvestCycle-Collection");
        
        var plantHarvestCollection = database.GetCollection<HarvestCycle>("PlantHarvest-Collection");

        var filter = Builders<HarvestCycle>.Filter.Empty;
        var harvests = plantHarvestCollection.Find<HarvestCycle>(filter).ToList();

        foreach(var harvest in harvests)
        {
            _output.WriteLine(harvest.HarvestCycleName);
            foreach(var plant in harvest.Plants)
            {
                plant.HarvestCycleId = harvest.Id;

                if(!plant.LastHarvestDate.HasValue && harvest.EndDate.HasValue)
                {
                    plant.LastHarvestDate = harvest.EndDate;
                }              
              
                _output.WriteLine($" {plant.PlantName} - {plant.LastHarvestDate} ");

                foreach(var bed in plant.GardenBedLayout)
                {
                    bed.PlantHarvestCycleId = plant.Id;
                    bed.HarvestCycleId = harvest.Id;
                    if(plant.PlantingMethod == PlantingMethodEnum.DirectSeed && plant.SeedingDate.HasValue)                       
                    {
                        bed.StartDate = plant.SeedingDate;
                    }
                    if (plant.PlantingMethod == PlantingMethodEnum.Transplanting && plant.TransplantDate.HasValue)
                    {
                        bed.StartDate = plant.TransplantDate;
                    }
                    if (plant.LastHarvestDate.HasValue)
                    {
                        bed.EndDate = plant.LastHarvestDate;
                    }
                    _output.WriteLine($"{bed.StartDate} - {bed.EndDate}");
                    //save bed
                    gardenCollection.InsertOne( bed );
                   
                }
                //save plant
                plant.GardenBedLayout.Clear();
                PlantHarvestCycleDocument plantDocument = new();
             //copy every attribute from plant to plantDocument including PlantCalendar
             foreach (PropertyInfo property in plant.GetType().GetProperties())
                {
                    plantDocument.GetType().GetProperty(property.Name)?.SetValue(plantDocument, property.GetValue(plant));
                }
           plantDocument.PlantCalendar = plant.PlantCalendar;

                plantCollection.InsertOne(plantDocument);
               
            }
            //save harvest
            HarvestCycleDocument harvestDocument = new();
            //copy every attribute from harvest to harvestDocument
            foreach (PropertyInfo property in harvest.GetType().GetProperties())
            {
                harvestDocument.GetType().GetProperty(property.Name)?.SetValue(harvestDocument, property.GetValue(harvest));
            }
            harvestCollection.InsertOne(harvestDocument);
        }
    }

}
