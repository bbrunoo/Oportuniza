using System.ComponentModel.DataAnnotations;

namespace Oportuniza.Domain.DTOs.Curriculum
{
    public class CurriculumCreateDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [Phone]
        [StringLength(15, MinimumLength = 8)]
        public string Phone { get; set; }

        [Required]
        [MinLength(10)]
        public string Objective { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required]
        public Guid CityId { get; set; }

        public List<EducationDto> Educations { get; set; } = new();
        public List<ExperienceDto> Experiences { get; set; } = new();
        public List<CertificationDto> Certifications { get; set; } = new();
    }
}
