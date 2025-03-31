using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;

namespace Oportuniza.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("user")]
        public async Task<IEnumerable<UserDTO>> Get()
        {
            var user = await _userRepository.Get();
            return user.Select(u => new UserDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            });
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<UserByIdDTO>> GetById(Guid id)
        {
            var user = await _userRepository.GetById(id);

            if (user == null) return NotFound();

            var userDto = new UserByIdDTO
            {
                Name = user.Name,
                Email = user.Email,
            };
            return Ok(userDto);
        }
    }
}
