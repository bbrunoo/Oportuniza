using System.ComponentModel.DataAnnotations;

namespace Oportuniza.Domain.DTOs.Curriculum
{
    public class EducationDto
    {
        [Required]
        public string Institution { get; set; }
        [Required]
        public string Degree { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

}
