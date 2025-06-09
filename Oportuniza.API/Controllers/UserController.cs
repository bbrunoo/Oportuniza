using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;
using System.Globalization;

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

        [HttpGet]
        public async Task<IEnumerable<UserDTO>> GetAll()
        {
            var user = await _userRepository.Get();
            return user.Select(u => new UserDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
            });
        }

        [HttpGet("{id}")]
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

        [HttpPut("completar-perfil/{id}")]
        public async Task<IActionResult> CompletePerfil(Guid id, [FromBody] CompleteProfileDTO model)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) return NotFound("Usuario nao encontrado");

            user.FullName = model.FullName;
            user.ImageUrl = model.ImageUrl;

            var result = await _userRepository.Update(user);
            if (!result) return StatusCode(500, "Erro ao atualizar perfil");

            return NoContent();
        }
    }
}
