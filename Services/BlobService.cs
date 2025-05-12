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
            if (string.IsNullOrEmpty(blobUrl))
            {
                // Nothing to delete
                return false;
            }
            try
            {
                // Need to parse the filename from the URL
                // The filename is the last segment of the URI path
                Uri blobUri = new Uri(blobUrl);
                string fileName = Path.GetFileName(blobUri.LocalPath);
                if (string.IsNullOrEmpty(fileName))
                {
                    // Could not parse filename from URL
                    // Log this situation if necessary
                    return false;
                }
                BlobClient blobClient = _container.GetBlobClient(fileName);
                // Delete the blob if it exists
                // DeleteIfExistsAsync returns true if the blob existed and was deleted, false otherwise.
                var result = await blobClient.DeleteIfExistsAsync();
                return result.Value; // Return the boolean indicating success
            }
            catch (Exception ex)
            {
                // Log the exception (using a proper logging framework is recommended)
                Console.WriteLine($"Error deleting blob {blobUrl}: {ex.Message}");
                // Depending on requirements, you might re-throw or just return false
                return false;
            }
        }
    }
}
