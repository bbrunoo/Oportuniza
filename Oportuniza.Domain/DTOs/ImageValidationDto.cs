using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Oportuniza.Domain.DTOs
{
    public class ImageValidationDto
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
