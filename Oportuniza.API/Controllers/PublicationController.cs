using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.Publication;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;

namespace Oportuniza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        private readonly IPublicationRepository _publicationRepository;
        private readonly IMapper _mapper;
        public PublicationController(IPublicationRepository publicationRepository, IMapper mapper)
        {
            _publicationRepository = publicationRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var publications = await _publicationRepository.GetAllAsync(c=>c.Author);

            if (publications == null) return NotFound("Currículo não encontrado.");

            var response = _mapper.Map<List<PublicationDto>>(publications);

            return StatusCode(200, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var publications = await _publicationRepository.GetByIdAsync(id);

            if (publications == null) return NotFound("Currículo não encontrado.");

            var response = _mapper.Map<PublicationDto>(publications);

            return StatusCode(200, response);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PublicationCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto == null)
                return BadRequest("Dados inválidos.");

            var publication = _mapper.Map<Publication>(dto);
            if (publication == null) return BadRequest();
            await _publicationRepository.AddAsync(publication);
            return CreatedAtAction(nameof(GetById), new { id = publication.Id }, publication);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] Publication publication)
        {
            if (publication == null || id != publication.Id)
                return BadRequest();

            var existingPublication = await _publicationRepository.GetByIdAsync(id);
            if (existingPublication == null)
                return NotFound();

            await _publicationRepository.UpdateAsync(publication);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingPublication = await _publicationRepository.GetByIdAsync(id);
            if (existingPublication == null)
                return NotFound();

            await _publicationRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
