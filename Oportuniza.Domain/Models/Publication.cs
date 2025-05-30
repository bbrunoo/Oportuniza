using System.Text.Json.Serialization;

namespace Oportuniza.Domain.Models
{
    public class Publication
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public string ImageUrl { get; set; }
        public bool Expired { get; set; }
        public Guid CompanyId { get; set; }
        [JsonIgnore]
        public Company Company { get; set; }
        public Guid PublishedByUserId { get; set; }
        [JsonIgnore]
        public User PublishedByUser { get; set; }
    }
}
