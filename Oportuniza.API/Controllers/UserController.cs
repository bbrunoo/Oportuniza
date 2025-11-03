using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.API.Services;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Security.Claims;

namespace Oportuniza.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICompanyEmployeeRepository _companyEmployeeRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly AzureBlobService _azureBlobService;
        private readonly GeolocationService _geolocationService;

        public UserController(IUserRepository userRepository, IMapper mapper, ICompanyEmployeeRepository companyEmployeeRepository, ICompanyRepository companyRepository, AzureBlobService azureBlobService, GeolocationService geolocationService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _companyEmployeeRepository = companyEmployeeRepository;
            _companyRepository = companyRepository;
            _azureBlobService = azureBlobService;
            _geolocationService = geolocationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = await _userRepository.GetAllAsync();
            if (user == null) return NotFound("Usuario não encontrado.");
            var response = _mapper.Map<List<UserDTO>>(user);
            return StatusCode(200, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null) return NotFound();

            var response = _mapper.Map<UserDTO>(user);
            return StatusCode(200, response);
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetOwnProfile()
        {
            var companyIdClaim = User.FindFirst("company_id")?.Value;
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Token 'sub' claim is missing.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return NotFound("User not found.");

            if (!string.IsNullOrEmpty(companyIdClaim) && Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                var company = await _companyRepository.GetByIdAsync(companyId);
                if (company == null)
                    return NotFound("Company not found.");

                var employee = await _companyEmployeeRepository.GetByUserAndCompanyAsync(user.Id, companyId);
                if (employee == null)
                    return Forbid("You are not associated with this company.");

                return Ok(new
                {
                    isCompany = true,
                    id = company.Id,
                    name = company.Name,
                    email = user.Email,
                    phone = user.Phone,
                    imageUrl = company.ImageUrl,
                    role = employee.CompanyRole.Name
                });
            }

            var response = _mapper.Map<UserDTO>(user);

            return Ok(new
            {
                isCompany = false,
                id = response.Id,
                name = response.Name,
                email = response.Email,
                phone = response.Phone,
                imageUrl = response.ImageUrl,
                local = response.Local,
                isProfileCompleted = response.IsProfileCompleted
            });
        }

        [HttpGet("getUserId")]
        [Authorize]
        public async Task<IActionResult> GetUserId()
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(keycloakId))
            {
                return Unauthorized("Token 'sub' claim is missing.");
            }

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);

            if (user == null)
            {
                return NotFound("User not found in local database.");
            }

            return Ok(new { id = user.Id });
        }

        [HttpPut("completar-perfil/{id}")]
        public async Task<IActionResult> CompletePerfil(Guid id, [FromBody] CompleteProfileDTO model)
        {
            var user = await _userRepository.GetByIdWithInterests(id);
            if (user == null) return NotFound("Usuario nao encontrado");

            user.FullName = model.FullName;
            user.ImageUrl = model.ImageUrl;
            user.IsProfileCompleted = true;

            user.UserAreasOfInterest.Clear();

            foreach (var areaId in model.AreaOfInterestIds)
            {
                user.UserAreasOfInterest.Add(new UserAreaOfInterest
                {
                    UserId = id,
                    AreaOfInterestId = areaId
                });
            }

            var result = await _userRepository.UpdateAsync(user);
            if (result == null) return StatusCode(500, "Erro ao atualizar perfil");

            return NoContent();
        }

        [HttpPut("editar-perfil")]
        [Authorize]
        public async Task<IActionResult> EditProfile([FromForm] EditUserProfileDto dto, IFormFile? image)
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Token inválido: claim 'sub' ausente.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return NotFound("Usuário não encontrado no banco de dados local.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                user.FullName = dto.Name.Trim();
                user.Name = dto.Name.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Location))
            {
                user.Local = dto.Location.Trim();

                try
                {
                    var (latitude, longitude) = await _geolocationService.GetCoordinatesAsync(dto.Location);
                    user.Latitude = latitude;
                    user.Longitude = longitude;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Falha ao geocodificar localização: {ex.Message}");
                }
            }

            if (image != null && image.Length > 0)
            {
                try
                {
                    var imageUrl = await _azureBlobService.UploadImageAsync(image);
                    user.ImageUrl = imageUrl;
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Erro ao fazer upload da imagem: {ex.Message}");
                }
            }

            var result = await _userRepository.UpdateAsync(user);
            if (result == null)
                return StatusCode(500, "Erro ao atualizar perfil do usuário.");

            var response = new
            {
                name = user.Name,
                fullName = user.FullName,
                email = user.Email,
                location = user.Local,
                latitude = user.Latitude,
                longitude = user.Longitude,
                imageUrl = user.ImageUrl
            };

            return Ok(response);
        }
    }
}
