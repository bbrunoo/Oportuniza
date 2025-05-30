using System.Text.Json.Serialization;

namespace Oportuniza.Domain.Models
{
    public class CompanyUser
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public Guid CompanyId { get; set; }
        [JsonIgnore]
        public Company Company { get; set; }
        public DateTime LinkDate { get; set; }
        public bool Active { get; set; } = false;
    }
}
