using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class CnpjCacheRepository : ICnpjCacheRepository
    {
        private readonly ApplicationDbContext _context;

        public CnpjCacheRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CNPJCache?> GetAsync(string cnpj)
        {
            return await _context.CnpjCache
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Cnpj == cnpj);
        }

        public async Task UpsertAsync(string cnpj, string situacao)
        {
            var entity = await _context.CnpjCache.FirstOrDefaultAsync(x => x.Cnpj == cnpj);

            if (entity == null)
            {
                entity = new CNPJCache
                {
                    Cnpj = cnpj,
                    Situacao = situacao,
                    AtualizadoEm = DateTime.UtcNow
                };
                _context.CnpjCache.Add(entity);
            }
            else
            {
                entity.Situacao = situacao;
                entity.AtualizadoEm = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
