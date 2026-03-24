using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Omega.FleetManagement.Domain.Interfaces;

namespace Omega.FleetManagement.Infrastructure.Storage
{
    public class AzureBlobFileStorageService : IFileStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobFileStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["StorageConfig:AzureBlob:ConnectionString"];
            var containerName = configuration["StorageConfig:AzureBlob:ContainerName"] ?? "uploads";

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("StorageConfig:AzureBlob:ConnectionString não configurado.");

            _containerClient = new BlobContainerClient(connectionString, containerName);
            _containerClient.CreateIfNotExists();
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string subFolder)
        {
            var safeName = Path.GetFileName(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{safeName}";
            var blobPath = $"{subFolder}/{DateTime.UtcNow:yyyy/MM}/{uniqueFileName}";

            var blob = _containerClient.GetBlobClient(blobPath);
            await blob.UploadAsync(fileStream, overwrite: false);

            return blobPath;
        }

        public async Task DeleteFileAsync(string filePath)
        {
            var normalizedPath = filePath.Replace("\\", "/");
            var blob = _containerClient.GetBlobClient(normalizedPath);
            await blob.DeleteIfExistsAsync();
        }
    }
}
