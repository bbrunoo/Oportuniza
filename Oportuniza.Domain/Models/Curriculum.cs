using System.Text.Json.Serialization;

namespace Oportuniza.Domain.Models
{
    public class Curriculum
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public string Phone { get; set; }
        public string Objective { get; set; }
        public DateTime BirthDate { get; set; }
        public Guid CityId { get; set; }
        [JsonIgnore]
        public City City { get; set; }
        public ICollection<Education> Educations { get; set; } = new List<Education>();
        public ICollection<Experience> Experiences { get; set; } = new List<Experience>();
        public ICollection<Certification> Certifications { get; set; } = new List<Certification>();
    }
}
