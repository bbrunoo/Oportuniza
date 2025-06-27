namespace Oportuniza.Domain.DTOs.Curriculum
{
    public class CurriculumUpdateDto
    {
        public string Phone { get; set; }
        public string Objective { get; set; }
        public DateTime BirthDate { get; set; }
        public Guid CityId { get; set; }
    }
}
