namespace Oportuniza.Domain.DTOs.Curriculum
{
    public class CurriculumResponseDto
    {
        public Guid Id { get; set; }
        public string Phone { get; set; }
        public string Objective { get; set; }
        public DateTime BirthDate { get; set; }
        public string CityName { get; set; }
        public IEnumerable<EducationDto> Educations { get; set; }
        public IEnumerable<ExperienceDto> Experiences { get; set; }
        public IEnumerable<CertificationDto> Certifications { get; set; }
    }
}
