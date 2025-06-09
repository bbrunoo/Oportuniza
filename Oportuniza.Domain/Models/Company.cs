namespace Oportuniza.Domain.Models
{
    public class Company
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? Desc { get; set; }
        public string? ImageUrl { get; set; }
        public byte[] PasswordHash { get; set; } //TODO - Integrar keycloak
        public byte[] PasswordSalt { get; set; }
        public bool IsAdmin { get; set; }
        public bool Active { get; set; }
        public ICollection<CompanyAreaOfInterest> CompanyAreasOfInterest { get; set; } = new List<CompanyAreaOfInterest>();

    }
}
