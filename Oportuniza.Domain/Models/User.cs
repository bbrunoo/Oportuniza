namespace Oportuniza.Domain.Models
{
    public enum UserType
    {
        Personal = 0,
        CompanyManager = 1,
        CompanyEmployee = 2
    }

    public class User
    {
        public User() { }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public byte[] PasswordHash { get; set; } //TODO - Integrar keycloak
        public byte[] PasswordSalt { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAdmin { get; set; }
        public bool Active { get; set; }
        public UserType UserType { get; set; }
        public Company CompanyOwned { get; set; }
        public ICollection<CompanyEmployee> CompanyLinks { get; set; } = new List<CompanyEmployee>();
        public ICollection<UserAreaOfInterest> UserAreasOfInterest { get; set; } = new List<UserAreaOfInterest>();
        public ICollection<Curriculum> Curriculum { get; set; } = new List<Curriculum>();
    }
}