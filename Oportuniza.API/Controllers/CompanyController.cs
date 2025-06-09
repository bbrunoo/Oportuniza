using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.Interfaces;

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
            var company = await _companyRepository.Get();
            var response = _mapper.Map<List<CompanyDTO>>(company);

            return StatusCode(200, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var company = await _companyRepository.GetById(id);

            if (company == null) return NotFound();

            var response = _mapper.Map<List<CompanyByIdDTO>>(company);

            return StatusCode(200, response);
        }

        //[HttpPut("completar-perfil/{id}")]
        //public async Task<IActionResult> CompletePerfil(Guid id, [FromBody] CompleteProfileDTO model)
        //{
        //    var company = await _companyRepository.GetById(id);
        //    if (company == null) return NotFound("Usuario nao encontrado");

        //    company.ImageUrl = model.ImageUrl;

        //    var result = await _companyRepository.Update(company);
        //    if (!result) return StatusCode(500, "Erro ao atualizar perfil");

        //    return NoContent();
        //}
    }
}
