using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Security.Claims;

namespace Oportuniza.Infrastructure.Repositories
{
    public class ActiveContextService : IActiveContextService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        public ActiveContextService(IUserRepository userRepository, ICompanyRepository companyRepository)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
        }

        public async Task<Company?> GetActiveCompanyAsync(HttpContext httpContext)
        {
            var (id, type, _) = await GetActiveContextAsync(httpContext);
            if (type == "Company")
            {
                return await _companyRepository.GetByIdAsync(id);
            }
            return null;
        }

        public async Task<(Guid Id, string Type, string? Role)> GetActiveContextAsync(HttpContext httpContext)
        {
            if (httpContext.Items.TryGetValue("ActiveContext", out var contextObj) && contextObj is ActiveContext ctx)
            {
                return (ctx.Id, ctx.Type, ctx.Role);
            }

            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              httpContext.User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("Usuário não autenticado.");

            return (Guid.Parse(userIdClaim), "User", null);
        }

        public async Task<User?> GetActiveUserAsync(HttpContext httpContext)
        {
            var (id, type, _) = await GetActiveContextAsync(httpContext);
            if (type == "User")
            {
                return await _userRepository.GetByIdAsync(id);
            }
            return null;
        }
    }
}
