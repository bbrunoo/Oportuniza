using Microsoft.AspNetCore.Http;
using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface IActiveContextService
    {
        Task<(Guid Id, string Type, string? Role)> GetActiveContextAsync(HttpContext httpContext);
        Task<User?> GetActiveUserAsync(HttpContext httpContext);
        Task<Company?> GetActiveCompanyAsync(HttpContext httpContext);
    }

}
