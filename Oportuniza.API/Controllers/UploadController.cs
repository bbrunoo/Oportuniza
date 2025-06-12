using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.API.Services;
using System.Security.Claims;

namespace Oportuniza.API.Controllers
{
    [Authorize]
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

        [HttpPost("upload-profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (file == null) return BadRequest("File not found");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                string containerName = "profile-images";
                var imageUrl = await _azureBlobService.UploadProfileImage(file, containerName, Guid.Parse(userId));
                return Ok(new {imageUrl});
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Aconteceu um erro ao enviar a imagem: {ex.Message}");
            }
        }

        [HttpPost("upload-publication-picture")]
        public async Task<IActionResult> UploadPublication(IFormFile file)
        {
            if (file == null) return BadRequest("File not found");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                string containerName = "publications";
                var imageUrl = await _azureBlobService.UploadPostImage(file, containerName, Guid.Parse(userId));
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Aconteceu um erro ao enviar a imagem: {ex.Message}");
            }
        }
    }
}
