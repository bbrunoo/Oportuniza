using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

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
        public async Task<EditUserDTO> Edit(Guid id, EditUserDTO editUserDto)
        {
            var userExist = await _context.User.FirstOrDefaultAsync(x => x.Id == id);
            if (userExist == null) { throw new KeyNotFoundException("User not found"); }

            userExist.Name = editUserDto.Name;

            _context.User.Update(userExist);
            await _context.SaveChangesAsync();
            return editUserDto;
        }
        public async Task<bool> Exist(Guid id)
        {
            return await _context.User.AnyAsync(c => c.Id == id);
        }
        public async Task<IEnumerable<User>> Get()
        {
            return await _context.User.ToListAsync();
        }
        public async Task<User> GetById(Guid id)
        {
            return await _context.User.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<UserInfoDTO> GetUserInfoAsync(Guid id)
        {
            var userInfo = await _context.User
                .Where(u => u.Id == id)
                .Select(u => new UserInfoDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    isACompany = u.IsACompany
                })
                .FirstOrDefaultAsync();

            if (userInfo == null) throw new KeyNotFoundException("User not found");
            return userInfo;
        }
    }
}
