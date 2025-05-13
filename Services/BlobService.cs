using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEase.Services
{
    public interface IBlobService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);
        // Add the DeleteAsync method signature
        Task<bool> DeleteAsync(string blobUrl);
    }
    public class BlobService : IBlobService
    {
        private readonly BlobContainerClient _container;
        public BlobService(IConfiguration cfg)
        {
            var conn = cfg.GetValue<string>("Storage:ConnectionString"); // Using GetValue is slightly more robust
            var containerName = cfg.GetValue<string>("Storage:Container");
            if (string.IsNullOrEmpty(conn) || string.IsNullOrEmpty(containerName))
            {
                throw new InvalidOperationException("Storage connection string or container name is not configured properly.");
            }
            _container = new BlobContainerClient(conn, containerName);
            // Consider making CreateIfNotExists async if startup time is critical
            _container.CreateIfNotExists(PublicAccessType.Blob);
        }
        public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
        {
            // Get a reference to a blob
            BlobClient blobClient = _container.GetBlobClient(fileName);
            // Optional: Overwrite if exists. Default is false (throws exception if blob exists)
            // Consider your desired behavior. Overwrite=true is often useful for profile pics.
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType }); // Added overwrite: true
            // Return the URI
            return blobClient.Uri.ToString();
        }
        // Implement the DeleteAsync method
        public async Task<bool> DeleteAsync(string blobUrl)
        {
            if (string.IsNullOrWhiteSpace(blobUrl))
            {
                return false; // Invalid input
            }

            try
            {
                // Parse the blob name from the URL
                var blobUri = new Uri(blobUrl);
                var blobName = Path.GetFileName(Uri.UnescapeDataString(blobUri.AbsolutePath));

                if (string.IsNullOrEmpty(blobName))
                {
                    // Log warning: Invalid blob name derived from URL
                    Console.WriteLine($"Invalid blob name derived from URL: {blobUrl}");
                    return false;
                }

                // Get the BlobClient for the specified blob
                BlobClient blobClient = _container.GetBlobClient(blobName);

                // Delete the blob if it exists
                var response = await blobClient.DeleteIfExistsAsync();
                return response; // Returns true if deleted, false if blob didn't exist
            }
            catch (Exception ex)
            {
                // Log the error (use a proper logging framework like Serilog or Microsoft.Extensions.Logging)
                Console.WriteLine($"Error deleting blob {blobUrl}: {ex.Message}");
                return false;
            }
        }
    }
}
