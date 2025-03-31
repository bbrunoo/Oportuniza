namespace Oportuniza.Domain.Models
{
    public class User
    {
        public User()
        {
            
        }

        public User( string name, string email, byte[] passwordHash, byte[] passwordSalt, bool isACompany, string imageUrl)
        {
            Id = Guid.NewGuid(); 
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            IsACompany = isACompany;
            ImageUrl = imageUrl;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public byte[] PasswordHash { get; set; }  
        public byte[] PasswordSalt { get; set; }  
        public bool IsACompany { get; set; }
        public string? ImageUrl { get; set; }
    }
}