using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface IAreaOfInterest : IRepository<AreaOfInterest>
    {
        Task<AreaOfInterest?> GetByNameAsync(string name);
        Task DeleteAllAsync();
        Task<IEnumerable<AreaOfInterest>> GetAreasAsync(string? areaName);
    }
}
