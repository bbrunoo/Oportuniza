using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;

namespace Oportuniza.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class ProfileController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public ProfileController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

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

        [HttpGet("profile-data/{id}")]
        public async Task<IActionResult> GetProfileDatas(Guid id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) return NotFound("Usuario nao encontrado");

            var result = await _userRepository.GetUserInfoAsync(id);
            return Ok(result);
        }

        [HttpGet("all-profiles")]
        public async Task<ActionResult<IEnumerable<AllUsersInfoDTO>>> GetAllUserProfiles()
        {
            var profiles = await _userRepository.GetAllUserInfosAsync();
            return Ok(profiles);
        }
    }
}
