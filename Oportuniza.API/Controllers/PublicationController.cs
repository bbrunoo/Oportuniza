using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.API.Services;
using Oportuniza.Domain.DTOs.Publication;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Oportuniza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PublicationController : ControllerBase
    {
        private readonly IPublicationRepository _publicationRepository;
        private readonly AzureBlobService _azureBlobService;
        private readonly IConfiguration _configuration;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;

        public PublicationController(IPublicationRepository publicationRepository, AzureBlobService azureBlobService, IConfiguration configuration, ICompanyRepository companyRepository, IMapper mapper)
        {
            _publicationRepository = publicationRepository;
            _azureBlobService = azureBlobService;
            _configuration = configuration;
            _companyRepository = companyRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var publications = await _publicationRepository.GetAllAsync(
                 orderBy: q => q.OrderByDescending(c => c.CreationDate),
                 includes: new Expression<Func<Publication, object>>[]
                 {
                    p => p.AuthorUser,
                    p => p.AuthorCompany
                 }
            );

            if (publications == null || !publications.Any())
            {
                return NotFound("Nenhuma publicação encontrada.");
            }

            var response = _mapper.Map<List<PublicationDto>>(publications);

            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
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
            if (dto == null) return BadRequest("Dados inválidos.");
            if (image == null || image.Length == 0) return BadRequest("A imagem é obrigatória.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userGuid))
            {
                return Unauthorized("Token inválido.");
            }

            var publication = new Publication
            {
                Title = dto.Title,
                Description = dto.Description,
                Salary = dto.Salary,
                CreatedByUserId = userGuid
            };

            if (dto.PostAsCompanyId.HasValue)
            {
                bool userOwnsCompany = await _companyRepository.UserHasAccessToCompanyAsync(userGuid, dto.PostAsCompanyId.Value);
                if (!userOwnsCompany)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "Você não tem permissão para postar em nome desta empresa.");
                }

                publication.AuthorCompanyId = dto.PostAsCompanyId.Value;
            }
            else
            {
                publication.AuthorUserId = userGuid;
            }

            try
            {
                string containerName = "publications";
                string imageUrl = await _azureBlobService.UploadPostImage(image, containerName, Guid.NewGuid());
                publication.ImageUrl = imageUrl;

                await _publicationRepository.AddAsync(publication);
                return CreatedAtAction(nameof(GetById), new { id = publication.Id }, publication);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro ao criar publicação: {ex.Message}" });
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
