using AutoMapper;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oportuniza.API.Services;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;
using System.ComponentModel.Design;
using System.Security.Claims;

namespace Oportuniza.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyEmployeeRepository _companyEmployeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly CNPJService _cnpjService;
        private readonly AzureBlobService _azureBlobService;
        private readonly IVerificationCodeService _verificationCodeService;
        public CompanyController(
            ICompanyRepository companyRepository,
            ICompanyEmployeeRepository companyEmployeeRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ApplicationDbContext context,
            CNPJService cnpjService,
            AzureBlobService azureBlobService,
            IVerificationCodeService verificationCodeService
            )
        {
            _companyRepository = companyRepository;
            _companyEmployeeRepository = companyEmployeeRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _context = context;
            _cnpjService = cnpjService;
            _azureBlobService = azureBlobService;
            _verificationCodeService = verificationCodeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companys = await _companyRepository.GetAllWithEmployeesAndUsersAsync();

            if (companys == null) return NotFound("Empresa não encontrado.");

            var response = _mapper.Map<List<CompanyDTO>>(companys);

            return StatusCode(200, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var company = await _companyRepository.GetByIdWithEmployeesAndUsersAsync(id);

            if (company == null) return NotFound("Empresa não encontrada.");

            var response = _mapper.Map<CompanyDTO>(company);

            return StatusCode(200, response);
        }

        [Authorize]
        [HttpGet("user-companies-paginated")]
        public async Task<IActionResult> GetCompaniesByUser([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(keycloakId))
            {
                return Unauthorized("Token 'sub' claim is missing.");
            }

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);

            if (user == null)
            {
                return NotFound("Usuário não encontrado no banco de dados local.");
            }

            var (companies, totalCompanies) = await _companyRepository.GetUserCompaniesPaginatedAsync(
                user.Id,
                pageNumber,
                pageSize
            );

            var response = _mapper.Map<List<CompanyListDto>>(companies);

            var paginatedResponse = new
            {
                TotalCount = totalCompanies,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = response
            };

            return Ok(paginatedResponse);
        }

        [Authorize]
        [HttpGet("user-companies")]
        public async Task<IActionResult> GetCompaniesByUser()
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(keycloakId))
            {
                return Unauthorized("Token 'sub' claim is missing.");
            }

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);

            if (user == null)
            {
                return NotFound("Usuário não encontrado no banco de dados local.");
            }

            var companies = await _companyRepository.GetAllByUserOrEmployeeAsync(user.Id);

            var response = _mapper.Map<List<CompanyListDto>>(companies);

            return Ok(response);
        }

        [HttpPatch("status/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCompanyStatus(Guid id, [FromBody] CompanyStatusUpdateDto dto)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                return NotFound("Empresa não encontrada.");
            }

            if (!Enum.IsDefined(typeof(CompanyAvailable), dto.NewStatus))
            {
                return BadRequest("Status inválido fornecido.");
            }

            company.IsActive = (CompanyAvailable)dto.NewStatus;

            await _companyRepository.UpdateAsync(company);

            return NoContent();
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] CompanyCreateDto dto, IFormFile image, [FromForm] string verificationCode)
        {
            if (dto == null)
                return BadRequest(new { message = "Dados inválidos." });

            if (image == null || image.Length == 0)
                return BadRequest(new { message = "A imagem é obrigatória." });

            if (!_verificationCodeService.ValidateCode(dto.Email, verificationCode, "company"))
                return BadRequest(new { message = "Código de verificação inválido ou expirado." });


            bool? statusAtivo;
            try
            {
                statusAtivo = await _cnpjService.VerificarAtividadeCnpjAsync(dto.Cnpj);
            }
            catch (Exception)
            {
                statusAtivo = null;
            }

            if (statusAtivo == false)
                return BadRequest(new { message = "O CNPJ informado não está ativo na Receita Federal." });

            if (statusAtivo == null)
                return StatusCode(503, new { message = "Não foi possível verificar o CNPJ no momento. Tente novamente." });

            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized(new { message = "Token inválido." });

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return NotFound(new { message = "Usuário não encontrado no banco local." });

            string imageUrl;
            try
            {
                imageUrl = await _azureBlobService.UploadCompanyImage(image, "company", Guid.NewGuid());
            }
            catch
            {
                return StatusCode(500, new { message = "Falha ao processar imagem da empresa." });
            }

            var company = _mapper.Map<Company>(dto);
            company.UserId = user.Id;
            company.ImageUrl = imageUrl;
            company.IsActive = CompanyAvailable.Active;

            var ownerRole = await _context.CompanyRole.FirstOrDefaultAsync(r => r.Name == "Owner");
            if (ownerRole == null)
            {
                ownerRole = new CompanyRole { Id = Guid.NewGuid(), Name = "Owner" };
                _context.CompanyRole.Add(ownerRole);
                await _context.SaveChangesAsync();
            }

            var companyEmployeeLink = new CompanyEmployee
            {
                UserId = user.Id,
                Company = company,
                CompanyRoleId = ownerRole.Id,
                IsActive = CompanyEmployeeStatus.Active,
                CanPostJobs = true
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Company.Add(company);
                await _context.SaveChangesAsync();

                companyEmployeeLink.CompanyId = company.Id;
                _context.CompanyEmployee.Add(companyEmployeeLink);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Erro ao criar empresa." });
            }

            var companyDtoToReturn = _mapper.Map<CompanyDTO>(company);
            return CreatedAtAction(nameof(GetById), new { id = company.Id }, companyDtoToReturn);
        }

        [HttpPatch("disable/{id}")]
        public async Task<IActionResult> DesactiveCompany(Guid id)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            company.IsActive = CompanyAvailable.Disabled;

            await _companyRepository.UpdateAsync(company);

            return NoContent();
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] CompanyUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCompany = await _companyRepository.GetByIdAsync(id);
            if (existingCompany == null)
                return NotFound("Empresa não encontrada.");

            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(keycloakId))
                return StatusCode(StatusCodes.Status401Unauthorized, "Usuário não autenticado.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return StatusCode(StatusCodes.Status401Unauthorized, "Usuário não encontrado.");

            bool isAuthorized = false;

            if (existingCompany.UserId == user.Id)
            {
                isAuthorized = true;
            }
            else
            {
                var employee = await _context.CompanyEmployee
                    .Include(e => e.CompanyRole)
                    .FirstOrDefaultAsync(e => e.CompanyId == existingCompany.Id && e.UserId == user.Id);

                if (employee != null)
                {
                    var roleName = employee.CompanyRole?.Name?.Trim().ToLower();

                    if (roleName == "owner" || roleName == "administrator")
                        isAuthorized = true;
                }
            }

            if (!isAuthorized)
                return StatusCode(StatusCodes.Status403Forbidden, "Você não tem permissão para editar esta empresa.");

            _mapper.Map(dto, existingCompany);
            await _companyRepository.UpdateAsync(existingCompany);

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingCompany = await _companyRepository.GetByIdAsync(id);
            if (existingCompany == null)
                return NotFound();

            await _companyRepository.DeleteAsync(id);
            return NoContent();
        }

        [Authorize]
        [HttpGet("verify-role/{companyId?}")]
        public async Task<IActionResult> VerifyUserRole(Guid? companyId = null)
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Token inválido: identificador (sub) ausente.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return NotFound("Usuário não encontrado no banco de dados local.");

            var query = _context.CompanyEmployee
                .Include(e => e.CompanyRole)
                .Where(e => e.UserId == user.Id && e.IsActive == CompanyEmployeeStatus.Active);

            if (companyId.HasValue && companyId != Guid.Empty)
                query = query.Where(e => e.CompanyId == companyId.Value);

            var employee = await query.FirstOrDefaultAsync();

            if (employee == null)
                return Ok(new { hasRole = false, message = "Usuário não está vinculado a nenhuma empresa ativa." });

            bool isOwner = string.Equals(employee.CompanyRole.Name, "Owner", StringComparison.OrdinalIgnoreCase);
            bool isWorker = string.Equals(employee.CompanyRole.Name, "Worker", StringComparison.OrdinalIgnoreCase);
            bool isAdmin = string.Equals(employee.CompanyRole.Name, "Administrator", StringComparison.OrdinalIgnoreCase);

            if (isAdmin || isWorker || isOwner)
            {
                return Ok(new
                {
                    hasRole = true,
                    role = employee.CompanyRole.Name,
                    companyId = employee.CompanyId,
                    message = $"Usuário possui papel '{employee.CompanyRole.Name}' na empresa vinculada."
                });
            }

            return Ok(new
            {
                hasRole = false,
                role = employee.CompanyRole?.Name,
                message = "Usuário não possui papel reconhecido (Owner, Worker ou Administrator)."
            });
        }

    }
}
