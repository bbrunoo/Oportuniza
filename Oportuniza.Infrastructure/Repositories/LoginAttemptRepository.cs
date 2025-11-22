using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class LoginAttemptRepository : ILoginAttemptRepository
    {
        private readonly ApplicationDbContext _context;

        public LoginAttemptRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<LoginAttempt?> GetByIpAsync(string ip)
        {
            return _context.LoginAttempt.FirstOrDefaultAsync(x => x.IPAddress == ip);
        }

        public async Task AddAsync(LoginAttempt attempt)
        {
            _context.Add(attempt);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LoginAttempt attempt)
        {
            _context.Update(attempt);
            await _context.SaveChangesAsync();
        }
    }

}
