using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oportuniza.API.Viewmodel;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Security.Cryptography;
using System.Text;
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
            if (model == null || IsInvalidInput(model.Name) || IsInvalidInput(model.Email) || IsInvalidInput(model.Password))
                return BadRequest("Todos os campos são obrigatórios.");

            if (model.Email.Contains(" "))
                return BadRequest("O e-mail não pode conter espaços em branco.");

            if (model.Password.Contains(" "))
                return BadRequest("A senha não pode conter espaços em branco.");

            if (!IsValidEmail(model.Email))
                return BadRequest("Formato de e-mail inválido.");

            if (model.Password.Length < 8)
                return BadRequest("A senha deve conter no mínimo 8 caracteres.");

            var emailJaExiste = await _authenticateUser.UserExists(model.Email);
            if (emailJaExiste)
                return Conflict("Este e-mail já está cadastrado.");

            CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = model.Name,
                Name = model.Email.Split('@')[0],
                Email = model.Email.Trim(),
                IsACompany = false,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            var result = await _userRepository.Add(user);
            if (result == null)
                return StatusCode(500, "Erro interno ao registrar o usuário.");

            return Ok(new UserByIdDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                isACompany = user.IsACompany
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserToken>> Login([FromBody] LoginRequestDto loginRequestDTO)
        {
            if (loginRequestDTO == null || IsInvalidInput(loginRequestDTO.Email) || IsInvalidInput(loginRequestDTO.Password))
                return BadRequest("Todos os campos são obrigatórios.");

            if (loginRequestDTO.Password.Contains(" "))
                return BadRequest("A senha não pode conter espaços.");

            if (!IsValidEmail(loginRequestDTO.Email))
                return BadRequest("Formato de e-mail inválido.");

            var ip = GetClientIp();
            var (isAuthenticated, errorMessage, statusCode) = await _authenticateUser.AuthenticateAsync(
                loginRequestDTO.Email, loginRequestDTO.Password, ip
            );

            if (!isAuthenticated)
            {
                if (statusCode.HasValue)
                    return StatusCode(statusCode.Value, errorMessage ?? "Erro de autenticação.");

                return Unauthorized(errorMessage);
            }

            var user = await _authenticateUser.GetUserByEmail(loginRequestDTO.Email);
            var token = _authenticateUser.GenerateToken(user.Id, user.Email, user.IsACompany, user.Name);

            return Ok(new UserToken { Token = token });
        }

        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }

        private bool IsInvalidInput(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
        private string GetClientIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}