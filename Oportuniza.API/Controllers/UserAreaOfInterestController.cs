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
    public class UserAreaOfInterestController : ControllerBase
    {
        private readonly IUserAreaOfInterestRepository _userAreaOfInterest;
        private readonly IMapper _mapper;
        public UserAreaOfInterestController(IUserAreaOfInterestRepository userAreaOfInterest, IMapper mapper)
        {
            _userAreaOfInterest = userAreaOfInterest;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var areas = await _userAreaOfInterest.GetAllAsync(
                c => c.User,
                c => c.AreaOfInterest);

            if (areas == null) return NotFound("Currículo não encontrado.");

            var response = _mapper.Map<List<UserAreaDto>>(areas);

            return StatusCode(200, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var areas = await _userAreaOfInterest.GetByIdAsync(id,
                c => c.User,
                c => c.AreaOfInterest
            );

            if (areas == null) return NotFound("Currículo não encontrado.");

            var response = _mapper.Map<UserAreaDto>(areas);

            return StatusCode(200, response);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserAreaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto == null)
                return BadRequest("Dados inválidos.");

            var area = _mapper.Map<UserAreaOfInterest>(dto);
            if (area == null) return BadRequest();
            await _userAreaOfInterest.AddAsync(area);
            return CreatedAtAction(nameof(GetById), new { id = area.Id }, area);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] UserAreaOfInterest area)
        {
            if (area == null || id != area.Id)
                return BadRequest();

            var existingAreas = await _userAreaOfInterest.GetByIdAsync(id);
            if (existingAreas == null)
                return NotFound();

            await _userAreaOfInterest.UpdateAsync(area);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingAreas = await _userAreaOfInterest.GetByIdAsync(id);
            if (existingAreas == null)
                return NotFound();

            await _userAreaOfInterest.DeleteAsync(id);
            return NoContent();
        }
    }
}
