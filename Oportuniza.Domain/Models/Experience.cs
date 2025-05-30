using System.Text.Json.Serialization;

namespace Oportuniza.Domain.Models
{
    public class Experience
    {
        public Guid Id { get; set; }
        public string? Position { get; set; }
        public string Company { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPrincipal { get; set; }
        public Guid CurriculumId { get; set; }
        [JsonIgnore]
        public Curriculum Curriculum { get; set; }
    }
}
