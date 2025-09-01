using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface IPublicationRepository : IRepository<Publication>
    {
        Task<IEnumerable<Publication>> GetMyPublications(Guid id);
        Task<(IEnumerable<Publication>, int)> GetMyPublicationsPaged(Guid userId, int pageNumber, int pageSize);
    }
}
