using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.Candidates;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Infrastructure.Repositories;
using System.Security.Claims;

namespace Oportuniza.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class ProfileController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        public ProfileController(IUserRepository userRepository, ICompanyRepository companyRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
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
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Token 'sub' claim is missing.");

            var companyIdClaim = User.FindFirst("company_id")?.Value;
            if (Guid.TryParse(companyIdClaim, out Guid companyContextId))
            {
                var company = await _companyRepository.GetByIdAsync(companyContextId);
                if (company == null)
                    return NotFound("Empresa não encontrada.");

                var companyDto = _mapper.Map<CompanyDTO>(company);
                return Ok(companyDto);
            }

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return NotFound("Usuário não encontrado no banco de dados local.");

            var userDto = _mapper.Map<UserDTO>(user);
            return Ok(userDto);
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
