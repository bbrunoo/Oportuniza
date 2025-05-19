namespace Oportuniza.Domain.Models
{
    public class User
    {
        public User() { }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public bool IsACompany { get; set; }
        public string? ImageUrl { get; set; }
        public string? Interests { get; set; }

    }
}