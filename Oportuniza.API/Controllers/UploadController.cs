using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.API.Services;

namespace Oportuniza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly AzureBlobService _azureBlobService;
        public UploadController(AzureBlobService azureBlobService)
        {
            _azureBlobService = azureBlobService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null) return BadRequest("File not found");

            var url = await _azureBlobService.UploadImageAsync(file);
            return Ok(new {imageUrl = url });
        }
    }
}
