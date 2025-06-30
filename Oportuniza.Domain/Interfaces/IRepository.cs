using System.Linq.Expressions;

namespace Oportuniza.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(); // simples

        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes); // inclui navegação

        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            params Expression<Func<T, object>>[] includes); // mais completa

        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>> filter,
            Expression<Func<T, object>> include,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null); // NOVA versão

        Task<T> GetByIdAsync(Guid id);
        Task<T> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(Guid id);

    }
}