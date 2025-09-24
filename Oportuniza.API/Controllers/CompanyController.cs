using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;
using Oportuniza.Infrastructure.Repositories;
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

        public CompanyController(ICompanyRepository companyRepository, ICompanyEmployeeRepository companyEmployeeRepository, IUserRepository userRepository, IMapper mapper, ApplicationDbContext context)
        {
            _companyRepository = companyRepository;
            _companyEmployeeRepository = companyEmployeeRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _context = context;
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
            var companys = await _companyRepository.GetByIdAsync(id);

            if (companys == null) return NotFound("Currículo não encontrado.");

            var response = _mapper.Map<CompanyDTO>(companys);

            return StatusCode(200, response);
        }

        [Authorize]
        [HttpGet("user-companies-paginated")]
        public async Task<IActionResult> GetCompaniesByUser([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(keycloakId))
            {
                return Unauthorized("Token 'sub' claim is missing.");
            }

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);

            if (user == null)
            {
                return NotFound("Usuário não encontrado no banco de dados local.");
            }

            var totalCompanies = await _context.Company
                .CountAsync(c => c.UserId == user.Id);

            var companies = await _companyRepository.GetByUserIdAsyncPaginated(user.Id, pageNumber, pageSize);

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
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(keycloakId))
            {
                return Unauthorized("Token 'sub' claim is missing.");
            }

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);

            if (user == null)
            {
                return NotFound("Usuário não encontrado no banco de dados local.");
            }

            var companies = await _companyRepository.GetByUserIdAsync(user.Id);
            var response = _mapper.Map<List<CompanyListDto>>(companies);

            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] CompanyCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(keycloakId))
            {
                return Unauthorized("Token 'sub' claim is missing.");
            }

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);

            if (user == null)
            {
                return NotFound("Usuário não encontrado no banco de dados local.");
            }

            var company = _mapper.Map<Company>(dto);

            company.UserId = user.Id;

            var companyEmployeeLink = new CompanyEmployee
            {
                User = user,
                Company = company,
                Roles = "Owner",
                CanPostJobs = true
            };

            _companyRepository.AddAsync(company);
            _companyEmployeeRepository.AddAsync(companyEmployeeLink);

            var companyDtoToReturn = _mapper.Map<CompanyDTO>(company);
            return CreatedAtAction(nameof(GetById), new { id = company.Id }, companyDtoToReturn);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] Company company)
        {
            if (company == null || id != company.Id)
                return BadRequest();

            var existingCompany = await _companyRepository.GetByIdAsync(id);
            if (existingCompany == null)
                return NotFound();

            await _companyRepository.UpdateAsync(company);

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
    }
}
