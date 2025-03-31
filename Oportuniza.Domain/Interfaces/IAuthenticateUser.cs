using Oportuniza.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oportuniza.Domain.Interfaces
{
    public interface IAuthenticateUser
    {
        Task<bool> AuthenticateAsync(string email, string senha);
        Task<bool> UserExists(string email);
        public string GenerateToken(Guid id, string email, bool isACompany);
        public Task<User> GetUserByEmail(string email);
    }
}
