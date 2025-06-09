namespace Oportuniza.Domain.Models
{
    public class Company
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Desc { get; set; }
        public bool Active { get; set; }
        public Guid UserId { get; set; }
        public User Manager { get; set; }
        public ICollection<CompanyEmployee> Employees  { get; set; } = new List<CompanyEmployee>();
    }
}
