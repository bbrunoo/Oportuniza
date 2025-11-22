using Oportuniza.Domain.Enums;

namespace Oportuniza.Domain.Models
{
    public class Company
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool Active { get; set; }
        public Guid UserId { get; set; }
        public virtual User Manager { get; set; }
        public string ImageUrl { get; set; }
        public string CityState { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Cnpj { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public CompanyAvailable IsActive { get; set; } = CompanyAvailable.Active;
        public ICollection<CompanyEmployee> Employees { get; set; } = new List<CompanyEmployee>();
        public virtual ICollection<Publication> AuthoredPublications { get; set; }
    }
}
