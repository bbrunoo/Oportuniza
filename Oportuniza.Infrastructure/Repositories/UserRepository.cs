using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> Add(User user)
        {

            if (user.PasswordHash == null || user.PasswordSalt == null)
                throw new ArgumentException("Hash e salt da senha são obrigatórios.");

            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<User> Delete(User user)
        {
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<bool> Exist(Guid id)
        {
            return await _context.User.AnyAsync(c => c.Id == id);
        }
        public async Task<IEnumerable<User>> Get()
        {
            return await _context.User.ToListAsync();
        }

        public async Task<IEnumerable<AllUsersInfoDTO>> GetAllUserInfosAsync()
        {
            return await _context.User
                .Select(u => new AllUsersInfoDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    FullName = u.FullName,
                    imageUrl = u.ImageUrl
                })
                .ToListAsync();
        }

        public async Task<User?> GetById(Guid id)
        {
            return await _context.User.FindAsync(id);
        }

        public async Task<User?> GetByIdWithInterests(Guid id)
        {
            return await _context.User
                    .Include(u => u.UserAreasOfInterest)
                    .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<UserInfoDTO> GetUserInfoAsync(Guid id)
        {
            var userInfo = await _context.User
                .Where(u => u.Id == id)
                .Select(u => new UserInfoDTO
                {
                    Name = u.Name,
                    FullName = u.FullName,
                    Email = u.Email,
                })
                .FirstOrDefaultAsync();

            if (userInfo == null) throw new KeyNotFoundException("User not found");
            return userInfo;
        }
        public async Task<bool> Update(User user)
        {
            _context.User.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
