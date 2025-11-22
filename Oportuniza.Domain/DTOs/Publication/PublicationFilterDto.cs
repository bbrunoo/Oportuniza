namespace Oportuniza.Domain.DTOs.Publication
{
    public class PublicationFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Local { get; set; }
        public List<string>? Contracts { get; set; }
        public List<string>? Shifts { get; set; }
        public string? SalaryRange { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? RadiusKm { get; set; } = 0;
    }
}
