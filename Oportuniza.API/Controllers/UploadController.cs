using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.API.Services;
using System.Security.Claims;

namespace Oportuniza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized(new { message = "Usuário não autenticado" });

            if (!Guid.TryParse(userIdStr, out Guid userId))
                return BadRequest(new { message = "UserId inválido" });

            try
            {
                string containerName = "profile-images";
                var imageUrl = await _azureBlobService.UploadProfileImage(file, containerName, userId);
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro ao enviar a imagem: {ex.Message}" });
            }
        }

        [HttpPost("upload-publication-picture")]
        public async Task<IActionResult> UploadPublication(IFormFile file)
        {
            if (file == null) return BadRequest("File not found");

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized(new { message = "Usuário não autenticado" });

            if (!Guid.TryParse(userIdStr, out Guid userId))
                return BadRequest(new { message = "UserId inválido" });

            try
            {
                string containerName = "publications";
                var imageUrl = await _azureBlobService.UploadPostImage(file, containerName, userId);
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro ao enviar a imagem: {ex.Message}" });
            }
        }

        [HttpPost("upload-company-picture")]
        public async Task<IActionResult> UploadCompanyImage(IFormFile file)
        {
            if (file == null) return BadRequest("File not found");

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized(new { message = "Usuário não autenticado" });

            if (!Guid.TryParse(userIdStr, out Guid userId))
                return BadRequest(new { message = "UserId inválido" });

            try
            {
                string containerName = "company";
                var imageUrl = await _azureBlobService.UploadCompanyImage(file, containerName, userId);
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro ao enviar a imagem: {ex.Message}" });
            }
        }
    }
}
