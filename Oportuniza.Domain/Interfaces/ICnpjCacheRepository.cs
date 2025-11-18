using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface ICnpjCacheRepository
    {
        Task<CNPJCache?> GetAsync(string cnpj);
        Task UpsertAsync(string cnpj, string situacao);
    }
}
