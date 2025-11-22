using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Oportuniza.API.Services;
using Oportuniza.API.Viewmodel;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Net.Http.Headers;
using System.Security.Claims;
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
        private readonly ICompanyEmployeeRepository _companyEmployeeRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly KeycloakAuthService _authService;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IEmailService _emailService;
        private readonly ILoginAttemptRepository _loginAttemptRepository;

        public AuthController(
            IUserRepository userRepository,
            IAuthenticateUser authenticateUser,
            IHttpClientFactory httpClientFactory,
            ICompanyEmployeeRepository companyEmployeeRepository,
            ICompanyRepository companyRepository,
            KeycloakAuthService authService,
            IVerificationCodeService verificationCodeService,
            IEmailService emailService,
            ILoginAttemptRepository loginAttemptRepository)
        {
            _userRepository = userRepository;
            _authenticateUser = authenticateUser;
            _httpClientFactory = httpClientFactory;
            _companyEmployeeRepository = companyEmployeeRepository;
            _companyRepository = companyRepository;
            _authService = authService;
            _verificationCodeService = verificationCodeService;
            _emailService = emailService;
            _loginAttemptRepository = loginAttemptRepository;
        }

        [Authorize]
        [HttpPost("switch-company/{companyId}")]
        public async Task<IActionResult> SwitchCompany(Guid companyId)
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Token inválido.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            var isOwner = await _companyRepository.UserOwnsCompanyAsync(user.Id, companyId);
            var employee = await _companyEmployeeRepository.GetByUserAndCompanyAsync(user.Id, companyId);

            if (!isOwner && employee == null)
                return Forbid("Você não tem acesso a essa empresa.");

            var roleName = isOwner ? "Owner" : employee.CompanyRole.Name;

            var token = _authenticateUser.GenerateToken(
                user.Id,
                user.Email,
                user.Name,
                companyId,
                roleName
            );

            return Ok(new { token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password) ||
        string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Nome, email e senha são obrigatórios.");

            string email = request.Email.Trim().ToLower();
            string password = request.Password.Trim();
            string name = request.Name.Trim();

            var nameRegex = new Regex(@"^[A-Za-zÀ-ÿ\s]+$");
            if (!nameRegex.IsMatch(name))
                return BadRequest("Nome contém caracteres inválidos.");

            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            if (!emailRegex.IsMatch(email))
                return BadRequest("E-mail inválido.");

            var passwordRegex = new Regex(@"^(?=.*[A-Z])(?=.*[!#@$%&.])(?=.*[0-9])(?=.*[a-z])[A-Za-z0-9!#@$%&.]{8,15}$");
            if (!passwordRegex.IsMatch(password))
                return BadRequest("A senha não atende aos critérios de segurança.");

            var token = await GetAdminToken();
            var client = _httpClientFactory.CreateClient();

            var userPayload = new
            {
                username = email,
                email = email,
                firstName = name,
                enabled = true,
                emailVerified = false,
                credentials = new[]
                {
            new { type = "password", value = password, temporary = false }
        }
            };

            var createReq = new HttpRequestMessage(
                HttpMethod.Post,
                "https://auth.oportuniza.site/admin/realms/oportuniza/users"
            );
            createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            createReq.Content = new StringContent(JsonConvert.SerializeObject(userPayload), Encoding.UTF8, "application/json");

            var createRes = await client.SendAsync(createReq);
            if (!createRes.IsSuccessStatusCode)
            {
                var error = await createRes.Content.ReadAsStringAsync();
                return StatusCode((int)createRes.StatusCode, $"Falha ao criar usuário no Keycloak: {error}");
            }

            string keycloakId = null;
            if (createRes.Headers.Location != null)
                keycloakId = createRes.Headers.Location.Segments.Last();

            if (string.IsNullOrEmpty(keycloakId))
            {
                var searchReq = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"https://auth.oportuniza.site/admin/realms/oportuniza/users?email={Uri.EscapeDataString(email)}&exact=true"
                );
                searchReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var searchRes = await client.SendAsync(searchReq);
                if (searchRes.IsSuccessStatusCode)
                {
                    var usersJson = await searchRes.Content.ReadAsStringAsync();
                    dynamic users = JsonConvert.DeserializeObject(usersJson);
                    if (users != null && users.Count > 0)
                        keycloakId = users[0].id;
                }
            }

            if (string.IsNullOrEmpty(keycloakId))
                return StatusCode(500, "Não foi possível obter o ID real do usuário no Keycloak.");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                KeycloakId = keycloakId,
                Email = email,
                Name = name,
                FullName = name,
                VerifiedEmail = false,
                IsProfileCompleted = false,
                Active = true
            };

            await _userRepository.AddAsync(newUser);

            var code = _verificationCodeService.GenerateCode(email, "email");
            var emailSent = await _emailService.SendVerificationEmailAsync(
                email,
                "Verificação de Conta - Oportuniza",
                "Use o código abaixo para verificar sua conta no Oportuniza:",
                code
            );

            if (!emailSent)
                return StatusCode(500, "Usuário criado, mas falha ao enviar o e-mail de verificação.");

            return Ok(new
            {
                message = "Usuário registrado com sucesso. Verifique seu e-mail para confirmar a conta.",
                requiresVerification = true,
                keycloakId
            });
        }

        [HttpPost("login-keycloak")]
        public async Task<IActionResult> LoginKeycloak([FromBody] LoginRequestDto login)
        {
            var ip = GetClientIp();

            var attempt = await _loginAttemptRepository.GetByIpAsync(ip);

            if (attempt != null && attempt.LockoutEnd.HasValue && attempt.LockoutEnd > DateTime.UtcNow)
            {
                var minutes = (attempt.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
                return Error($"Muitas tentativas. IP bloqueado por {minutes:F0} minutos.", 429);
            }

            var result = await _authService.LoginWithCredentialsAsync(login.Email, login.Password);

            if (string.IsNullOrEmpty(result) || result == "NOT_VERIFIED")
            {
                if (attempt == null)
                {
                    attempt = new LoginAttempt
                    {
                        IPAddress = ip,
                        FailedAttempts = 1,
                        LockoutEnd = null
                    };

                    await _loginAttemptRepository.AddAsync(attempt);
                }
                else
                {
                    attempt.FailedAttempts++;

                    if (attempt.FailedAttempts >= 5)
                        attempt.LockoutEnd = DateTime.UtcNow.AddHours(1);

                    await _loginAttemptRepository.UpdateAsync(attempt);
                }

                if (result == "NOT_VERIFIED")
                    return Error("E-mail não verificado.", 401);

                return Error("Usuário ou senha inválidos.", 401);
            }

            if (attempt != null)
            {
                attempt.FailedAttempts = 0;
                attempt.LockoutEnd = null;
                await _loginAttemptRepository.UpdateAsync(attempt);
            }

            return Ok(result);
        }

        private async Task<string> GetAdminToken()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            using var client = new HttpClient(handler);

            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "admin-cli",
                ["username"] = "admin",
                ["password"] = "admin"
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync("https://auth.oportuniza.site/realms/master/protocol/openid-connect/token", content);

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            dynamic obj = JsonConvert.DeserializeObject(json)!;
            return obj.access_token;
        }

        private string GetClientIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
        private IActionResult Error(string message, int status)
        {
            return StatusCode(status, new { error = message });
        }
    }
}