using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.DTOs.Curriculum
{
    public class CurriculumDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }
        public string Objective { get; set; }
        public DateTime BirthDate { get; set; }

        public Guid CityId { get; set; }
        public string CityName { get; set; }
        public string Uf { get; set; } 

        public List<EducationDto> Educations { get; set; }
        public List<ExperienceDto> Experiences { get; set; }
        public List<CertificationDto> Certifications { get; set; }
    }
}
