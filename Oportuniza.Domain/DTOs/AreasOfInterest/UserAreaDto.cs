namespace Oportuniza.Domain.DTOs.AreasOfInterest
{
    public class UserAreaDto
    {
        public Guid Id { get; set; }
        public Guid AreaOfInterestId { get; set; }
        public string AreaName { get; set; }
        public bool Principal { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
    }
}
