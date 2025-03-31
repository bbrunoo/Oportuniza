using Microsoft.AspNetCore.Mvc;
using Oportuniza.API.Viewmodel;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Text.RegularExpressions;

namespace Oportuniza.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticateUser _authenticateUser;

        public AuthController(IUserRepository userRepository, IAuthenticateUser authenticateUser)
        {
            _userRepository = userRepository;
            _authenticateUser = authenticateUser;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserToken>> Register([FromBody] UserRegisterViewmodel model)
        {
            if (model == null)
                return BadRequest("Wrong datas.");
            if (string.IsNullOrWhiteSpace(model.Name) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest("Todos os campos são obrigatórios.");
            }

            if (model.Email.Contains(" "))
            {
                return BadRequest("The emaail can't have empty spaces.");
            }

            if (model.Password.Contains(" "))
            {
                return BadRequest("The password can't have empty spaces.");
            }

            if (!IsValidEmail(model.Email))
            {
                return BadRequest("This email format is not accepted.");
            }

            if (model.Password.Length < 8)
            {
                return BadRequest("The password must have 8 or more characters.");
            }

            var emailExistente = await _authenticateUser.UserExists(model.Email);
            if (emailExistente) return BadRequest("Email já cadastrado.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Email = model.Email,
                Password = model.Password,
                IsACompany = model.isACompany
            };

            var users = await _userRepository.Add(user);
            var userReturn = new UserByIdDTO
            {
                Name = user.Name,
                Email = user.Email,
                isACompany = user.IsACompany
            };

            if (users == null) return BadRequest("A Error has ocorred");

            return Ok(userReturn);
        }
        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserToken>> Login(LoginRequestDto loginRequestDTO)
        {
            if (loginRequestDTO == null)
                return BadRequest("Dados inválidos.");

            if (string.IsNullOrWhiteSpace(loginRequestDTO.Email) ||
                string.IsNullOrWhiteSpace(loginRequestDTO.Password))
            {
                return BadRequest("Todos os campos são obrigatórios.");
            }

            if (loginRequestDTO.Password.Contains(" "))
            {
                return BadRequest("A senha não pode conter espaços em branco.");
            }

            if (!IsValidEmail(loginRequestDTO.Email))
            {
                return BadRequest("O formato do email é inválido.");
            }

            var existe = await _authenticateUser.UserExists(loginRequestDTO.Email);
            if (!existe) return BadRequest("User not exist.");

            var result = await _authenticateUser.AuthenticateAsync(loginRequestDTO.Email, loginRequestDTO.Password);
            if (!result) return Unauthorized("User or password is invalid.");

            var usuario = await _authenticateUser.GetUserByEmail(loginRequestDTO.Email);

            var token = _authenticateUser.GenerateToken(usuario.Id, usuario.Email, usuario.IsACompany);

            return new UserToken { Token = token };
        }
    }
}