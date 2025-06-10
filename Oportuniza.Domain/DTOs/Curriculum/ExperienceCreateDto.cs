namespace Oportuniza.Domain.DTOs.Curriculum
{
    public class ExperienceCreateDto
    {
        public string? Position { get; set; }
        public string Company { get; set; }
        public string Role { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPrincipal { get; set; }
    }
}
