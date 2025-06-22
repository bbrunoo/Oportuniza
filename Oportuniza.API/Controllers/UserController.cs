using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
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
        public UserController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
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
            var userObjectIdString = User.FindFirst(ClaimConstants.ObjectId)?.Value;

            if (string.IsNullOrEmpty(userObjectIdString))
            {
      
                return Unauthorized("User Object ID claim not found in token.");
            }

            Guid azureAdObjectId;
            if (!Guid.TryParse(userObjectIdString, out azureAdObjectId))
            {
                return BadRequest("Invalid User Object ID format in token.");
            }

            var user = await _userRepository.GetByAzureAdObjectIdAsync(azureAdObjectId);

            if (user == null)
            {

                var email = User.FindFirst("preferred_username")?.Value;
                var name = User.FindFirst("name")?.Value;
                var givenName = User.FindFirst("given_name")?.Value;
                var familyName = User.FindFirst("family_name")?.Value;

                var newUser = new User
                {
                    Id = Guid.Parse(userObjectIdString),
                    AzureAdObjectId = azureAdObjectId,
                    Email = email,
                    Name = name,
                    FullName = name
                };

                await _userRepository.AddAsync(newUser);

                return NotFound($"User with Azure AD Object ID '{userObjectIdString}' not found in local database. Consider implementing Just-In-Time provisioning.");
            }

            var response = _mapper.Map<UserDTO>(user);
            return StatusCode(200, response);
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
    }
}
