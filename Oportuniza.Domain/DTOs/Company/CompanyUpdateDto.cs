using System.ComponentModel.DataAnnotations;

namespace Oportuniza.Domain.DTOs.Company
{
    public class CompanyUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }
        public string ImageUrl { get; set; }
    }
}