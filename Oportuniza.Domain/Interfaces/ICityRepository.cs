using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface ICityRepository : IRepository<City>
    {
        Task<City?> GetByNameAsync(string name);
        Task<IEnumerable<City>> GetByUfAsync(string uf);
        Task DeleteAllAsync();
        Task<IEnumerable<City>> GetCitiesAsync(string? uf, string? name);
    }
}
