using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Text.Json;

namespace Oportuniza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly ICityRepository _cityRepository;
        public CityController(ICityRepository cityRepository)
        {
            _cityRepository = cityRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var cities = await _cityRepository.GetAllAsync();
            var totalItems = cities.Count();

            var ordered = cities.OrderBy(c => c.Name);

            var pagedCities = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Data = pagedCities
            };

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var city = await _cityRepository.GetByIdAsync(id);
            if (city == null)
                return NotFound();

            return Ok(city);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetCities([FromQuery] string? uf, [FromQuery] string? name)
        {
            var city = await _cityRepository.GetCitiesAsync(uf, name);
            if (city == null)
                return NotFound();

            return Ok(city);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var city = await _cityRepository.GetByNameAsync(name);

            if (city == null)
                return NotFound();

            return Ok(city);
        }

        [HttpGet("uf/{uf}")]
        public async Task<IActionResult> GetByUf(string uf)
        {
            var cities = await _cityRepository.GetByUfAsync(uf);
            var ordered = cities.OrderBy(c => c.Name).ToList();
            return Ok(ordered);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _cityRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpDelete("deleteAll")]
        public async Task<IActionResult> DeleteAll()
        {
            await _cityRepository.DeleteAllAsync();
            return Ok("Todas as cidades foram deletadas.");
        }

        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            var existingCities = await _cityRepository.GetAllAsync();
            if (existingCities.Any())
                return Ok("As cidades já foram inseridas anteriormente.");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Helper", "municipios.json");

            if (!System.IO.File.Exists(filePath))
                return NotFound("Arquivo municipios.json não encontrado.");

            var json = await System.IO.File.ReadAllTextAsync(filePath);

            var municipios = JsonSerializer.Deserialize<List<MunicipioDTO>>(json);

            if (municipios == null || !municipios.Any())
                return BadRequest("Nenhum município encontrado no arquivo.");

            var cities = municipios
               .Where(m => m.microrregiao != null && m.microrregiao.mesorregiao != null && m.microrregiao.mesorregiao.UF != null)
               .Select(m => new City
               {
                   Id = Guid.NewGuid(),
                   Name = m.nome,
                   Uf = m.microrregiao.mesorregiao.UF.sigla
               })
               .ToList();

            foreach (var city in cities)
            {
                await _cityRepository.AddAsync(city);
            }

            return Ok($"{cities.Count} cidades inseridas com sucesso.");
        }
    }

    public class MunicipioDTO
    {
        public string nome { get; set; }
        public Microrregiao microrregiao { get; set; }
    }

    public class Microrregiao
    {
        public Mesorregiao mesorregiao { get; set; }
    }

    public class Mesorregiao
    {
        public UF UF { get; set; }
    }

    public class UF
    {
        public string sigla { get; set; }
    }
}

