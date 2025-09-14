using Oportuniza.Domain.DTOs.Publication;

namespace Oportuniza.Domain.DTOs.Extra
{
    public class UserApplicationDto
    {
        public Guid Id { get; set; }
        public PublicationDto Publication { get; set; }
    }
}
