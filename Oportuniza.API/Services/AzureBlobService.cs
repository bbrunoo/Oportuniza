using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Oportuniza.API.Services
{
    public class AzureBlobService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobService(IConfiguration configuration)
        {
            _connectionString = configuration["AzureStorage:ConnectionString"];
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) throw new ArgumentNullException("Invalid File" + nameof(file));

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            string blobName = $"{Guid.NewGuid()}-{file.FileName}";

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadProfileImage(IFormFile file, string containerName, Guid userId)
        {
            if (file == null || file.Length == 0) throw new ArgumentNullException("Invalid File" + nameof(file));

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            string blobName = $"{userId}";

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType,
                    }
                };
                await blobClient.UploadAsync(stream, uploadOptions);
            }

            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadPostImage(IFormFile file, string containerName, Guid userId)
        {
            if (file == null || file.Length == 0) throw new ArgumentNullException("Invalid File" + nameof(file));

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            string blobName = $"publication-{userId}-{Guid.NewGuid()}";

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType,
                    }
                };
                await blobClient.UploadAsync(stream, uploadOptions);
            }

            return blobClient.Uri.ToString();
        }
    }
}
