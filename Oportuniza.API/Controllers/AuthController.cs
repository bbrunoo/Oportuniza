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

        public AuthController(IUserRepository userRepository, IAuthenticateUser authenticateUser, IHttpClientFactory httpClientFactory, ICompanyEmployeeRepository companyEmployeeRepository, ICompanyRepository companyRepository, KeycloakAuthService authService, IVerificationCodeService verificationCodeService, IEmailService emailService)
        {
            _userRepository = userRepository;
            _authenticateUser = authenticateUser;
            _httpClientFactory = httpClientFactory;
            _companyEmployeeRepository = companyEmployeeRepository;
            _companyRepository = companyRepository;
            _authService = authService;
            _verificationCodeService = verificationCodeService;
            _emailService = emailService;
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

        //[HttpPost("login")]
        //public async Task<ActionResult<UserToken>> Login([FromBody] LoginRequestDto loginRequestDTO)
        //{
        //    if (loginRequestDTO == null || IsInvalidInput(loginRequestDTO.Email) || IsInvalidInput(loginRequestDTO.Password))
        //        return BadRequest("Todos os campos são obrigatórios.");

        //    if (loginRequestDTO.Password.Contains(" "))
        //        return BadRequest("A senha não pode conter espaços.");

        //    if (!IsValidEmail(loginRequestDTO.Email))
        //        return BadRequest("Formato de e-mail inválido.");

        //    var ip = GetClientIp();
        //    var (isAuthenticated, errorMessage, statusCode) = await _authenticateUser.AuthenticateAsync(
        //        loginRequestDTO.Email, loginRequestDTO.Password, ip
        //    );

        //    if (!isAuthenticated)
        //    {
        //        if (statusCode.HasValue)
        //            return StatusCode(statusCode.Value, errorMessage ?? "Erro de autenticação.");

        //        return Unauthorized(errorMessage);
        //    }

        //    var user = await _authenticateUser.GetUserByEmail(loginRequestDTO.Email);
        //    var token = _authenticateUser.GenerateToken(user.Id, user.Email, user.Name);

        //    return Ok(new UserToken { Token = token });
        //}

        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        //{
        //    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        //        return BadRequest("Email e senha são obrigatórios.");

        //    var token = await GetAdminToken();

        //    var userPayload = new
        //    {
        //        username = request.Email,
        //        email = request.Email,
        //        enabled = true,
        //        emailVerified = false,
        //        credentials = new[]
        //        {
        //    new
        //    {
        //        type = "password",
        //        value = request.Password,
        //        temporary = false
        //    }
        //}
        //    };

        //    var client = _httpClientFactory.CreateClient();
        //    var req = new HttpRequestMessage(HttpMethod.Post, "https://auth.oportuniza.site/admin/realms/oportuniza/users");
        //    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //    req.Content = new StringContent(JsonConvert.SerializeObject(userPayload), Encoding.UTF8, "application/json");

        //    var response = await client.SendAsync(req);
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var error = await response.Content.ReadAsStringAsync();
        //        return StatusCode((int)response.StatusCode, error);
        //    }

        //    var location = response.Headers.Location;
        //    if (location == null)
        //        return StatusCode(500, "Erro ao obter ID do usuário do Keycloak.");

        //    var keycloakId = location.Segments.Last();

        //    var newUser = new User
        //    {
        //        Id = Guid.NewGuid(),
        //        KeycloakId = keycloakId,
        //        Email = request.Email,
        //        Name = GenerateNameFromEmail(request.Email),
        //        FullName = request.Email,
        //        VerifiedEmail = false,
        //        IsProfileCompleted = false,
        //        Active = true
        //    };

        //    await _userRepository.AddAsync(newUser);

        //    var code = _verificationCodeService.GenerateCode(request.Email);
        //    await _emailService.SendVerificationEmailAsync(
        //        request.Email,
        //        "Verificação de Conta - Oportuniza",
        //        "Use o código abaixo para verificar sua conta.",
        //        code
        //    );

        //    return Ok(new
        //    {
        //        message = "Usuário registrado. Verifique seu e-mail.",
        //        requiresVerification = true
        //    });
        //}

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email e senha são obrigatórios.");

            string email = request.Email.Trim().ToLower();
            string password = request.Password.Trim();

            // ✅ Token do próprio realm 'oportuniza'
            var token = await GetAdminToken();
            var client = _httpClientFactory.CreateClient();

            // 🔹 Corpo da requisição para criar o usuário no Keycloak
            var userPayload = new
            {
                username = email,
                email = email,
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

            // ✅ Buscar o ID real do usuário recém-criado
            string keycloakId = null;

            // 1️⃣ Extrai do header Location
            if (createRes.Headers.Location != null)
            {
                keycloakId = createRes.Headers.Location.Segments.Last();
                Console.WriteLine($"[Keycloak] ID obtido via Location: {keycloakId}");
            }

            // 2️⃣ Fallback — busca o usuário pelo e-mail
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
                    {
                        keycloakId = users[0].id;
                        Console.WriteLine($"[Keycloak] ID obtido via busca por e-mail: {keycloakId}");
                    }
                }
            }

            if (string.IsNullOrEmpty(keycloakId))
                return StatusCode(500, "Não foi possível obter o ID real do usuário no Keycloak.");

            // ✅ Cria o usuário local com o ID correto
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                KeycloakId = keycloakId,
                Email = email,
                Name = GenerateNameFromEmail(email),
                FullName = email,
                VerifiedEmail = false,
                IsProfileCompleted = false,
                Active = true
            };

            await _userRepository.AddAsync(newUser);

            // ✅ Envia o e-mail de verificação
            var code = _verificationCodeService.GenerateCode(email);
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
            var result = await _authService.LoginWithCredentialsAsync(login.Email, login.Password);

            if (result == "NOT_VERIFIED")
                return Unauthorized(new { error = "E-mail não verificado. Verifique sua caixa de entrada antes de fazer login." });

            if (string.IsNullOrEmpty(result))
                return Unauthorized(new { error = "Usuário ou senha inválidos" });

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