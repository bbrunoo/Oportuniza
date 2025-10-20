using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Interfaces;
using System.Security.Claims;

namespace Oportuniza.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyEmployeeRepository _companyEmployeeRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyRoleRepository _companyRoleRepository;
        private readonly IAuthenticateUser _authenticateUser;
        public AccountController(
            IUserRepository userRepository,
            ICompanyEmployeeRepository companyEmployeeRepository,
            ICompanyRepository companyRepository,
            ICompanyRoleRepository companyRoleRepository,
            IAuthenticateUser authenticateUser)
        {
            _userRepository = userRepository;
            _companyEmployeeRepository = companyEmployeeRepository;
            _companyRepository = companyRepository;
            _companyRoleRepository = companyRoleRepository;
            _authenticateUser = authenticateUser;
        }
        [HttpGet("contexts")]
        public async Task<IActionResult> GetContexts()
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Token inválido.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            var contexts = new List<object>
            {
                new {
                    Type = "User",
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    ImageUrl = user.ImageUrl,
                }
            };

            var employeeLinks = await _companyEmployeeRepository.GetByUserIdAsync(user.Id);
            foreach (var link in employeeLinks)
            {
                var company = await _companyRepository.GetByIdAsync(link.CompanyId);

                if (company != null && company.IsActive == CompanyAvailable.Active)
                {
                    var role = await _companyRoleRepository.GetByIdAsync(link.CompanyRoleId);

                    contexts.Add(new
                    {
                        Type = "Company",
                        Id = company.Id,
                        Name = company.Name,
                        Role = role.Name,
                        ImageUrl = company.ImageUrl,
                        OwnerId = company.UserId
                    });
                }
            }

            return Ok(contexts);
        }

        [HttpPost("switch-context/{companyId}")]
        public async Task<IActionResult> SwitchCompany(Guid companyId)
        {

            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Token inválido. O identificador do usuário (sub) está ausente.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return NotFound("Usuário Oportuniza não encontrado com o KeycloakId fornecido.");

            var isOwner = await _companyRepository.UserOwnsCompanyAsync(user.Id, companyId);

            var employee = await _companyEmployeeRepository.GetByUserAndCompanyAsync(user.Id, companyId);

            if (employee != null && employee.IsActive != CompanyEmployeeStatus.Active)
            {
                employee = null;
            }

            if (!isOwner && employee == null)
                return Forbid("Acesso negado. Você não tem permissão ativa para esta empresa.");

            string roleName = isOwner
                ? "Owner"
                : employee.CompanyRole.Name;

            var token = _authenticateUser.GenerateToken(
                Guid.Parse(keycloakId),
                user.Email,
                user.Name,
                companyId,
                roleName
            );

            return Ok(new { token });
        }
    }
}