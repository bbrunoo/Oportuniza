using System.ComponentModel.DataAnnotations;

namespace Oportuniza.Domain.DTOs.Curriculum
{
    public class CertificationDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}
