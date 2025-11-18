using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Oportuniza.API.Services;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace Oportuniza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationCodeService _codeService;
        private readonly IUserRepository _userRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEmailService _emailService;

        public VerificationController(
            IVerificationCodeService codeService,
            IUserRepository userRepository,
            IHttpClientFactory httpClientFactory,
            IEmailService emailService)
        {
            _codeService = codeService;
            _userRepository = userRepository;
            _httpClientFactory = httpClientFactory;
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendCode([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("E-mail é obrigatório.");

            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            var code = _codeService.GenerateCode(request.Email, "email");
            var message = "Use o código abaixo para verificar sua conta no Oportuniza:";

            var success = await _emailService.SendVerificationEmailAsync(
                request.Email,
                "Verificação de Conta - Oportuniza",
                message,
                code
            );

            if (!success)
                return StatusCode(500, "Falha ao enviar e-mail de verificação.");

            return Ok(new { message = "Código de verificação enviado com sucesso.", expiresInSeconds = 60 });
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] VerificationRequest request)
        {
            if (!_codeService.ValidateCode(request.Email, request.Code, "email"))
                return BadRequest("Código inválido ou expirado.");

            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            user.VerifiedEmail = true;
            await _userRepository.UpdateAsync(user);

            var token = await GetAdminToken();
            var client = _httpClientFactory.CreateClient();

            var searchReq = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://auth.oportuniza.site/admin/realms/oportuniza/users?email={Uri.EscapeDataString(request.Email)}&exact=true"
            );
            searchReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var searchRes = await client.SendAsync(searchReq);
            if (!searchRes.IsSuccessStatusCode)
            {
                var err = await searchRes.Content.ReadAsStringAsync();
                return StatusCode((int)searchRes.StatusCode, $"Erro ao buscar usuário no Keycloak: {err}");
            }

            var json = await searchRes.Content.ReadAsStringAsync();
            dynamic users = JsonConvert.DeserializeObject(json);

            if (users == null || users.Count == 0)
                return NotFound("Usuário não encontrado no Keycloak pelo e-mail.");

            string keycloakId = users[0].id;

            var updateReq = new HttpRequestMessage(
                HttpMethod.Put,
                $"https://auth.oportuniza.site/admin/realms/oportuniza/users/{keycloakId}"
            );
            updateReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            updateReq.Content = new StringContent("{\"emailVerified\": true}", Encoding.UTF8, "application/json");

            var updateRes = await client.SendAsync(updateReq);
            if (!updateRes.IsSuccessStatusCode)
            {
                var err = await updateRes.Content.ReadAsStringAsync();
                return StatusCode((int)updateRes.StatusCode, $"Falha ao atualizar o e-mail no Keycloak: {err}");
            }

            return Ok(new { message = "E-mail verificado com sucesso!" });
        }

        [HttpPost("send-post-code")]
        public async Task<IActionResult> SendPostCode([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("E-mail é obrigatório.");

            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            var code = _codeService.GenerateCode(request.Email, "post");
            var message = "Use o código abaixo para confirmar sua publicação no Oportuniza:";

            var success = await _emailService.SendVerificationEmailAsync(
                request.Email,
                "Verificação de Publicação - Oportuniza",
                message,
                code
            );

            if (!success)
                return StatusCode(500, "Falha ao enviar e-mail de verificação de postagem.");

            return Ok(new { message = "Código de verificação enviado via e-mail.", expiresInSeconds = 60 });
        }

        [HttpPost("validate-post-code")]
        public IActionResult ValidatePostCode([FromBody] VerificationRequest request)
        {
            if (!_codeService.ValidateCode(request.Email, request.Code, "post"))
                return BadRequest("Código de postagem inválido ou expirado.");

            return Ok(new { message = "Postagem verificada com sucesso, pode prosseguir!" });
        }

        [HttpPost("send-company-code")]
        public async Task<IActionResult> SendCompanyCode([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("E-mail é obrigatório.");

            var code = _codeService.GenerateCode(request.Email, "company");
            var message = "Use o código abaixo para confirmar a criação da sua empresa no Oportuniza:";

            var success = await _emailService.SendVerificationEmailAsync(
                request.Email,
                "Verificação de Empresa - Oportuniza",
                message,
                code
            );

            if (!success)
                return StatusCode(500, "Falha ao enviar e-mail de verificação de postagem.");

            return Ok(new { message = "Código de verificação enviado via e-mail.", expiresInSeconds = 60 });
        }

        [HttpPost("validate-company-code")]
        public IActionResult ValidateCompanyCode([FromBody] VerificationRequest request)
        {
            if (!_codeService.ValidateCode(request.Email, request.Code, "company"))
                return BadRequest("Código de empresa inválido ou expirado.");

            return Ok(new { message = "Empresa verificada com sucesso, pode prosseguir!" });
        }
        private async Task<string> GetAdminToken()
        {
            using var client = new HttpClient();

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

        public class VerificationRequest
        {
            public string Email { get; set; }
            public string Code { get; set; }
        }

        public class EmailRequest
        {
            public string Email { get; set; }
        }
    }
}
