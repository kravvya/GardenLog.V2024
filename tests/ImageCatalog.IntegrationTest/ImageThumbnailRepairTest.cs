using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ImageCatalog.IntegrationTest.Fixture;
using Xunit.Abstractions;

namespace ImageCatalog.IntegrationTest
{
    /// <summary>
    /// Test for repairing missing thumbnails for images uploaded in 2025.
    /// This test is skipped by default and should be run manually when needed.
    /// Uses DefaultAzureCredential to authenticate with Azure Storage.
    /// </summary>
    public class ImageThumbnailRepairTest : IClassFixture<ImageCatalogServiceFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly FileCatalogClient _fileClient;

        private const string IMAGE_CONTAINER = "images";
        private const string THUMBNAIL_CONTAINER = "thumbnails";
        
        // Hardcoded Azure Storage account URL - update if needed
        private const string STORAGE_ACCOUNT_URL = "https://glimages.blob.core.windows.net";

        public ImageThumbnailRepairTest(ImageCatalogServiceFixture fixture, ITestOutputHelper output)
        {
            _fileClient = fixture.FileCatalogClient;
            _output = output;

            _output.WriteLine($"Starting Thumbnail Repair Process @ {DateTime.Now:F}");
            _output.WriteLine($"Using Azure Storage Account: {STORAGE_ACCOUNT_URL}");
            _output.WriteLine($"Authentication: DefaultAzureCredential (your Azure credentials)");
            _output.WriteLine("");
        }

        /// <summary>
        /// Scans all images in blob storage created in 2025 and generates thumbnails for ones that are missing.
        /// Mark with [Fact] to run, or run manually from Test Explorer.
        /// </summary>
        [Fact(Skip = "Manual repair task - remove Skip attribute to run")]
        public async Task RepairMissingThumbnails_For2025Images()
        {
            var startYear = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endYear = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            await RepairMissingThumbnails(startYear, endYear);
        }

        /// <summary>
        /// Test to check the status without making any changes.
        /// This will report how many images are missing thumbnails.
        /// </summary>
        [Fact(Skip = "Manual diagnostic task - remove Skip attribute to run")]
        public async Task CheckMissingThumbnails_For2025Images_DryRun()
        {
            var startYear = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endYear = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            await CheckMissingThumbnails(startYear, endYear);
        }

        /// <summary>
        /// Core method to repair missing thumbnails for a given date range.
        /// </summary>
        private async Task RepairMissingThumbnails(DateTime startDate, DateTime endDate)
        {
            try
            {
                _output.WriteLine($"=== Starting Thumbnail Repair Process ===");
                _output.WriteLine($"Date Range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
                _output.WriteLine("");

                // Create BlobServiceClient using DefaultAzureCredential
                var credential = new DefaultAzureCredential();
                var blobServiceClient = new BlobServiceClient(new Uri(STORAGE_ACCOUNT_URL), credential);
                
                var imageContainer = blobServiceClient.GetBlobContainerClient(IMAGE_CONTAINER);
                var thumbnailContainer = blobServiceClient.GetBlobContainerClient(THUMBNAIL_CONTAINER);

                // Get all images created in the specified date range
                var imagesInDateRange = await GetImagesInDateRange(imageContainer, startDate, endDate);
                _output.WriteLine($"Found {imagesInDateRange.Count} images created between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}");
                _output.WriteLine("");

                if (imagesInDateRange.Count == 0)
                {
                    _output.WriteLine("No images found in the specified date range. Nothing to repair.");
                    return;
                }

                // Get all existing thumbnails
                var existingThumbnails = await GetAllThumbnailNames(thumbnailContainer);
                _output.WriteLine($"Found {existingThumbnails.Count} existing thumbnails");
                _output.WriteLine("");

                // Find images missing thumbnails
                var missingThumbnails = imagesInDateRange
                    .Where(img => !existingThumbnails.Contains(img.Name))
                    .ToList();

                _output.WriteLine($"Images missing thumbnails: {missingThumbnails.Count}");
                _output.WriteLine("");

                if (missingThumbnails.Count == 0)
                {
                    _output.WriteLine("✓ All images have thumbnails! No repair needed.");
                    return;
                }

                // Process each missing thumbnail
                int successCount = 0;
                int failureCount = 0;
                var failures = new List<(string fileName, string error)>();

                _output.WriteLine("=== Starting Thumbnail Generation ===");
                _output.WriteLine("");

                for (int i = 0; i < missingThumbnails.Count; i++)
                {
                    var image = missingThumbnails[i];
                    var progress = $"[{i + 1}/{missingThumbnails.Count}]";

                    try
                    {
                        _output.WriteLine($"{progress} Processing: {image.Name} (Size: {FormatBytes(image.Properties.ContentLength ?? 0)}, Created: {image.Properties.CreatedOn:yyyy-MM-dd HH:mm:ss})");

                        // Call the API to generate the thumbnail
                        var response = await _fileClient.ResizeImageToThumbnail(image.Name);

                        if (response.IsSuccessStatusCode)
                        {
                            successCount++;
                            _output.WriteLine($"  ✓ Success - Thumbnail generated");
                        }
                        else
                        {
                            failureCount++;
                            var errorMessage = $"HTTP {response.StatusCode}: {response.ReasonPhrase}";
                            failures.Add((image.Name, errorMessage));
                            _output.WriteLine($"  ✗ Failed - {errorMessage}");
                        }

                        // Add a small delay to avoid overwhelming the API
                        if (i < missingThumbnails.Count - 1)
                        {
                            await Task.Delay(100);
                        }
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        failures.Add((image.Name, ex.Message));
                        _output.WriteLine($"  ✗ Exception - {ex.Message}");
                    }

                    _output.WriteLine("");
                }

                // Print summary
                _output.WriteLine("=== Summary ===");
                _output.WriteLine($"Total images processed: {missingThumbnails.Count}");
                _output.WriteLine($"Successful: {successCount}");
                _output.WriteLine($"Failed: {failureCount}");
                _output.WriteLine("");

                if (failures.Count > 0)
                {
                    _output.WriteLine("=== Failed Images ===");
                    foreach (var (fileName, error) in failures)
                    {
                        _output.WriteLine($"  - {fileName}: {error}");
                    }
                    _output.WriteLine("");
                }

                _output.WriteLine($"Process completed @ {DateTime.Now:F}");

                // Assert that at least some were successful if there were any to process
                Assert.True(successCount > 0 || missingThumbnails.Count == 0,
                    $"Expected at least one successful thumbnail generation, but got {successCount} successes out of {missingThumbnails.Count} attempts");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"FATAL ERROR: {ex.Message}");
                _output.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Dry run - just checks and reports status without making changes.
        /// </summary>
        private async Task CheckMissingThumbnails(DateTime startDate, DateTime endDate)
        {
            try
            {
                _output.WriteLine($"=== Checking Thumbnail Status (Dry Run) ===");
                _output.WriteLine($"Date Range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
                _output.WriteLine("");

                // Create BlobServiceClient using DefaultAzureCredential
                var credential = new DefaultAzureCredential();
                var blobServiceClient = new BlobServiceClient(new Uri(STORAGE_ACCOUNT_URL), credential);
                
                var imageContainer = blobServiceClient.GetBlobContainerClient(IMAGE_CONTAINER);
                var thumbnailContainer = blobServiceClient.GetBlobContainerClient(THUMBNAIL_CONTAINER);

                var imagesInDateRange = await GetImagesInDateRange(imageContainer, startDate, endDate);
                _output.WriteLine($"Found {imagesInDateRange.Count} images created between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}");

                var existingThumbnails = await GetAllThumbnailNames(thumbnailContainer);
                _output.WriteLine($"Found {existingThumbnails.Count} existing thumbnails");
                _output.WriteLine("");

                var missingThumbnails = imagesInDateRange
                    .Where(img => !existingThumbnails.Contains(img.Name))
                    .ToList();

                _output.WriteLine($"=== Results ===");
                _output.WriteLine($"Images with thumbnails: {imagesInDateRange.Count - missingThumbnails.Count}");
                _output.WriteLine($"Images missing thumbnails: {missingThumbnails.Count}");
                _output.WriteLine("");

                if (missingThumbnails.Count > 0)
                {
                    _output.WriteLine("Images that need thumbnails:");
                    foreach (var image in missingThumbnails.Take(20)) // Show first 20
                    {
                        _output.WriteLine($"  - {image.Name} (Created: {image.Properties.CreatedOn:yyyy-MM-dd HH:mm:ss}, Size: {FormatBytes(image.Properties.ContentLength ?? 0)})");
                    }

                    if (missingThumbnails.Count > 20)
                    {
                        _output.WriteLine($"  ... and {missingThumbnails.Count - 20} more");
                    }
                }
                else
                {
                    _output.WriteLine("✓ All images have thumbnails!");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"ERROR: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets all images from the blob container that were created in the specified date range.
        /// </summary>
        private async Task<List<BlobItem>> GetImagesInDateRange(BlobContainerClient container, DateTime startDate, DateTime endDate)
        {
            var images = new List<BlobItem>();

            await foreach (var blob in container.GetBlobsAsync(BlobTraits.Metadata))
            {
                if (blob.Properties.CreatedOn.HasValue)
                {
                    var createdOn = blob.Properties.CreatedOn.Value.UtcDateTime;
                    if (createdOn >= startDate && createdOn < endDate)
                    {
                        images.Add(blob);
                    }
                }
            }

            return images.OrderBy(b => b.Properties.CreatedOn).ToList();
        }

        /// <summary>
        /// Gets all thumbnail file names from the thumbnail container.
        /// </summary>
        private async Task<HashSet<string>> GetAllThumbnailNames(BlobContainerClient container)
        {
            var thumbnails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            await foreach (var blob in container.GetBlobsAsync())
            {
                thumbnails.Add(blob.Name);
            }

            return thumbnails;
        }

        /// <summary>
        /// Formats bytes into a human-readable string.
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
