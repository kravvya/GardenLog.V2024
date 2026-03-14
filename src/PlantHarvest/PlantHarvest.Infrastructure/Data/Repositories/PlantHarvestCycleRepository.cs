using GardenLog.SharedInfrastructure.MongoDB;
using GardenLog.SharedKernel.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PlantHarvest.Contract.Query;
using System.Text.RegularExpressions;

namespace PlantHarvest.Infrastructure.Data.Repositories;

public class PlantHarvestCycleRepository : BaseRepository<PlantHarvestCycle>, IPlantHarvestCycleRepository
{
    private const string PLANT_HARVEST_COLLECTION_NAME = "PlantHarvestCycle-Collection";
    private const string HARVEST_COLLECTION_NAME = "HarvestCycle-Collection";
    private readonly ILogger<PlantHarvestCycleRepository> _logger;

    public PlantHarvestCycleRepository(IUnitOfWork unitOfWork, ILogger<PlantHarvestCycleRepository> logger)
        : base(unitOfWork, logger)
    {
        _logger = logger;
    }

    #region Plant Harvest Cycle
    public void AddPlantHarvestCycle(string plantHarvestCyclceId, HarvestCycle harvestCyclce)
    {
        var plantHarvestCycle = harvestCyclce.Plants.First(g => g.Id == plantHarvestCyclceId);

        var bsonDocument = plantHarvestCycle.ToBsonDocument();
        bsonDocument.Set("HarvestCycleId", harvestCyclce.Id);
       
        this.Add(bsonDocument);
    }

    public void DeletePlantHarvestCycle(string harvestCyclceId)
    {
        AddCommand(() => Collection.DeleteManyAsync(Builders<PlantHarvestCycle>.Filter.Eq("HarvestCycleId", harvestCyclceId)));
    }

    public void DeletePlantHarvestCycle(string plantHarvestCyclceId, HarvestCycle harvestCyclce)
    {
        this.Delete(plantHarvestCyclceId);
    }

    public void UpdatePlantHarvestCycle(string plantHarvestCyclceId, HarvestCycle harvestCyclce)
    {
        var plantHarvestCycle = harvestCyclce.Plants.First(g => g.Id == plantHarvestCyclceId);
        var bsonDocument = plantHarvestCycle.ToBsonDocument();
        bsonDocument.Set("HarvestCycleId", harvestCyclce.Id);
        this.Update(bsonDocument);
    }

    public async Task<List<PlantHarvestCycle>> GetPlantHarvestCyclesByHarvestCyleId(string harvestCycleId)
    {
        var data = await Collection
          .FindAsync<PlantHarvestCycle>(Builders<PlantHarvestCycle>.Filter.Eq("HarvestCycleId", harvestCycleId));

        return data.ToList();
    }

    public async Task<PlantHarvestCycleViewModel> GetPlantHarvestCycleView(string harvestCycleId, string id)
    {
        var filter = Builders<PlantHarvestCycle>.Filter.And(
                      Builders<PlantHarvestCycle>.Filter.Eq("HarvestCycleId", harvestCycleId),
                                 Builders<PlantHarvestCycle>.Filter.Eq("_id", id));

        var plant = await Collection
         .Find<PlantHarvestCycle>(filter)
         .As<PlantHarvestCycleViewModel>()
         .FirstAsync();

        plant.PlantCalendar.ForEach(p =>
        {
            p.HarvestCycleId = harvestCycleId;
            p.PlantHarvestCycleId = id;
        });

        return plant;
    }

    public async Task<IReadOnlyCollection<PlantHarvestCycleViewModel>> GetPlantHarvestCycleViews(string harvestCycleId)
    {

        var plants = await Collection
         .Find<PlantHarvestCycle>(Builders<PlantHarvestCycle>.Filter.Eq("HarvestCycleId", harvestCycleId))
         .As<PlantHarvestCycleViewModel>()
         .ToListAsync();

        plants.ForEach(plant =>
        {
            plant.PlantCalendar.ForEach(p =>
            {
                p.HarvestCycleId = harvestCycleId;
                p.PlantHarvestCycleId = plant.PlantHarvestCycleId;
            });
        });

        return plants;
    }


    public async Task<IReadOnlyCollection<PlantHarvestCycleIdentityOnlyViewModel>> GetPlantHarvestCyclesByPlantId(string plantId)
    {
       var response = new List<PlantHarvestCycleIdentityOnlyViewModel>();

        var projection = Builders<BsonDocument>.Projection
            .Include("HarvestCycleId")
            .Include("_id");

        var cursor = await GetBsonCollection()
       .Find<BsonDocument>(Builders<BsonDocument>.Filter.Eq("PlantId", plantId))
       .Project(projection).ToCursorAsync();

        await cursor.ForEachAsync(document =>
        {
            var plantHarvestCycle = new PlantHarvestCycleIdentityOnlyViewModel
            {
                HarvestCycleId = document["HarvestCycleId"].AsString,
                PlantHarvestCycleId = document["_id"].AsString
            };
            response.Add(plantHarvestCycle);
        });

        return response;
    }

    public async Task<IReadOnlyCollection<PlantHarvestCycleViewModel>> SearchPlantHarvestCyclesForUser(PlantHarvestCycleSearch search, string userProfileId)
    {
        List<FilterDefinition<PlantHarvestCycle>> filters = new();
        var builder = Builders<PlantHarvestCycle>.Filter;

        if (!string.IsNullOrWhiteSpace(search.PlantId))
        {
            filters.Add(builder.Eq("PlantId", search.PlantId));
        }

        if (!string.IsNullOrWhiteSpace(search.HarvestCycleId))
        {
            filters.Add(builder.Eq("HarvestCycleId", search.HarvestCycleId));
        }

        if (search.StartDate.HasValue)
        {
            filters.Add(builder.Gte("SeedingDate", search.StartDate.Value));
        }

        if (search.EndDate.HasValue)
        {
            filters.Add(builder.Lte("LastHarvestDate", search.EndDate.Value));
        }

        if (search.MinGerminationRate.HasValue)
        {
            filters.Add(builder.Gte("GerminationRate", search.MinGerminationRate.Value));
        }

        int boundedLimit = search.Limit.HasValue && search.Limit.Value > 0
            ? Math.Min(search.Limit.Value, 500)
            : 100;

        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var plantSerializer = serializerRegistry.GetSerializer<PlantHarvestCycle>();

        var pipeline = new List<BsonDocument>();

        if (filters.Count > 0)
        {
            var renderedFilters = filters
                .Select(filter => filter.Render(new RenderArgs<PlantHarvestCycle>(plantSerializer, serializerRegistry)))
                .ToArray();

            pipeline.Add(new BsonDocument("$match", new BsonDocument("$and", new BsonArray(renderedFilters))));
        }

        pipeline.Add(
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", HARVEST_COLLECTION_NAME },
                { "let", new BsonDocument("cycleId", "$HarvestCycleId") },
                {
                    "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$eq", new BsonArray { "$_id", "$$cycleId" }))),
                        new BsonDocument("$match", new BsonDocument("UserProfileId", userProfileId))
                    }
                },
                { "as", "AuthorizedHarvestCycle" }
            }));

        pipeline.Add(new BsonDocument("$match", new BsonDocument("AuthorizedHarvestCycle.0", new BsonDocument("$exists", true))));
        pipeline.Add(new BsonDocument("$sort", new BsonDocument("SeedingDate", -1)));
        pipeline.Add(new BsonDocument("$limit", boundedLimit));
        pipeline.Add(new BsonDocument("$project", new BsonDocument("AuthorizedHarvestCycle", 0)));

        var documents = await GetBsonCollection()
            .Aggregate<BsonDocument>(pipeline)
            .ToListAsync();

        var plants = documents
            .Select(document => BsonSerializer.Deserialize<PlantHarvestCycleViewModel>(document))
            .ToList();

        plants.ForEach(plant =>
        {
            plant.PlantCalendar.ForEach(schedule =>
            {
                schedule.HarvestCycleId = plant.HarvestCycleId;
                schedule.PlantHarvestCycleId = plant.PlantHarvestCycleId;
            });
        });

        return plants;
    }

    public async Task<IReadOnlyCollection<PlantHarvestCycleSummaryViewModel>> SearchPlantHarvestCycleSummariesForUser(PlantHarvestCycleSummarySearch search, string userProfileId)
    {
        var builder = Builders<PlantHarvestCycle>.Filter;
        List<FilterDefinition<PlantHarvestCycle>> filters = new();

        if (!string.IsNullOrWhiteSpace(search.PlantId))
        {
            filters.Add(builder.Eq("PlantId", search.PlantId));
        }

        if (!string.IsNullOrWhiteSpace(search.HarvestCycleId))
        {
            filters.Add(builder.Eq("HarvestCycleId", search.HarvestCycleId));
        }

        if (!string.IsNullOrWhiteSpace(search.PlantName))
        {
            var plantNamePattern = Regex.Escape(search.PlantName.Trim());
            filters.Add(builder.Regex("PlantName", new BsonRegularExpression(plantNamePattern, "i")));
        }

        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var plantSerializer = serializerRegistry.GetSerializer<PlantHarvestCycle>();

        var pipeline = new List<BsonDocument>();

        if (filters.Count > 0)
        {
            var renderedFilters = filters
                .Select(filter => filter.Render(new RenderArgs<PlantHarvestCycle>(plantSerializer, serializerRegistry)))
                .ToArray();

            pipeline.Add(new BsonDocument("$match", new BsonDocument("$and", new BsonArray(renderedFilters))));
        }

        var harvestLookupPipeline = new BsonArray
        {
            new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$eq", new BsonArray { "$_id", "$$cycleId" }))),
            new BsonDocument("$match", new BsonDocument("UserProfileId", userProfileId))
        };

        if (!string.IsNullOrWhiteSpace(search.HarvestCycleName))
        {
            harvestLookupPipeline.Add(new BsonDocument("$match", new BsonDocument("HarvestCycleName", new BsonRegularExpression(Regex.Escape(search.HarvestCycleName.Trim()), "i"))));
        }

        pipeline.Add(
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", HARVEST_COLLECTION_NAME },
                { "let", new BsonDocument("cycleId", "$HarvestCycleId") },
                { "pipeline", harvestLookupPipeline },
                { "as", "AuthorizedHarvestCycle" }
            }));

        pipeline.Add(new BsonDocument("$match", new BsonDocument("AuthorizedHarvestCycle.0", new BsonDocument("$exists", true))));
        pipeline.Add(new BsonDocument("$addFields", new BsonDocument
        {
            { "HarvestCycleName", new BsonDocument("$arrayElemAt", new BsonArray { "$AuthorizedHarvestCycle.HarvestCycleName", 0 }) },
            { "NormalizedNotes", new BsonDocument("$trim", new BsonDocument("input", new BsonDocument("$ifNull", new BsonArray { "$Notes", string.Empty }))) }
        }));
        pipeline.Add(new BsonDocument("$group", new BsonDocument
        {
            {
                "_id",
                new BsonDocument
                {
                    { "PlantId", "$PlantId" },
                    { "PlantGrowthInstructionId", "$PlantGrowthInstructionId" },
                    { "HarvestCycleId", "$HarvestCycleId" }
                }
            },
            { "PlantName", new BsonDocument("$first", "$PlantName") },
            { "PlantGrowthInstructionName", new BsonDocument("$first", "$PlantGrowthInstructionName") },
            { "HarvestCycleName", new BsonDocument("$first", "$HarvestCycleName") },
            { "EarliestSeedingDate", new BsonDocument("$min", "$SeedingDate") },
            { "EarliestTransplantDate", new BsonDocument("$min", "$TransplantDate") },
            { "EarliestHarvestDate", new BsonDocument("$min", "$FirstHarvestDate") },
            { "LatestHarvestDate", new BsonDocument("$max", "$LastHarvestDate") },
            { "FeedbackNotes", new BsonDocument("$addToSet", "$NormalizedNotes") }
        }));
        pipeline.Add(new BsonDocument("$project", new BsonDocument
        {
            { "_id", 0 },
            { "PlantId", "$_id.PlantId" },
            { "PlantName", "$PlantName" },
            { "PlantGrowthInstructionId", "$_id.PlantGrowthInstructionId" },
            { "PlantGrowthInstructionName", "$PlantGrowthInstructionName" },
            { "HarvestCycleId", "$_id.HarvestCycleId" },
            { "HarvestCycleName", "$HarvestCycleName" },
            { "EarliestSeedingDate", "$EarliestSeedingDate" },
            { "EarliestTransplantDate", "$EarliestTransplantDate" },
            { "EarliestHarvestDate", "$EarliestHarvestDate" },
            { "LatestHarvestDate", "$LatestHarvestDate" },
            {
                "FeedbackNotes",
                new BsonDocument("$filter", new BsonDocument
                {
                    { "input", "$FeedbackNotes" },
                    { "as", "note" },
                    { "cond", new BsonDocument("$ne", new BsonArray { "$$note", string.Empty }) }
                })
            }
        }));
        pipeline.Add(new BsonDocument("$sort", new BsonDocument
        {
            { "LatestHarvestDate", -1 },
            { "EarliestHarvestDate", -1 },
            { "HarvestCycleName", 1 },
            { "PlantName", 1 },
            { "PlantGrowthInstructionName", 1 }
        }));

        var documents = await GetBsonCollection()
            .Aggregate<BsonDocument>(pipeline)
            .ToListAsync();

        return documents
            .Select(document => BsonSerializer.Deserialize<PlantHarvestCycleSummaryViewModel>(document))
            .ToList();
    }


    #endregion

    #region Plant Schedule
    public void AddPlantSchedule(string plantCalendarId, string plantHarvestCycleId, HarvestCycle harvestCyclce)
    {
        var schedule = harvestCyclce.Plants.First(g => g.Id == plantHarvestCycleId).PlantCalendar.First(p => p.Id == plantCalendarId);
        var plantFilter = Builders<PlantHarvestCycle>.Filter.Eq("_id", plantHarvestCycleId);
        var update = Builders<PlantHarvestCycle>.Update.Push<PlantSchedule>("PlantCalendar", schedule);

        AddCommand(() => Collection.UpdateOneAsync(plantFilter, update));
    }

    public void DeletePlantSchedule(string plantCalendarId, string plantHarvestCycleId, HarvestCycle harvestCyclce)
    {
        var plantFilter = Builders<PlantHarvestCycle>.Filter.Eq("_id", plantHarvestCycleId);
        var update = Builders<PlantHarvestCycle>.Update.PullFilter(p => p.PlantCalendar, Builders<PlantSchedule>.Filter.Eq(p => p.Id, plantCalendarId));

        AddCommand(() => Collection.UpdateOneAsync(plantFilter, update));
    }

    public void UpdatePlantSchedule(string plantCalendarId, string plantHarvestCycleId, HarvestCycle harvestCyclce)
    {
        var schedule = harvestCyclce.Plants.First(g => g.Id == plantHarvestCycleId).PlantCalendar.First(p => p.Id == plantCalendarId);
        var plantFilter = Builders<PlantHarvestCycle>.Filter.Eq("_id", plantHarvestCycleId);
        var update = Builders<PlantHarvestCycle>.Update.Set("PlantCalendar.$[f]", schedule);
        var options = new UpdateOptions()
        {
            ArrayFilters = new List<ArrayFilterDefinition<BsonValue>>()
            {
                new BsonDocument("f._id",
                new BsonDocument("$eq", plantCalendarId))
            }
        };

        AddCommand(() => Collection.UpdateOneAsync(plantFilter, update, options));
    }

    #endregion

    protected override IMongoCollection<PlantHarvestCycle> GetCollection()
    {
        return _unitOfWork.GetCollection<IMongoCollection<PlantHarvestCycle>, PlantHarvestCycle>(PLANT_HARVEST_COLLECTION_NAME);
    }
}
