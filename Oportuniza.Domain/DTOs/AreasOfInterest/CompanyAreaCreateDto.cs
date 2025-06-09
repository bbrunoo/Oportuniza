using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.DTOs.AreasOfInterest
{
    public class CompanyAreaCreateDto
    {
        public Guid AreaOfInterestId { get; set; }
        public bool Principal { get; set; }
        public Guid CompanyId { get; set; }
    }
}
