using System.ComponentModel.DataAnnotations;

namespace Oportuniza.Domain.DTOs.Curriculum
{
    public class ExperienceDto
    {
        [Required]
        public string Company { get; set; }
        [Required]
        public string Role { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
