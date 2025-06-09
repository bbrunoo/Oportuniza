namespace Oportuniza.Domain.DTOs.Company
{
    public class CompanyCreateDto
    {
        public string Name { get; set; }
        public string? Desc { get; set; }
        public Guid UserId { get; set; } // ID do gerente
    }
}
