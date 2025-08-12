using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
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
        private readonly OpenAIService _openAIService;
        public UserController(IUserRepository userRepository, IMapper mapper, OpenAIService openAIService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _openAIService = openAIService;
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + "KeycloakScheme")]
        public async Task<IActionResult> GetOwnProfile()
        {
            var userUniqueId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userUniqueId))
                return Unauthorized("User unique identifier (NameIdentifier) claim not found in token.");

            var identityProvider = User.FindFirst("idp")?.Value;

            var email = User.FindFirst("preferred_username")?.Value ??
                        User.FindFirst(ClaimTypes.Email)?.Value;

            var name = User.FindFirst("name")?.Value ??
                       User.FindFirst(ClaimTypes.Name)?.Value ??
                       User.FindFirst("given_name")?.Value;

            Console.WriteLine($"Backend: Profile request from IDP: {identityProvider ?? "Azure AD"}, UserID: {userUniqueId}, Email: {email}, Name: {name}");

            var user = await _userRepository.GetByIdentityProviderIdAsync(userUniqueId, identityProvider ?? "Azure AD");

            if (user == null)
            {
                Guid userIdFromToken;
                if (Guid.TryParse(userUniqueId, out userIdFromToken))
                {
                    var finalName = await _openAIService.GetFirstEmailName(email);

                    var newUser = new User
                    {
                        Id = userIdFromToken,
                        IdentityProviderId = userUniqueId,
                        IdentityProvider = identityProvider ?? "Azure AD",
                        Email = email,
                        Name = Convert.ToString(finalName),
                        FullName = Convert.ToString(finalName)
                    };

                    await _userRepository.AddAsync(newUser);
                    user = newUser;
                    Console.WriteLine($"Backend: Novo perfil criado e salvo para {email} via {identityProvider ?? "Azure AD"}.");
                }
                //else
                //{

                //    var newUser = new User
                //    {
                //        Id = Guid.NewGuid(),
                //        IdentityProviderId = userUniqueId,
                //        IdentityProvider = identityProvider ?? "Azure AD",
                //        Email = email,
                //        Name =  Convert.ToString(finalName),
                //        FullName = Convert.ToString(finalName)
                //    };

                //    await _userRepository.AddAsync(newUser);
                //    user = newUser;
                //    Console.WriteLine($"Backend: Novo perfil criado e salvo com ID gerado para {email} via {identityProvider ?? "Azure AD"}.");
                //}
            }

            var response = _mapper.Map<UserDTO>(user);
            return StatusCode(200, response);
        }

        private string GenerateFirstNameFromEmail(string? email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@'))
                return "Usuário";

            string beforeAt = email.Split('@')[0];
            string firstName = beforeAt.Split(new[] { '.', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

            if (firstName.Length > 0)
                firstName = char.ToUpper(firstName[0]) + firstName.Substring(1).ToLower();

            return firstName;
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
