namespace Oportuniza.Domain.Models
{
    public class CompanyRole
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<CompanyEmployee> Employees { get; set; } = new List<CompanyEmployee>();
    }
}
