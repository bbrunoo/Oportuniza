using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.DTOs.Curriculum;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Globalization;
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
        public async Task<IActionResult> GetOwnProfile()
        {  
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null) return NotFound();

            var response = _mapper.Map<UserDTO>(user);
            return StatusCode(200, response);
        }

        [HttpPut("completar-perfil/{id}")]
        public async Task<IActionResult> CompletePerfil(Guid id, [FromBody] CompleteProfileDTO model)
        {
            var user = await _userRepository.GetByIdWithInterests(id);
            if (user == null) return NotFound("Usuario nao encontrado");

            user.Phone = model.Phone;
            user.FullName = model.FullName;
            user.ImageUrl = model.ImageUrl;

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
