using System.Text.Json.Serialization;

namespace Oportuniza.Domain.Models
{
    public class CompanyAreaOfInterest
    {
        public Guid Id { get; set; }
        public Guid AreaOfInterestId { get; set; }
        [JsonIgnore]
        public AreaOfInterest AreaOfInterest { get; set; }
        public bool Principal { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
