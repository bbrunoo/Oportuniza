using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Repositories;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Oportuniza.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CompanyEmployeeController : ControllerBase
    {
        private readonly ICompanyEmployeeRepository _companyEmployeeRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;

        public CompanyEmployeeController(ICompanyEmployeeRepository companyEmployeeRepository, ICompanyRepository companyRepository, IUserRepository userRepository, IMapper mapper, IHttpClientFactory httpClientFactory)
        {
            _companyEmployeeRepository = companyEmployeeRepository;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{companyId}/employees-ordered")]
        public async Task<IActionResult> GetOrderedEmployees(Guid companyId)
        {
            var employees = await _companyEmployeeRepository
                                    .GetEmployeesOrderedByRoleAndCreationAsync(companyId);

            if (employees == null || !employees.Any())
            {
                return NotFound("Nenhum funcionário ativo encontrado para esta empresa.");
            }

            var response = _mapper.Map<List<CompanyEmployeeDto>>(employees);

            return Ok(response);
        }

        [HttpPatch("status/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEmployeeStatus(Guid id, [FromBody] EmployeeStatusUpdateDto dto)
        {
            var employee = await _companyEmployeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound("Funcionário não encontrado.");
            }

            if (!Enum.IsDefined(typeof(CompanyEmployeeStatus), dto.NewStatus))
            {
                return BadRequest("Status inválido fornecido.");
            }

            employee.IsActive = dto.NewStatus;

            await _companyEmployeeRepository.UpdateAsync(employee);

            return NoContent();
        }

        [Authorize]
        [HttpPost("register-employee")]
        public async Task<IActionResult> RegisterEmployee([FromBody] EmployeeRegisterRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email e senha são obrigatórios.");
            }

            var keycloakIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(keycloakIdClaim)) return Unauthorized("Token inválido.");

            var loggedUser = await _userRepository.GetUserByKeycloakIdAsync(keycloakIdClaim);
            if (loggedUser == null) return NotFound("Usuário logado não encontrado.");

            var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            if (company == null || company.UserId != loggedUser.Id)
            {
                return Forbid("Você não tem permissão para adicionar funcionários a esta empresa.");
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

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(userPayload);
            req.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(req);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Erro no Keycloak ao criar usuário: {error}");
            }

            var location = response.Headers.Location;
            if (location == null) return StatusCode(500, "Erro ao obter o ID do usuário do Keycloak.");

            var keycloakId = location.Segments.Last();

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                KeycloakId = keycloakId,
                Email = request.Email,
                Name = request.EmployeeName ?? GenerateNameFromEmail(request.Email),
                FullName = request.EmployeeName ?? GenerateNameFromEmail(request.Email),
                IsProfileCompleted = true,
                Active = true,
                ImageUrl = company.ImageUrl
            };

            await _userRepository.AddAsync(newUser);

            var companyEmployeeLink = new CompanyEmployee
            {
                UserId = newUser.Id,
                CompanyId = request.CompanyId,
                Roles = "Employee",
                CanPostJobs = true
            };

            await _companyEmployeeRepository.AddAsync(companyEmployeeLink);

            return Ok(new { message = "Funcionário registrado e vinculado à empresa com sucesso." });
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

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                if (doc.RootElement.TryGetProperty("access_token", out JsonElement tokenElement))
                {
                    return tokenElement.GetString()!;
                }
            }
            throw new Exception("Falha ao obter access_token do Keycloak.");
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
    }
}