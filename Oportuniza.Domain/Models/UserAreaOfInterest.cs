using System.Text.Json.Serialization;

namespace Oportuniza.Domain.Models
{
    public class UserAreaOfInterest
    {
        public Guid Id { get; set; }
        public Guid AreaOfInterestId { get; set; }
        [JsonIgnore]
        public AreaOfInterest AreaOfInterest { get; set; }
        public bool Principal { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
