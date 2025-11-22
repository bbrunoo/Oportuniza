using Oportuniza.Domain.Enums;

namespace Oportuniza.Domain.DTOs.Company
{
    public class CompanyEmployeeDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Roles { get; set; }
        public bool CanPostJobs { get; set; }
        public string UserEmail { get; set; }
        public string ImageUrl { get; set; }
        public CompanyEmployeeStatus IsActive { get; set; }
    }
}
