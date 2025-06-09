namespace Oportuniza.Domain.Models
{
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
        public ICollection<UserAreaOfInterest> UserAreasOfInterest { get; set; } = new List<UserAreaOfInterest>();
        public ICollection<Curriculum> Curriculum { get; set; } = new List<Curriculum>();
    }
}