using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;

namespace Oportuniza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ProfileController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public ProfileController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfileById(Guid id)
        {
            var user = await _userRepository.GetById(id);

            if (user == null) return NotFound();

            var dto = new UserProfileDTO
            {
                Id = user.Id,
                Name = user.Name
            };
            return Ok(dto);
        }
    }
}
