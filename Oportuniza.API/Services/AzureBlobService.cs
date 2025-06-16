using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;

namespace Oportuniza.API.Services
{
    public class AzureBlobService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobService(IConfiguration configuration)
        {
            _connectionString = configuration["AZURE_CONNECTION_STRING"];
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentNullException("Invalid File" + nameof(file));

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            memoryStream.Position = 0;
            bool isSafe = await IsImageSafeAsync(memoryStream, file.ContentType);
            if (!isSafe)
                throw new Exception("Imagem imprópria detectada. Upload bloqueado");

            memoryStream.Position = 0;

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient("images"); 
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            string blobName = $"{Guid.NewGuid()}-{file.FileName}";
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blobClient.Uri.ToString();
        }


        public async Task<string> UploadProfileImage(IFormFile file, string containerName, Guid userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentNullException("Invalid File" + nameof(file));

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            memoryStream.Position = 0;
            bool isSafe = await IsImageSafeAsync(memoryStream, file.ContentType);
            if (!isSafe)
                throw new Exception("Imagem imprópria detectada. Upload bloqueado");

            memoryStream.Position = 0;

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            string blobName = $"{userId}";
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadPostImage(IFormFile file, string containerName, Guid userId)
        {
            if (file == null || file.Length == 0) throw new ArgumentNullException("Invalid File" + nameof(file));

            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            try
            {
                bool isSafe = await IsImageSafeAsync(memoryStream, file.ContentType);
                if (!isSafe)
                {
                    throw new Exception("Imagem imprópria detectada. Upload bloqueado");
                }

                memoryStream.Position = 0;

                var blobServiceClient = new BlobServiceClient(_connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

                await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                string blobName = $"publication-{userId}-{Guid.NewGuid()}";

                var blobClient = blobContainerClient.GetBlobClient(blobName);

                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType,
                    }
                };

                await blobClient.UploadAsync(memoryStream, uploadOptions);

                return blobClient.Uri.ToString();
            }
            finally
            {
                memoryStream.Dispose();
            }
        }

        public async Task<string> UploadCompanyImage(IFormFile file, string containerName, Guid userId)
        {
            if (file == null || file.Length == 0) throw new ArgumentNullException("Invalid File" + nameof(file));

            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            try
            {
                bool isSafe = await IsImageSafeAsync(memoryStream, file.ContentType);
                if (!isSafe)
                {
                    throw new Exception("Imagem imprópria detectada. Upload bloqueado");
                }

                memoryStream.Position = 0;

                var blobServiceClient = new BlobServiceClient(_connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

                await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                string blobName = $"company-{userId}-{Guid.NewGuid()}";

                var blobClient = blobContainerClient.GetBlobClient(blobName);

                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType,
                    }
                };

                await blobClient.UploadAsync(memoryStream, uploadOptions);

                return blobClient.Uri.ToString();
            }
            finally
            {
                memoryStream.Dispose();
            }
        }
        public async Task<bool> IsImageSafeAsync(Stream originalStream, string contentType)
        {
            string AZURE_MODERATOR_APIKEY = Environment.GetEnvironmentVariable("AZURE_MODERATOR_APIKEY");
            string AZURE_MODERATOR_ENDPOINT = Environment.GetEnvironmentVariable("AZURE_MODERATOR_ENDPOINT");
            
            string endpoint = AZURE_MODERATOR_ENDPOINT;
            string apiKey = AZURE_MODERATOR_APIKEY;

            var analyzeUrl = endpoint + "vision/v3.2/analyze?visualFeatures=Adult";

            var tempStream = new MemoryStream();
            originalStream.Position = 0;
            await originalStream.CopyToAsync(tempStream);
            tempStream.Position = 0;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

            using var content = new StreamContent(tempStream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            var response = await client.PostAsync(analyzeUrl, content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception("Erro ao analisar imagem: " + json);

            var result = JsonConvert.DeserializeObject<AdultContentResult>(json);
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

            return !(result.adult.isAdultContent || result.adult.isRacyContent || result.adult.isGoryContent);
        }

        public class AdultContentResult
        {
            public AdultResult adult { get; set; }
        }

        public class AdultResult
        {
            public bool isAdultContent { get; set; }
            public bool isRacyContent { get; set; }
            public bool isGoryContent { get; set; }
        }
    }
}
