namespace Oportuniza.Domain.DTOs.AreasOfInterest
{
    public class CompanyAreaDto
    {
        public Guid Id { get; set; }
        public Guid AreaOfInterestId { get; set; }
        public string AreaName { get; set; }
        public bool Principal { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}
