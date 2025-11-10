using Oportuniza.Domain.DTOs.Publication;
using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface IPublicationRepository : IRepository<Publication>
    {
        Task<IEnumerable<Publication>> GetMyPublications(Guid id);
        Task<(IEnumerable<Publication>, int)> GetMyPublicationsPaged(Guid userId, int pageNumber, int pageSize);
        Task<IEnumerable<Publication>> FilterPublicationsAsync(PublicationFilterDto filters);
        Task<(IEnumerable<Publication> publications, int totalCount)> GetCompanyPublicationsPaged(
            Guid companyId, int pageNumber, int pageSize);
        Task<(IEnumerable<Publication>, int)> GetMyPublicationsPaged(Guid userId, int pageNumber, int pageSize, bool onlyPersonal);
        Task UpdateRangeAsync(IEnumerable<Publication> publications);
    }
}
