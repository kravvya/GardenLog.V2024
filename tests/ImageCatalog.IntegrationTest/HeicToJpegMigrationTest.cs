using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ImageCatalog.Api.Services;
using ImageCatalog.IntegrationTest.Fixture;
using MongoDB.Driver;
using Xunit.Abstractions;

namespace ImageCatalog.IntegrationTest;

/// <summary>
/// Migration test to convert all existing HEIC files to JPEG and update database references.
/// This is a ONE-TIME migration and should be run manually LOCALLY ONLY.
/// Requires: az login (for Azure Storage access)
/// 
/// ⚠️ WARNING: Contains hardcoded MongoDB connection string - REMOVE BEFORE COMMIT!
/// </summary>
public class HeicToJpegMigrationTest : IClassFixture<ImageCatalogServiceFixture>
{
    private readonly ITestOutputHelper _output;
    
    private const string IMAGE_CONTAINER = "images";
    private const string THUMBNAIL_CONTAINER = "thumbnails";
    private const string STORAGE_ACCOUNT_URL = "https://glimages.blob.core.windows.net";
    
    // ⚠️ TODO: HARDCODE YOUR PRODUCTION MONGODB CONNECTION STRING HERE
    // Format: mongodb+srv://username:password@cluster.mongodb.net/database?retryWrites=true&w=majority
    private const string MONGO_CONNECTION_STRING = "";
    private const string MONGO_DATABASE_NAME = "GardenLog-Db";
    private const string MONGO_COLLECTION_NAME = "ImageCatalog-Collection";

    public HeicToJpegMigrationTest(ImageCatalogServiceFixture fixture, ITestOutputHelper output)
    {
        _output = output;

        _output.WriteLine($"Starting HEIC to JPEG Migration @ {DateTime.Now:F}");
        _output.WriteLine($"Storage Account: {STORAGE_ACCOUNT_URL}");
        _output.WriteLine($"MongoDB Database: {MONGO_DATABASE_NAME}");
        _output.WriteLine("");
    }

    /// <summary>
    /// DRY RUN - Reports what will be converted without making changes
    /// </summary>
    [Fact(Skip = "Manual migration - remove Skip attribute to run")]
    public async Task DryRun_ReportHeicFilesForConversion()
    {
        _output.WriteLine("=== DRY RUN - Scanning for HEIC Files ===");
        _output.WriteLine("");

        var credential = new DefaultAzureCredential();
        var blobServiceClient = new BlobServiceClient(new Uri(STORAGE_ACCOUNT_URL), credential);

        var imageContainer = blobServiceClient.GetBlobContainerClient(IMAGE_CONTAINER);
        var thumbnailContainer = blobServiceClient.GetBlobContainerClient(THUMBNAIL_CONTAINER);

        // Scan images container
        var imageHeicFiles = await GetHeicFiles(imageContainer);
        _output.WriteLine($"Found {imageHeicFiles.Count} HEIC files in '{IMAGE_CONTAINER}' container");
        
        if (imageHeicFiles.Count > 0)
        {
            _output.WriteLine("\nImages to convert:");
            foreach (var file in imageHeicFiles.Take(20))
            {
                _output.WriteLine($"  - {file.Name} ({FormatBytes(file.Properties.ContentLength ?? 0)})");
            }
            if (imageHeicFiles.Count > 20)
            {
                _output.WriteLine($"  ... and {imageHeicFiles.Count - 20} more");
            }
        }

        // Scan thumbnails container
        var thumbnailHeicFiles = await GetHeicFiles(thumbnailContainer);
        _output.WriteLine($"\nFound {thumbnailHeicFiles.Count} HEIC files in '{THUMBNAIL_CONTAINER}' container");

        if (thumbnailHeicFiles.Count > 0)
        {
            _output.WriteLine("\nThumbnails to convert:");
            foreach (var file in thumbnailHeicFiles.Take(20))
            {
                _output.WriteLine($"  - {file.Name} ({FormatBytes(file.Properties.ContentLength ?? 0)})");
            }
            if (thumbnailHeicFiles.Count > 20)
            {
                _output.WriteLine($"  ... and {thumbnailHeicFiles.Count - 20} more");
            }
        }

        // Check database
        var dbHeicReferences = await GetDatabaseHeicReferences();
        _output.WriteLine($"\nFound {dbHeicReferences.Count} HEIC references in MongoDB");

        _output.WriteLine("\n=== Summary ===");
        _output.WriteLine($"Total HEIC images: {imageHeicFiles.Count}");
        _output.WriteLine($"Total HEIC thumbnails: {thumbnailHeicFiles.Count}");
        _output.WriteLine($"Total database references: {dbHeicReferences.Count}");
        _output.WriteLine("");
        _output.WriteLine("Run ConvertAllHeicToJpeg_AndUpdateDatabase() to perform the migration.");
    }

    /// <summary>
    /// ACTUAL MIGRATION - Converts all HEIC files to JPEG and updates database
    /// WARNING: This will modify production data!
    /// </summary>
    [Fact(Skip = "Manual migration - remove Skip attribute to run")]
    public async Task ConvertAllHeicToJpeg_AndUpdateDatabase()
    {
        _output.WriteLine("=== STARTING HEIC TO JPEG MIGRATION ===");
        _output.WriteLine("⚠️ WARNING: This will modify production data!");
        _output.WriteLine("");

        var credential = new DefaultAzureCredential();
        var blobServiceClient = new BlobServiceClient(new Uri(STORAGE_ACCOUNT_URL), credential);

        var imageContainer = blobServiceClient.GetBlobContainerClient(IMAGE_CONTAINER);
        var thumbnailContainer = blobServiceClient.GetBlobContainerClient(THUMBNAIL_CONTAINER);

        // Get image processors
        var imageSharpProcessor = new ImageSharpProcessor(new Microsoft.Extensions.Logging.Abstractions.NullLogger<ImageSharpProcessor>());
        var imageMagickProcessor = new ImageMagickProcessor(new Microsoft.Extensions.Logging.Abstractions.NullLogger<ImageMagickProcessor>());

        var stats = new MigrationStats();

        try
        {
            // Step 1: Convert images
            _output.WriteLine("Step 1: Converting HEIC images to JPEG...");
            var imageHeicFiles = await GetHeicFiles(imageContainer);
            stats.TotalImages = imageHeicFiles.Count;

            foreach (var heicFile in imageHeicFiles)
            {
                try
                {
                    await ConvertAndReplaceFile(
                        imageContainer, 
                        heicFile.Name, 
                        imageMagickProcessor, 
                        stats,
                        isImage: true);
                }
                catch (Exception ex)
                {
                    stats.FailedImages++;
                    stats.Errors.Add($"Image: {heicFile.Name} - {ex.Message}");
                    _output.WriteLine($"  ✗ Failed: {heicFile.Name} - {ex.Message}");
                }
            }

            _output.WriteLine($"\nStep 1 Complete: {stats.ConvertedImages}/{stats.TotalImages} images converted");
            _output.WriteLine("");

            // Step 2: Convert thumbnails
            _output.WriteLine("Step 2: Converting HEIC thumbnails to JPEG...");
            var thumbnailHeicFiles = await GetHeicFiles(thumbnailContainer);
            stats.TotalThumbnails = thumbnailHeicFiles.Count;

            foreach (var heicFile in thumbnailHeicFiles)
            {
                try
                {
                    await ConvertAndReplaceFile(
                        thumbnailContainer, 
                        heicFile.Name, 
                        imageMagickProcessor, 
                        stats,
                        isImage: false);
                }
                catch (Exception ex)
                {
                    stats.FailedThumbnails++;
                    stats.Errors.Add($"Thumbnail: {heicFile.Name} - {ex.Message}");
                    _output.WriteLine($"  ✗ Failed: {heicFile.Name} - {ex.Message}");
                }
            }

            _output.WriteLine($"\nStep 2 Complete: {stats.ConvertedThumbnails}/{stats.TotalThumbnails} thumbnails converted");
            _output.WriteLine("");

            // Step 3: Update database
            _output.WriteLine("Step 3: Updating MongoDB references...");
            var updatedCount = await UpdateDatabaseReferences();
            stats.DatabaseUpdates = updatedCount;

            _output.WriteLine($"Step 3 Complete: {updatedCount} database records updated");
            _output.WriteLine("");

            // Print final summary
            PrintMigrationSummary(stats);

            // Assert success
            Assert.True(stats.FailedImages == 0, $"{stats.FailedImages} image conversions failed");
            Assert.True(stats.FailedThumbnails == 0, $"{stats.FailedThumbnails} thumbnail conversions failed");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"\n❌ MIGRATION FAILED: {ex.Message}");
            _output.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    private async Task ConvertAndReplaceFile(
        BlobContainerClient container,
        string heicFileName,
        IImageProcessor processor,
        MigrationStats stats,
        bool isImage)
    {
        var jpegFileName = Path.ChangeExtension(heicFileName, ".jpg");

        _output.WriteLine($"  Converting: {heicFileName} → {jpegFileName}");

        // Download HEIC file
        var heicBlob = container.GetBlobClient(heicFileName);
        using var heicStream = new MemoryStream();
        await heicBlob.DownloadToAsync(heicStream);
        heicStream.Position = 0;

        // Convert to JPEG
        var (jpegStream, width, height) = await processor.ConvertToJpegAsync(heicStream, quality: 95);

        using (jpegStream)
        {
            // Upload JPEG
            var jpegBlob = container.GetBlobClient(jpegFileName);
            jpegStream.Position = 0;
            await jpegBlob.UploadAsync(jpegStream, new BlobHttpHeaders { ContentType = "image/jpeg" });

            _output.WriteLine($"    ✓ Uploaded: {jpegFileName} ({width}x{height})");
        }

        // Delete original HEIC
        await heicBlob.DeleteIfExistsAsync();
        _output.WriteLine($"    ✓ Deleted: {heicFileName}");

        if (isImage)
            stats.ConvertedImages++;
        else
            stats.ConvertedThumbnails++;
    }

    private async Task<int> UpdateDatabaseReferences()
    {
        _output.WriteLine($"  Connecting to MongoDB: {MONGO_DATABASE_NAME}");
        
        var client = new MongoClient(MONGO_CONNECTION_STRING);
        var database = client.GetDatabase(MONGO_DATABASE_NAME);
        var collection = database.GetCollection<MongoDB.Bson.BsonDocument>(MONGO_COLLECTION_NAME);

        // Find all documents with .heic extension
        var filter = MongoDB.Bson.BsonDocument.Parse("{ FileName: /\\.heic$/i }");
        var documents = await collection.Find(filter).ToListAsync();

        _output.WriteLine($"  Found {documents.Count} documents with .heic extension");

        int updatedCount = 0;
        foreach (var doc in documents)
        {
            var oldFileName = doc["FileName"].AsString;
            var newFileName = Path.ChangeExtension(oldFileName, ".jpg");

            var update = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Update
                .Set("FileName", newFileName);

            var result = await collection.UpdateOneAsync(
                MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", doc["_id"]),
                update);

            if (result.ModifiedCount > 0)
            {
                updatedCount++;
                _output.WriteLine($"    ✓ Updated: {oldFileName} → {newFileName}");
            }
        }

        return updatedCount;
    }

    private async Task<List<BlobItem>> GetHeicFiles(BlobContainerClient container)
    {
        var heicFiles = new List<BlobItem>();

        await foreach (var blob in container.GetBlobsAsync())
        {
            if (blob.Name.EndsWith(".heic", StringComparison.OrdinalIgnoreCase) ||
                blob.Name.EndsWith(".heif", StringComparison.OrdinalIgnoreCase))
            {
                heicFiles.Add(blob);
            }
        }

        return heicFiles;
    }

    private async Task<List<string>> GetDatabaseHeicReferences()
    {
        var heicReferences = new List<string>();

        try
        {
            var client = new MongoClient(MONGO_CONNECTION_STRING);
            var database = client.GetDatabase(MONGO_DATABASE_NAME);
            var collection = database.GetCollection<MongoDB.Bson.BsonDocument>(MONGO_COLLECTION_NAME);

            var filter = MongoDB.Bson.BsonDocument.Parse("{ FileName: /\\.heic$/i }");
            var documents = await collection.Find(filter).ToListAsync();

            foreach (var doc in documents)
            {
                heicReferences.Add(doc["FileName"].AsString);
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"  ⚠️ Error checking database: {ex.Message}");
        }

        return heicReferences;
    }

    private void PrintMigrationSummary(MigrationStats stats)
    {
        _output.WriteLine("");
        _output.WriteLine("╔════════════════════════════════════════╗");
        _output.WriteLine("║     MIGRATION SUMMARY                  ║");
        _output.WriteLine("╚════════════════════════════════════════╝");
        _output.WriteLine("");
        _output.WriteLine($"Images:");
        _output.WriteLine($"  Total:     {stats.TotalImages}");
        _output.WriteLine($"  Converted: {stats.ConvertedImages}");
        _output.WriteLine($"  Failed:    {stats.FailedImages}");
        _output.WriteLine("");
        _output.WriteLine($"Thumbnails:");
        _output.WriteLine($"  Total:     {stats.TotalThumbnails}");
        _output.WriteLine($"  Converted: {stats.ConvertedThumbnails}");
        _output.WriteLine($"  Failed:    {stats.FailedThumbnails}");
        _output.WriteLine("");
        _output.WriteLine($"Database:");
        _output.WriteLine($"  Records Updated: {stats.DatabaseUpdates}");
        _output.WriteLine("");

        if (stats.Errors.Any())
        {
            _output.WriteLine("❌ ERRORS:");
            foreach (var error in stats.Errors)
            {
                _output.WriteLine($"  - {error}");
            }
            _output.WriteLine("");
        }

        var totalSuccess = stats.ConvertedImages + stats.ConvertedThumbnails;
        var totalFailed = stats.FailedImages + stats.FailedThumbnails;

        if (totalFailed == 0)
        {
            _output.WriteLine("✅ MIGRATION COMPLETED SUCCESSFULLY!");
        }
        else
        {
            _output.WriteLine($"⚠️ MIGRATION COMPLETED WITH {totalFailed} ERRORS");
        }

        _output.WriteLine($"\nCompleted @ {DateTime.Now:F}");
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private class MigrationStats
    {
        public int TotalImages { get; set; }
        public int ConvertedImages { get; set; }
        public int FailedImages { get; set; }

        public int TotalThumbnails { get; set; }
        public int ConvertedThumbnails { get; set; }
        public int FailedThumbnails { get; set; }

        public int DatabaseUpdates { get; set; }

        public List<string> Errors { get; set; } = new();
    }
}
