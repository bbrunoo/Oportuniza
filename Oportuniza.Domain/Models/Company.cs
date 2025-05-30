using System.Text.Json.Serialization;

namespace Oportuniza.Domain.Models
{
    public class Company
    {
        public Guid Id { get; set; }
        public Guid CreatedByUserId { get; set; }
        [JsonIgnore]
        public User CreatedByUser { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<CompanyUser> CompanyUsers { get; set; } = new List<CompanyUser>();
    }
}
