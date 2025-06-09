namespace Oportuniza.Domain.DTOs.Company
{
    public class CompanyDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Desc { get; set; }
        public bool Active { get; set; }
        public Guid UserId { get; set; }
        public string ManagerName { get; set; }
        public List<CompanyEmployeeDto> Employees { get; set; }
    }
}
