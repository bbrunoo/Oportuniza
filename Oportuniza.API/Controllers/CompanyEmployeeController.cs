using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.DTOs.Employee;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Security.Claims;

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
        private readonly ICompanyRoleRepository _companyRoleRepository;

        public CompanyEmployeeController(
            ICompanyEmployeeRepository companyEmployeeRepository,
            ICompanyRepository companyRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IHttpClientFactory httpClientFactory,
            ICompanyRoleRepository companyRoleRepository)
        {
            _companyEmployeeRepository = companyEmployeeRepository;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
            _companyRoleRepository = companyRoleRepository;
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
            var keycloakIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(keycloakIdClaim))
                return Error("Token inválido.", 401);

            var loggedUser = await _userRepository.GetUserByKeycloakIdAsync(keycloakIdClaim);
            if (loggedUser == null)
                return Error("Usuário logado não encontrado.", 404);

            var employee = await _companyEmployeeRepository.GetByIdAsync(id);
            if (employee == null)
                return Error("Funcionário não encontrado.", 404);

            if (employee.UserId == loggedUser.Id)
                return Error("Você não pode alterar o seu próprio status.", 400);

            var company = await _companyRepository.GetByIdAsync(employee.CompanyId);
            if (company == null)
                return Error("Empresa vinculada não encontrada.", 404);

            if (employee.UserId == company.UserId)
                return Error("Não é permitido alterar o status do dono da empresa.", 400);

            if (!Enum.IsDefined(typeof(CompanyEmployeeStatus), dto.NewStatus))
                return Error("Status inválido fornecido.", 400);

            employee.IsActive = dto.NewStatus;
            await _companyEmployeeRepository.UpdateAsync(employee);

            return NoContent();
        }

        [Authorize]
        [HttpPost("register-employee")]
        public async Task<IActionResult> RegisterEmployee([FromBody] EmployeeRegisterRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email é obrigatório.");

            var keycloakIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(keycloakIdClaim))
                return Unauthorized("Token inválido.");

            var loggedUser = await _userRepository.GetUserByKeycloakIdAsync(keycloakIdClaim);
            if (loggedUser == null)
                return NotFound("Usuário logado não encontrado.");

            var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            if (company == null || company.UserId != loggedUser.Id)
                return Forbid("Você não tem permissão para adicionar funcionários a esta empresa.");

            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser == null)
                return NotFound("Usuário com este e-mail não foi encontrado. Apenas usuários existentes podem ser vinculados.");

            var existingEmployee = await _companyEmployeeRepository.GetEmployeeByUserIdAndCompanyIdAsync(existingUser.Id, request.CompanyId);
            if (existingEmployee != null)
                return BadRequest("Este usuário já está vinculado a esta empresa.");

            var defaultRole = await _companyRoleRepository.GetRoleByNameAsync("Worker");
            if (defaultRole == null)
                return StatusCode(500, "Erro de configuração: O papel padrão 'Worker' não foi encontrado.");

            var companyEmployeeLink = new CompanyEmployee
            {
                UserId = existingUser.Id,
                CompanyId = request.CompanyId,
                CompanyRoleId = defaultRole.Id,
                CanPostJobs = true
            };

            await _companyEmployeeRepository.AddAsync(companyEmployeeLink);

            return Ok(new { message = $"Usuário {existingUser.Email} vinculado à empresa com sucesso." });
        }

        [HttpGet("search-user")]
        public async Task<IActionResult> SearchUserByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email é obrigatório.");
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null) return NotFound();

            var result = new UserSearchResultDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                ImageUrl = user.ImageUrl
            };
            return Ok(result);
        }

        [HttpPatch("roles/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEmployeeRoles(Guid id, [FromBody] EmployeeRoleUpdateDto dto)
        {
            var keycloakIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(keycloakIdClaim))
                return Error("Token inválido.", 401);

            var loggedUser = await _userRepository.GetUserByKeycloakIdAsync(keycloakIdClaim);
            if (loggedUser == null)
                return Error("Usuário logado não encontrado.", 404);

            var employee = await _companyEmployeeRepository.GetByIdAsync(id);
            if (employee == null)
                return Error("Funcionário não encontrado.", 404);

            if (employee.UserId == loggedUser.Id)
                return Error("Você não pode alterar o próprio cargo.", 400);

            var company = await _companyRepository.GetByIdAsync(employee.CompanyId);
            if (company == null)
                return Error("Empresa vinculada não encontrada.", 404);

            if (employee.UserId == company.UserId)
                return Error("Não é permitido alterar o cargo do dono da empresa.", 400);

            var newRoleName = dto.IsAdmin ? "Administrator" : "Worker";
            var role = await _companyRoleRepository.GetRoleByNameAsync(newRoleName);
            if (role == null)
                return Error($"Cargo inválido: {newRoleName}", 400);

            employee.CompanyRoleId = role.Id;
            employee.CanPostJobs = dto.CanPost;

            await _companyEmployeeRepository.UpdateAsync(employee);

            return NoContent();
        }

        [Authorize]
        [HttpDelete("unlink/{companyId}")]
        public async Task<IActionResult> UnlinkCurrentUserFromCompany(Guid companyId)
        {
            var keycloakIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(keycloakIdClaim))
                return Unauthorized(new { message = "Token inválido." });

            var loggedUser = await _userRepository.GetUserByKeycloakIdAsync(keycloakIdClaim);
            if (loggedUser == null)
                return NotFound(new { message = "Usuário logado não encontrado." });

            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
                return NotFound(new { message = "Empresa não encontrada." });

            var employee = await _companyEmployeeRepository.GetEmployeeByUserIdAndCompanyIdAsync(loggedUser.Id, companyId);
            if (employee == null)
                return NotFound(new { message = "O usuário não está vinculado a esta empresa." });

            if (company.UserId == loggedUser.Id)
                return BadRequest(new { message = "O dono da empresa não pode se desvincular." });

            await _companyEmployeeRepository.DeleteAsync(employee.Id);

            return Ok(new
            {
                message = "Usuário desvinculado da empresa com sucesso."
            });
        }

        private IActionResult Error(string message, int status)
        {
            return StatusCode(status, new { error = message });
        }
    }
}