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
        public string? ImageUrl { get; set; }
        public bool IsAdmin { get; set; }
        public bool Active { get; set; }
        public bool IsProfileCompleted { get; set; }
        public string KeycloakId { get; set; }

        //public bool VerifiedEmail { get; set; }
        public UserType UserType { get; set; }
        public ICollection<CandidateApplication> CandidateApplication { get; set; }
        public ICollection<CompanyEmployee> CompanyLinks { get; set; } = new List<CompanyEmployee>();
        public ICollection<UserAreaOfInterest> UserAreasOfInterest { get; set; } = new List<UserAreaOfInterest>();
        public virtual ICollection<Publication> CreatedPublications { get; set; }
        public virtual ICollection<Publication> AuthoredAsUserPublications { get; set; }
        public virtual ICollection<Company> CompaniesOwned { get; set; } = new List<Company>();
    }
}