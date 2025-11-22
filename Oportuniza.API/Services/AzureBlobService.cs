using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;

namespace Oportuniza.API.Services
{
    public class AzureBlobService
    {
        private readonly string _connectionString;
        private readonly string _moderatorEndpoint;
        private readonly string _moderatorApiKey;

        public AzureBlobService(IConfiguration configuration)
        {
            _connectionString = configuration["Azure:ConnectionString"];
            _moderatorApiKey = configuration["Azure:Moderator:ApiKey"];
            _moderatorEndpoint = configuration["Azure:Moderator:Endpoint"];
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentNullException(nameof(file), "Arquivo inválido.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            using var analysisStream = new MemoryStream(memoryStream.ToArray());

            memoryStream.Position = 0;

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient("images");
            await blobContainerClient.CreateIfNotExistsAsync();

            string blobName = $"{Guid.NewGuid()}-{file.FileName}";
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadProfileImage(IFormFile file, string containerName, Guid userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentNullException(nameof(file), "Arquivo inválido.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            using var analysisStream = new MemoryStream(memoryStream.ToArray());

            //bool isSafe = await IsImageSafeAsync(analysisStream, file.ContentType);
            //if (!isSafe)
            //    throw new Exception("Imagem imprópria detectada. Upload bloqueado.");

            memoryStream.Position = 0;

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            string blobName = $"{userId}";
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadPostImage(IFormFile file, string containerName, Guid userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentNullException(nameof(file), "Arquivo inválido.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            using var analysisStream = new MemoryStream(memoryStream.ToArray());

            //bool isSafe = await IsImageSafeAsync(analysisStream, file.ContentType);
            //if (!isSafe)
            //    throw new Exception("Imagem imprópria detectada. Upload bloqueado.");

            memoryStream.Position = 0;

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            string blobName = $"publication-{userId}-{Guid.NewGuid()}";
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadCompanyImage(IFormFile file, string containerName, Guid userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentNullException(nameof(file), "Arquivo inválido.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            using var analysisStream = new MemoryStream(memoryStream.ToArray());
            bool isSafe = await IsImageSafeAsync(analysisStream, file.ContentType);
            if (!isSafe)
                throw new Exception("Imagem imprópria detectada. Upload bloqueado.");

            memoryStream.Position = 0;

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            string blobName = $"company-{userId}-{Guid.NewGuid()}";
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadResumeAsync(IFormFile file, string containerName, Guid userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentNullException(nameof(file), "Arquivo inválido.");

            var allowedExtensions = new[] { ".pdf", ".docx", ".doc" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                throw new InvalidOperationException("Apenas arquivos PDF ou Word são permitidos.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            string blobName = $"resume-{userId}-{Guid.NewGuid()}{fileExtension}";
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blobClient.Uri.ToString();
        }

        public async Task<bool> IsImageSafeAsync(Stream imageStream, string contentType)
        {
            string endpoint = _moderatorEndpoint;
            string apiKey = _moderatorApiKey;

            var analyzeUrl = $"{endpoint}vision/v3.2/analyze?visualFeatures=Adult";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

            imageStream.Position = 0;

            using var content = new StreamContent(imageStream);
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
