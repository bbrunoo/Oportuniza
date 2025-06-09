using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;

namespace Oportuniza.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        public CompanyController(ICompanyRepository companyRepository, IMapper mapper)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companys = await _companyRepository.GetAllAsync();

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

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CompanyCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto == null)
                return BadRequest("Dados inválidos.");

            var company = _mapper.Map<Company>(dto);
            if (company == null) return BadRequest();
            await _companyRepository.AddAsync(company);
            return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
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
