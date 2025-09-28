using System.ComponentModel.DataAnnotations;

namespace Oportuniza.Domain.DTOs.Company
{
    public class EmployeeRegisterRequestDto
    {
        public Guid CompanyId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? EmployeeName { get; set; }
        public string? ImageUrl { get; set; } 
    }
}
