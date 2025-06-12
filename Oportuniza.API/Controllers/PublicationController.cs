using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.API.Services;
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
        private readonly AzureBlobService _azureBlobService;
        private readonly IMapper _mapper;
        public PublicationController(IPublicationRepository publicationRepository, IMapper mapper, AzureBlobService azureBlobService)
        {
            _publicationRepository = publicationRepository;
            _mapper = mapper;
            _azureBlobService = azureBlobService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var publications = await _publicationRepository.GetAllAsync(
                include: c=>c.Author,
                orderBy: q => q.OrderByDescending(c=>c.CreationDate));

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
        public async Task<IActionResult> Post([FromForm] PublicationCreateDto dto, IFormFile image)
        {
            if (dto == null)
                return BadRequest("Dados inválidos.");

            if (image == null || image.Length == 0)
                return BadRequest("A imagem é obrigatória.");

            try
            {
                string containerName = "publications";
                string imageUrl = await _azureBlobService.UploadPostImage(image, containerName, dto.AuthorId);

                dto.ImageUrl = imageUrl;

                var publication = _mapper.Map<Publication>(dto);
                await _publicationRepository.AddAsync(publication);

                return CreatedAtAction(nameof(GetById), new {id = publication.Id}, publication);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao criar publicação: {ex.Message}");
            }
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
