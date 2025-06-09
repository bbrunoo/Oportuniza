using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.AreasOfInterest;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;
using Oportuniza.Infrastructure.Repositories;
using System.Text.Json;

namespace Oportuniza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyAreaOfInterestController : ControllerBase
    {
        private readonly ICompanyAreaOfInterestRepository _companyAreaOfInterest;
        private readonly IMapper _mapper;
        public CompanyAreaOfInterestController(ICompanyAreaOfInterestRepository companyAreaOfInterest, IMapper mapper)
        {
            _companyAreaOfInterest = companyAreaOfInterest;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var areas = await _companyAreaOfInterest.GetAllAsync(
                c => c.Company,
                c => c.AreaOfInterest
            );

            if (areas == null) return NotFound("Currículo não encontrado.");

            var response = _mapper.Map<List<CompanyAreaDto>>(areas);

            return StatusCode(200, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var areas = await _companyAreaOfInterest.GetByIdAsync(id,
                c => c.Company,
                c => c.AreaOfInterest);

            if (areas == null) return NotFound("Currículo não encontrado.");

            var response = _mapper.Map<CompanyAreaDto>(areas);

            return StatusCode(200, response);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CompanyAreaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto == null)
                return BadRequest("Dados inválidos.");

            var area = _mapper.Map<CompanyAreaOfInterest>(dto);
            if (area == null) return BadRequest();
            await _companyAreaOfInterest.AddAsync(area);
            return CreatedAtAction(nameof(GetById), new { id = area.Id }, area);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] CompanyAreaOfInterest area)
        {
            if (area == null || id != area.Id)
                return BadRequest();

            var existingAreas = await _companyAreaOfInterest.GetByIdAsync(id);
            if (existingAreas == null)
                return NotFound();

            await _companyAreaOfInterest.UpdateAsync(area);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingAreas = await _companyAreaOfInterest.GetByIdAsync(id);
            if (existingAreas == null)
                return NotFound();

            await _companyAreaOfInterest.DeleteAsync(id);
            return NoContent();
        }
    }
}
