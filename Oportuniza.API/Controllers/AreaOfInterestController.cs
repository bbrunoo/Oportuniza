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
    public class AreaOfInterestController : ControllerBase
    {
        private readonly IAreaOfInterest _areaOfInterest;
        private readonly IMapper _mapper;
        public AreaOfInterestController(IAreaOfInterest areaOfInterest, IMapper mapper)
        {
            _areaOfInterest = areaOfInterest;
            _mapper = mapper;
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetAreas([FromQuery] string? areaName)
        {
            var areas = await _areaOfInterest.GetAreasAsync(areaName);

            if (areas == null || !areas.Any())
                return NotFound();

            return Ok(areas);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var areas = await _areaOfInterest.GetAllAsync();

            if (areas == null) return NotFound("Currículo não encontrado.");

            var response = _mapper.Map<List<AreasDto>>(areas);

            return StatusCode(200, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var areas = await _areaOfInterest.GetByIdAsync(id);

            if (areas == null) return NotFound("Currículo não encontrado.");

            var response = _mapper.Map<AreasDto>(areas);

            return StatusCode(200, response);
        }

        [HttpGet("area/{area}")]
        public async Task<IActionResult> GetByName(string area)
        {
            var city = await _areaOfInterest.GetByNameAsync(area);

            if (city == null)
                return NotFound();

            return Ok(city);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AreasCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto == null)
                return BadRequest("Dados inválidos.");

            var area = _mapper.Map<AreaOfInterest>(dto);
            if (area == null) return BadRequest();
            await _areaOfInterest.AddAsync(area);
            return CreatedAtAction(nameof(GetById), new { id = area.Id }, area);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] AreaOfInterest area)
        {
            if (area == null || id != area.Id)
                return BadRequest();

            var existingAreas = await _areaOfInterest.GetByIdAsync(id);
            if (existingAreas == null)
                return NotFound();

            await _areaOfInterest.UpdateAsync(area);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingAreas = await _areaOfInterest.GetByIdAsync(id);
            if (existingAreas == null)
                return NotFound();

            await _areaOfInterest.DeleteAsync(id);
            return NoContent();
        }
        
        [HttpDelete("deleteAll")]
        public async Task<IActionResult> DeleteAll()
        {
            await _areaOfInterest.DeleteAllAsync();
            return Ok("Todas as areas foram deletadas.");
        }

        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            var existingCities = await _areaOfInterest.GetAllAsync();
            if (existingCities.Any())
                return Ok("As cidades já foram inseridas anteriormente.");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Helper", "areas.json");

            if (!System.IO.File.Exists(filePath))
                return NotFound("Arquivo municipios.json não encontrado.");

            var json = await System.IO.File.ReadAllTextAsync(filePath);

            var areas = JsonSerializer.Deserialize<List<AreasDto>>(json);

            if (areas == null || !areas.Any())
                return BadRequest("Nenhuma area encontrada no arquivo.");

            var areasDefinidas = areas
               .Where(m => m.InterestArea != null)
               .Select(m => new AreaOfInterest
               {
                   InterestArea = m.InterestArea,
               })
               .ToList();

            foreach (var area in areasDefinidas)
            {
                await _areaOfInterest.AddAsync(area);
            }

            return Ok($"{areasDefinidas.Count} cidades inseridas com sucesso.");
        }

    }
}
