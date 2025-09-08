namespace Oportuniza.Domain.DTOs.Publication
{
    public class PublicationFilterDto
    {
public string? SearchTerm { get; set; }
        public string? Local { get; set; }
        public List<string>? Contracts { get; set; } // Tipo correto para lista de strings
        public List<string>? Shifts { get; set; } // Tipo correto para lista de strings
        public string? SalaryRange { get; set; }
    }
}
