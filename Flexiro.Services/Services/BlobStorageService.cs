using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Flexiro.Services.Services.Interfaces;

namespace Flexiro.Services.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public BlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration["AzureBlobStorage:ConnectionString"]!;
            _containerName = configuration["AzureBlobStorage:ContainerName"]!;
        }

        // Upload the image to blob storage
        public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
        {
            var blobClient = new BlobContainerClient(_connectionString, _containerName);
            await blobClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

            var blob = blobClient.GetBlobClient(fileName);
            await blob.UploadAsync(imageStream, overwrite: true);
            return blob.Uri.ToString();
        }

        public string GetImageUrl(string relativePath)
        {
            var blobClient = new BlobContainerClient(_connectionString, _containerName);
            var blob = blobClient.GetBlobClient(relativePath);
            return blob.Uri.ToString(); // Construct the full URL
        }

        // Retrieve the image from blob storage
        public async Task<Stream> GetImageAsync(string fileName)
        {
            var blobClient = new BlobContainerClient(_connectionString, _containerName);
            var blob = blobClient.GetBlobClient(fileName);

            if (await blob.ExistsAsync())
            {
                var downloadInfo = await blob.DownloadAsync();
                return downloadInfo.Value.Content;
            }

            throw new FileNotFoundException("Image not found in Blob Storage.");
        }

        public async Task<List<string>> GetProductImageUrlsAsync(int productId)
        {
            var blobClient = new BlobContainerClient(_connectionString, _containerName);
            var imageUrls = new List<string>();

            await foreach (var blobItem in blobClient.GetBlobsAsync(prefix: $"{productId}/"))
            {
                var blob = blobClient.GetBlobClient(blobItem.Name);
                imageUrls.Add(blob.Uri.ToString());
            }

            return imageUrls;
        }

        // Delete image from blob storage
        public async Task DeleteImageAsync(string fileName)
        {
            var blobClient = new BlobContainerClient(_connectionString, _containerName);
            var blob = blobClient.GetBlobClient(fileName);

            if (await blob.ExistsAsync())
            {
                await blob.DeleteAsync();
            }
        }
    }
}