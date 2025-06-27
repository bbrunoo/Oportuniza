using System.Text.Json.Serialization;

namespace Oportuniza.Domain.Models
{
    public class Education
    {
        public Guid Id { get; set; }
        public string Institution { get; set; }
        public string? Course { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool InProgress { get; set; }
        public Guid CurriculumId { get; set; }
        [JsonIgnore]
        public Curriculum Curriculum { get; set; }
    }
}
