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
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                throw new ArgumentException("The password is needed");
            }
            using (var hmac = new HMACSHA512())
            {
                user.PasswordSalt = hmac.Key;
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(user.Password));
            }
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
    }
}
