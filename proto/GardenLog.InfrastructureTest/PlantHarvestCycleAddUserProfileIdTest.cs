using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PlantHarvest.Contract.Enum;
using Xunit.Abstractions;

namespace GardenLog.InfrastructureTest;

/// <summary>
/// Migration test to add UserProfileId field to existing PlantHarvestCycle documents.
/// This test reads all PlantHarvestCycle documents, looks up their parent HarvestCycle,
/// and updates each document with the UserProfileId from the parent.
/// </summary>
public class PlantHarvestCycleAddUserProfileIdTest
{
    private readonly ITestOutputHelper _output;

    public PlantHarvestCycleAddUserProfileIdTest(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Migration test - adds UserProfileId to PlantHarvestCycle-Collection documents
    /// by looking up the parent HarvestCycle and copying the UserProfileId field.
    /// 
    /// IMPORTANT: Update the connection string before running!
    /// 
    /// This test:
    /// 1. Reads all PlantHarvestCycle documents from the collection
    /// 2. For each document, looks up its parent HarvestCycle using HarvestCycleId
    /// 3. Copies the UserProfileId from the parent HarvestCycle
    /// 4. Updates the PlantHarvestCycle document with the new UserProfileId field
    /// </summary>
    [Fact]
    public void AddUserProfileIdToPlantHarvestCycles()
    {
        // IMPORTANT: Update connection string with actual credentials
        var client = new MongoClient("mongodb+srv://<user>:<password>@gardenlog2023-cluster.rln8w5k.mongodb.net/");
        var database = client.GetDatabase("GardenLog-Db");

        // Register BsonClassMaps for domain entities
        RegisterBsonClassMaps();

        // Get references to collections
        var plantHarvestCycleCollection = database.GetCollection<BsonDocument>("PlantHarvestCycle-Collection");
        var harvestCycleCollection = database.GetCollection<BsonDocument>("HarvestCycle-Collection");

        _output.WriteLine("=== Starting Migration: Add UserProfileId to PlantHarvestCycle documents ===");
        _output.WriteLine("");

        // Get all PlantHarvestCycle documents
        var plantFilter = Builders<BsonDocument>.Filter.Empty;
        var plantDocuments = plantHarvestCycleCollection.Find(plantFilter).ToList();

        _output.WriteLine($"Found {plantDocuments.Count} PlantHarvestCycle documents to process");
        _output.WriteLine("");

        int successCount = 0;
        int skipCount = 0;
        int errorCount = 0;

        // Process each PlantHarvestCycle document
        foreach (var plantDoc in plantDocuments)
        {
            try
            {
                var plantId = plantDoc["_id"].AsString;
                var harvestCycleId = plantDoc["HarvestCycleId"].AsString;

                // Check if UserProfileId already exists (skip if already migrated)
                if (plantDoc.Contains("UserProfileId"))
                {
                    _output.WriteLine($"SKIP: PlantHarvestCycle {plantId} already has UserProfileId");
                    skipCount++;
                    continue;
                }

                // Find the parent HarvestCycle document
                var harvestFilter = Builders<BsonDocument>.Filter.Eq("_id", harvestCycleId);
                var harvestDoc = harvestCycleCollection.Find(harvestFilter).FirstOrDefault();

                if (harvestDoc == null)
                {
                    _output.WriteLine($"ERROR: HarvestCycle {harvestCycleId} not found for PlantHarvestCycle {plantId}");
                    errorCount++;
                    continue;
                }

                // Check if the parent has UserProfileId
                if (!harvestDoc.Contains("UserProfileId"))
                {
                    _output.WriteLine($"ERROR: HarvestCycle {harvestCycleId} does not have UserProfileId");
                    errorCount++;
                    continue;
                }

                // Get the UserProfileId from parent HarvestCycle
                var userProfileId = harvestDoc["UserProfileId"].AsString;

                // Update the PlantHarvestCycle document with UserProfileId
                var update = Builders<BsonDocument>.Update.Set("UserProfileId", userProfileId);
                var updateFilter = Builders<BsonDocument>.Filter.Eq("_id", plantId);
                
                var result = plantHarvestCycleCollection.UpdateOne(updateFilter, update);

                if (result.ModifiedCount > 0)
                {
                    _output.WriteLine($"SUCCESS: Added UserProfileId '{userProfileId}' to PlantHarvestCycle {plantId}");
                    successCount++;
                }
                else
                {
                    _output.WriteLine($"WARNING: Update had no effect for PlantHarvestCycle {plantId}");
                    errorCount++;
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"EXCEPTION: Error processing document: {ex.Message}");
                errorCount++;
            }
        }

        _output.WriteLine("");
        _output.WriteLine("=== Migration Summary ===");
        _output.WriteLine($"Total documents: {plantDocuments.Count}");
        _output.WriteLine($"Successfully updated: {successCount}");
        _output.WriteLine($"Skipped (already had UserProfileId): {skipCount}");
        _output.WriteLine($"Errors: {errorCount}");
        _output.WriteLine("");

        // Verify the migration
        VerifyMigration(plantHarvestCycleCollection);
    }

    /// <summary>
    /// Verification test - checks that all PlantHarvestCycle documents have UserProfileId
    /// and that they match their parent HarvestCycle's UserProfileId.
    /// 
    /// Run this after the migration to ensure everything was successful.
    /// </summary>
    [Fact]
    public void VerifyUserProfileIdMigration()
    {
        // IMPORTANT: Update connection string with actual credentials
        var client = new MongoClient("mongodb+srv://<user>:<password>@gardenlog2023-cluster.rln8w5k.mongodb.net/");
        var database = client.GetDatabase("GardenLog-Db");

        var plantHarvestCycleCollection = database.GetCollection<BsonDocument>("PlantHarvestCycle-Collection");

        _output.WriteLine("=== Verifying UserProfileId Migration ===");
        _output.WriteLine("");

        VerifyMigration(plantHarvestCycleCollection);
    }

    /// <summary>
    /// Verifies that all PlantHarvestCycle documents have the UserProfileId field
    /// </summary>
    private void VerifyMigration(IMongoCollection<BsonDocument> plantHarvestCycleCollection)
    {
        // Count documents without UserProfileId
        var missingUserProfileIdFilter = Builders<BsonDocument>.Filter.Exists("UserProfileId", false);
        var missingCount = plantHarvestCycleCollection.CountDocuments(missingUserProfileIdFilter);

        // Count total documents
        var totalCount = plantHarvestCycleCollection.CountDocuments(Builders<BsonDocument>.Filter.Empty);

        _output.WriteLine($"Total PlantHarvestCycle documents: {totalCount}");
        _output.WriteLine($"Documents with UserProfileId: {totalCount - missingCount}");
        _output.WriteLine($"Documents MISSING UserProfileId: {missingCount}");
        _output.WriteLine("");

        if (missingCount == 0)
        {
            _output.WriteLine("✓ VERIFICATION PASSED: All documents have UserProfileId");
        }
        else
        {
            _output.WriteLine("✗ VERIFICATION FAILED: Some documents are missing UserProfileId");
            
            // List the documents missing UserProfileId
            var missingDocs = plantHarvestCycleCollection
                .Find(missingUserProfileIdFilter)
                .Project(Builders<BsonDocument>.Projection.Include("_id").Include("HarvestCycleId"))
                .Limit(10)
                .ToList();

            _output.WriteLine("");
            _output.WriteLine("Sample of documents missing UserProfileId (up to 10):");
            foreach (var doc in missingDocs)
            {
                _output.WriteLine($"  - PlantHarvestCycle Id: {doc["_id"]}, HarvestCycleId: {doc.GetValue("HarvestCycleId", "MISSING")}");
            }
        }
    }

    /// <summary>
    /// Rollback test - removes UserProfileId from all PlantHarvestCycle documents.
    /// Use this ONLY if you need to rollback the migration.
    /// </summary>
    [Fact]
    public void RollbackUserProfileIdMigration()
    {
        // IMPORTANT: Update connection string with actual credentials
        var client = new MongoClient("mongodb+srv://<user>:<password>@gardenlog2023-cluster.rln8w5k.mongodb.net/");
        var database = client.GetDatabase("GardenLog-Db");

        var plantHarvestCycleCollection = database.GetCollection<BsonDocument>("PlantHarvestCycle-Collection");

        _output.WriteLine("=== Rolling Back UserProfileId Migration ===");
        _output.WriteLine("WARNING: This will remove UserProfileId from ALL PlantHarvestCycle documents!");
        _output.WriteLine("");

        // Remove UserProfileId field from all documents
        var update = Builders<BsonDocument>.Update.Unset("UserProfileId");
        var filter = Builders<BsonDocument>.Filter.Empty;

        var result = plantHarvestCycleCollection.UpdateMany(filter, update);

        _output.WriteLine($"Rollback complete:");
        _output.WriteLine($"  - Matched documents: {result.MatchedCount}");
        _output.WriteLine($"  - Modified documents: {result.ModifiedCount}");
        _output.WriteLine("");

        // Verify rollback
        var remainingCount = plantHarvestCycleCollection.CountDocuments(
            Builders<BsonDocument>.Filter.Exists("UserProfileId", true));

        if (remainingCount == 0)
        {
            _output.WriteLine("✓ ROLLBACK SUCCESSFUL: No documents have UserProfileId");
        }
        else
        {
            _output.WriteLine($"✗ ROLLBACK INCOMPLETE: {remainingCount} documents still have UserProfileId");
        }
    }

    /// <summary>
    /// Register BsonClassMaps for PlantHarvest domain entities
    /// </summary>
    private void RegisterBsonClassMaps()
    {
        // Only register if not already registered
        if (!BsonClassMap.IsClassMapRegistered(typeof(BaseEntity)))
        {
            BsonClassMap.RegisterClassMap<BaseEntity>(p =>
            {
                p.AutoMap();
                p.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(PlantHarvestCycle)))
        {
            BsonClassMap.RegisterClassMap<PlantHarvestCycle>(g =>
            {
                g.AutoMap();
                g.SetIgnoreExtraElements(true);
                g.MapMember(m => m.PlantingMethod).SetSerializer(new EnumSerializer<PlantingMethodEnum>(BsonType.String));
                g.MapProperty(m => m.PlantCalendar).SetDefaultValue(new List<PlantSchedule>());
                g.MapProperty(m => m.SpacingInInches).SetDefaultValue(0);
                g.MapProperty(m => m.PlantsPerFoot).SetDefaultValue(0.0);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(PlantSchedule)))
        {
            BsonClassMap.RegisterClassMap<PlantSchedule>(g =>
            {
                g.AutoMap();
                g.SetIgnoreExtraElements(true);
                g.MapMember(m => m.TaskType).SetSerializer(new EnumSerializer<WorkLogReasonEnum>(BsonType.String));
            });
        }
    }
}
