namespace Oportuniza.Domain.DTOs.AreasOfInterest
{
    public class UserAreaCreateDto
    {
        public Guid AreaOfInterestId { get; set; }
        public bool Principal { get; set; }
        public Guid UserId { get; set; }
    }
}
