namespace Oportuniza.Domain.DTOs.Company
{
    public class CompanyEmployeeDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public bool CanPostJobs { get; set; }
    }
}
