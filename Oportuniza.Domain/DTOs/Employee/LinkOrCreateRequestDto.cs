namespace Oportuniza.Domain.DTOs.Employee
{
    public class LinkOrCreateRequestDto
    {
        public string Email { get; set; } = "";
        public Guid CompanyId { get; set; }
        public string? EmployeeName { get; set; }
    }
}
