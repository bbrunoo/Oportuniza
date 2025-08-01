using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Oportuniza.API.Viewmodel;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly KeycloakAuthService _authService;
        public AuthController(IUserRepository userRepository, IAuthenticateUser authenticateUser, IHttpClientFactory httpClientFactory, KeycloakAuthService authService)
        {
            _userRepository = userRepository;
            _authenticateUser = authenticateUser;
            _httpClientFactory = httpClientFactory;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserByIdDTO>> Register([FromBody] UserRegisterViewmodel model)
        {
            if (model == null || IsInvalidInput(model.Name) || IsInvalidInput(model.Email) || IsInvalidInput(model.Password))
                return BadRequest("Todos os campos são obrigatórios.");

            if (model.Email.Contains(" "))
                return BadRequest("O e-mail não pode conter espaços em branco.");

            if (model.Password.Contains(" "))
                return BadRequest("A senha não pode conter espaços em branco.");

            if (!IsValidPassword(model.Password))
                return BadRequest("A senha contém caracteres inválidos. Use apenas letras, números e os símbolos: ! @ # $ % ^ & * _ - + .");

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
                FullName = model.Name.Trim(),
                Name = model.Email.Split('@')[0],
                Email = model.Email.Trim(),
                //PasswordHash = passwordHash,
                //PasswordSalt = passwordSalt,
                Active = true
            };

            var result = await _userRepository.AddAsync(user);
            if (result == null)
                return StatusCode(500, "Erro interno ao registrar o usuário.");

            var userDto = new UserByIdDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };

            return Ok(userDto);
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
            var token = _authenticateUser.GenerateToken(user.Id, user.Email, user.Name);

            return Ok(new UserToken { Token = token });
        }

        [HttpPost("register-keycloak")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var token = await GetAdminToken();

            var userPayload = new
            {
                username = request.Email,
                email = request.Email,
                enabled = true,
                emailVerified = true,
                credentials = new[]
                {
                new
                {
                    type = "password",
                    value = request.Password,
                    temporary = false
                }
            }
            };

            var client = _httpClientFactory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Post, "http://localhost:9090/admin/realms/oportuniza/users");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent(JsonConvert.SerializeObject(userPayload), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(req);
            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }

            var error = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, error);
        }

        [HttpPost("login-keycloak")]
        public async Task<IActionResult> LoginKeycloak([FromBody] LoginRequestDto login)
        {
            var result = await _authService.LoginWithCredentialsAsync(login.Email, login.Password);

            if (string.IsNullOrEmpty(result))
            {
                return Unauthorized(new { error = "Usuário ou senha inválidos" });
            }

            return Ok(result); 
        }
        private async Task<string> GetAdminToken()
        {
            var client = _httpClientFactory.CreateClient();
            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "admin-cli",
                ["username"] = "admin",
                ["password"] = "admin" 
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync("http://localhost:9090/realms/master/protocol/openid-connect/token", content);

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            dynamic obj = JsonConvert.DeserializeObject(json)!;
            return obj.access_token;
        }
        private static bool IsValidPassword(string password)
        {
            var regex = new Regex(@"^[a-zA-Z0-9!@#$%^&*_\-+.]+$");
            return regex.IsMatch(password);
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