using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.DTOs.Curriculum
{
    public class CurriculumDto
    {
        public Guid Id { get; set; }
        public string Phone { get; set; }
        public string Objective { get; set; }
        public DateTime BirthDate { get; set; }
        public string UserName { get; set; }
        public List<Education> Educations { get; set; }
        public List<Experience> Experiences { get; set; }
        public List<Certification> Certifications { get; set; }
    }

}
