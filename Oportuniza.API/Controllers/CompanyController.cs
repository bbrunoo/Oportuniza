using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;
using System.Security.Claims;

namespace Oportuniza.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        public CompanyController(ICompanyRepository companyRepository, IMapper mapper, ApplicationDbContext context)
        {
            _companyRepository = companyRepository;
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
        [HttpGet("user-companies")]
        public async Task<IActionResult> GetCompaniesByUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userGuid))
                return Unauthorized("Token inválido.");

            var companies = await _companyRepository.GetByUserIdAsync(userGuid);

            var response = _mapper.Map<List<CompanyListDto>>(companies);

            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] CompanyCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userGuid))
            {
                return Unauthorized("Token inválido ou ID do usuário ausente.");
            }

            var user = await _context.User.FindAsync(userGuid);
            if (user == null)
            {
                return BadRequest($"Usuário com Id {userGuid} não existe no banco de dados.");
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

            await _context.CompanyEmployee.AddAsync(companyEmployeeLink);
            await _context.Company.AddAsync(company);
            await _context.SaveChangesAsync();

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
