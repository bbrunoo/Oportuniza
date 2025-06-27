namespace Oportuniza.Domain.DTOs.Curriculum
{
    public class EducationCreateDto
    {
        public string Institution { get; set; }
        public string? Course { get; set; }
        public string Degree { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool InProgress { get; set; }
    }
}
