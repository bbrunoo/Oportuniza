using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Oportuniza.API.Viewmodel;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email e senha são obrigatórios.");
            }

            var token = await GetAdminToken();
            var userPayload = new
            {
                    username = request.Email,
                    email = request.Email,
                    enabled = true,
                    emailVerified = false,
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

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }


            var location = response.Headers.Location;
            if (location == null)
            {
                return StatusCode(500, "Erro ao obter o ID do usuário do Keycloak.");
            }

            var keycloakId = location.Segments.Last();

            var existingUser = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (existingUser != null)
            {
                return Ok(new { message = "Usuário já registrado localmente." });
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                KeycloakId = keycloakId,
                Email = request.Email,
                Name = GenerateNameFromEmail(request.Email),
                FullName = request.Email ?? GenerateNameFromEmail(request.Email),
                IsProfileCompleted = false,
                //VerifiedEmail=false,
                Active = true
            };

            try
            {
                await _userRepository.AddAsync(newUser);
            }
            catch (Exception ex)
            {

                return StatusCode(500, "Erro ao registrar o usuário localmente. Por favor, tente novamente.");
            }

            return Ok(new { message = "Usuário registrado com sucesso." });
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
        private string GenerateNameFromEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            {
                return "Usuário";
            }

            string name = email.Split('@')[0];
            if (name.Length > 0)
            {
                name = char.ToUpper(name[0]) + name.Substring(1);
            }
            return name;
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