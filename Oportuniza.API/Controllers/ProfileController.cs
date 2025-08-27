using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.Candidates;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;
using System.Security.Claims;

namespace Oportuniza.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class ProfileController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public ProfileController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfileById(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null) return NotFound();

            var dto = new UserProfileDTO
            {
                Id = user.Id,
                Name = user.Name
            };
            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(keycloakId))
            {
                return Unauthorized("Token 'sub' claim is missing.");
            }

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);

            if (user == null)
            {
                return NotFound("Usuário não encontrado no banco de dados local.");
            }

            var own = await _userRepository.GetByIdAsync(user.Id);
            var result = _mapper.Map<UserDTO>(own);
            return Ok(result);
        }

        [HttpGet("profile-data/{id}")]
        public async Task<IActionResult> GetProfileDatas(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound("Usuario nao encontrado");

            var own = await _userRepository.GetUserInfoAsync(id);
            var result = _mapper.Map<IEnumerable<UserDTO>>(own);
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
