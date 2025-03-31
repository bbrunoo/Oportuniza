
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oportuniza.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> Add(User user);
        Task<IEnumerable<User>> Get();
        Task<User> GetById(Guid id);
        Task<EditUserDTO> Edit(Guid id, EditUserDTO editUserDto);
        Task<bool> Exist(Guid id);
        Task<User> Delete(User user);
    }
}
