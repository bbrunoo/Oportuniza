using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.API.Services;
using Oportuniza.API.Services.Oportuniza.API.Services;
using Oportuniza.Domain.DTOs.Publication;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Repositories;
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
        private readonly IUserRepository _userRepository;
        private readonly IActiveContextService _activeContextService;
        private readonly IMapper _mapper;
        private readonly GeminiClientService _geminiService;

        public PublicationController(IPublicationRepository publicationRepository, AzureBlobService azureBlobService, IConfiguration configuration, ICompanyRepository companyRepository, IUserRepository userRepository, IMapper mapper, GeminiClientService geminiService)
        {
            _publicationRepository = publicationRepository;
            _azureBlobService = azureBlobService;
            _configuration = configuration;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _geminiService = geminiService;
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
                 },
                 filter: p => p.IsActive == PublicationAvailable.Enabled
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
            var publication = await _publicationRepository.GetByIdAsync(
                id,
                p => p.AuthorUser,
                p => p.AuthorCompany
            );

            if (publication == null)
                return NotFound("Publicação não encontrada.");

            var response = _mapper.Map<PublicationDto>(publication);

            return Ok(response);
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetByLoggedId([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Token inválido. O identificador Keycloak ('sub') está ausente.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return NotFound("Usuário não encontrado no banco de dados local.");

            var userLocalId = user.Id;

            var companyIdClaim = User.FindFirst("company_id")?.Value;

            Guid contextId;
            bool isCompanyContext;

            if (Guid.TryParse(companyIdClaim, out Guid activeCompanyId))
            {
                contextId = activeCompanyId;
                isCompanyContext = true;
            }
            else
            {
                contextId = userLocalId;
                isCompanyContext = false;
            }

            (IEnumerable<Publication> publications, int totalCount) result;

            if (isCompanyContext)
            {
                result = await _publicationRepository.GetCompanyPublicationsPaged(contextId, pageNumber, pageSize);
            }
            else
            {
                result = await _publicationRepository.GetMyPublicationsPaged(userLocalId, pageNumber, pageSize, onlyPersonal: true);
            }

            var publications = result.publications ?? Enumerable.Empty<Publication>();

            if (!publications.Any())
                return NotFound("Nenhuma publicação encontrada para o contexto ativo.");

            var response = _mapper.Map<IEnumerable<PublicationDto>>(publications);

            var finalResult = new
            {
                result.totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling(result.totalCount / (double)pageSize),
                items = response
            };

            return Ok(finalResult);
        }

        //[HttpGet("my")]
        //[Authorize]
        //public async Task<IActionResult> GetByActiveContext([FromServices] IActiveContextService contextService, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        //{
        //    var (ctxId, ctxType, _) = await contextService.GetActiveContextAsync(HttpContext);

        //    (IEnumerable<Publication> publications, int totalCount) result;

        //    if (ctxType == "User")
        //    {
        //        result = await _publicationRepository.GetMyPublicationsPagedByUser(ctxId, pageNumber, pageSize);
        //    }
        //    else if (ctxType == "Company")
        //    {
        //        result = await _publicationRepository.GetMyPublicationsPagedByCompany(ctxId, pageNumber, pageSize);
        //    }
        //    else
        //    {
        //        return BadRequest("Contexto inválido.");
        //    }

        //    if (result.publications == null || !result.publications.Any())
        //        return NotFound("Publicações não encontradas.");

        //    var response = _mapper.Map<IEnumerable<PublicationDto>>(result.publications);

        //    var final = new
        //    {
        //        result.totalCount,
        //        pageNumber,
        //        pageSize,
        //        totalPages = (int)Math.Ceiling(result.totalCount / (double)pageSize),
        //        items = response
        //    };

        //    return Ok(final);
        //}

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromForm] PublicationCreateDto dto, IFormFile image)
        {
            if (dto == null)
                return BadRequest("Dados inválidos.");

            if (image == null || image.Length == 0)
                return BadRequest("A imagem é obrigatória.");

            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Token 'sub' claim is missing.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return NotFound("Usuário não encontrado no banco local.");

            var publication = new Publication
            {
                Title = dto.Title,
                Description = dto.Description,
                Salary = dto.Salary,
                Local = dto.Local,
                Shift = dto.Shift,
                ExpirationDate = dto.ExpirationDate,
                Contract = dto.Contract,
                CreatedByUserId = user.Id,
                CreationDate = DateTime.UtcNow
            };

            Guid? companyId = null;

            var companyClaim = User.FindFirst("company_id")?.Value;
            if (!string.IsNullOrEmpty(companyClaim) && Guid.TryParse(companyClaim, out var claimCompanyId))
            {
                companyId = claimCompanyId;
            }

            if (dto.PostAsCompanyId.HasValue)
            {
                bool hasAccess = await _companyRepository.UserHasAccessToCompanyAsync(user.Id, dto.PostAsCompanyId.Value);
                if (!hasAccess)
                    return Forbid("Você não tem permissão para postar por esta empresa.");

                companyId = dto.PostAsCompanyId;
            }

            if (companyId.HasValue)
                publication.AuthorCompanyId = companyId.Value;
            else
                publication.AuthorUserId = user.Id;

            string imageUrl = await _azureBlobService.UploadPostImage(image, "publications", Guid.NewGuid());
            publication.ImageUrl = imageUrl;

            try
            {
                publication.Resumee = await _geminiService.CreateSummaryAsync(
                    dto.Description, dto.Shift, dto.Local, dto.Contract, 80, dto.Salary);
            }
            catch
            {
                publication.Resumee = string.Join(" ",
                    dto.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(30));
            }

            await _publicationRepository.AddAsync(publication);

            var publicationDto = _mapper.Map<PublicationDto>(publication);
            return CreatedAtAction(nameof(GetById), new { id = publication.Id }, publicationDto);
        }

        //[HttpPost]
        //[Authorize]
        //public async Task<IActionResult> Post([FromForm] PublicationCreateDto dto, IFormFile image)
        //{
        //    if (dto == null)
        //    {
        //        return BadRequest("Dados inválidos.");
        //    }

        //    if (image == null || image.Length == 0)
        //    {
        //        return BadRequest("A imagem é obrigatória.");
        //    }

        //    var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

        //    if (string.IsNullOrEmpty(keycloakId))
        //    {
        //        return Unauthorized("Token 'sub' claim is missing.");
        //    }

        //    var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);

        //    if (user == null)
        //    {
        //        return NotFound("Usuário não encontrado no banco de dados local.");
        //    }

        //    var userLocalId = user.Id;

        //    var publication = new Publication
        //    {
        //        Title = dto.Title,
        //        Description = dto.Description,
        //        Salary = dto.Salary,
        //        Local = dto.Local,
        //        Shift = dto.Shift,
        //        ExpirationDate = dto.ExpirationDate,
        //        Contract = dto.Contract,
        //        CreatedByUserId = userLocalId,
        //    };

        //    if (dto.PostAsCompanyId.HasValue)
        //    {
        //        bool userOwnsCompany = await _companyRepository.UserHasAccessToCompanyAsync(userLocalId, dto.PostAsCompanyId.Value);
        //        if (!userOwnsCompany)
        //        {
        //            return StatusCode(StatusCodes.Status403Forbidden, "Você não tem permissão para postar em nome desta empresa.");
        //        }

        //        publication.AuthorCompanyId = dto.PostAsCompanyId.Value;
        //    }
        //    else
        //    {
        //        publication.AuthorUserId = userLocalId;
        //    }

        //    try
        //    {
        //        string containerName = "publications";
        //        string imageUrl = await _azureBlobService.UploadPostImage(image, containerName, Guid.NewGuid());
        //        publication.ImageUrl = imageUrl;

        //        try
        //        {
        //            publication.Resumee = await _geminiService.CreateSummaryAsync(
        //                dto.Description,
        //                dto.Shift,
        //                dto.Local,
        //                dto.Contract,
        //                80,
        //                dto.Salary
        //            );
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"[Gemini Error] Falha ao gerar resumo automático: {ex.Message}");

        //            publication.Resumee = string.Join(" ", dto.Description
        //                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
        //                .Take(30));
        //        }

        //        await _publicationRepository.AddAsync(publication);

        //        var publicationDto = _mapper.Map<PublicationDto>(publication);

        //        return CreatedAtAction(nameof(GetById), new { id = publication.Id }, publicationDto);
        //    }

        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Erro ao criar publicação: {ex.Message}" });
        //    }
        //}

        [HttpPatch("disable/{id}")]
        public async Task<IActionResult> DesactivePost(Guid id)
        {
            var publication = await _publicationRepository.GetByIdAsync(id);
            if (publication == null)
            {
                return NotFound();
            }

            publication.IsActive = PublicationAvailable.Disabled;

            await _publicationRepository.UpdateAsync(publication);

            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(Guid id, [FromForm] PublicationUpdateDto dto, IFormFile? image)
        {
            var existingPublication = await _publicationRepository.GetByIdAsync(id);
            if (existingPublication == null)
                return NotFound("Publicação não encontrada.");

            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Token 'sub' claim is missing.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return Unauthorized("Usuário não encontrado no banco de dados local.");

            // Verificação de autorização
            bool isAuthorized = false;

            if (existingPublication.AuthorUserId.HasValue && existingPublication.AuthorUserId.Value == user.Id)
            {
                isAuthorized = true;
            }
            else if (existingPublication.AuthorCompanyId.HasValue)
            {
                bool userOwnsCompany = await _companyRepository.UserHasAccessToCompanyAsync(user.Id, existingPublication.AuthorCompanyId.Value);
                if (userOwnsCompany)
                    isAuthorized = true;
            }

            if (!isAuthorized)
                return Forbid();

            // Atualiza campos da publicação
            _mapper.Map(dto, existingPublication);

            // Atualiza imagem se fornecida
            if (image != null)
            {
                var imageUrl = await _azureBlobService.UploadPostImage(image, "publications", Guid.NewGuid());
                existingPublication.ImageUrl = imageUrl;
            }

            // Define o contexto ativo (company_id)
            var companyClaim = User.FindFirst("company_id")?.Value;
            if (!string.IsNullOrEmpty(companyClaim) && Guid.TryParse(companyClaim, out var companyId))
            {
                existingPublication.AuthorCompanyId = companyId;
                existingPublication.AuthorUserId = null; // garante que FK de usuário não será violada
            }
            else
            {
                existingPublication.AuthorUserId = user.Id;
                existingPublication.AuthorCompanyId = null; // garante que FK de empresa não será violada
            }

            try
            {
                await _publicationRepository.UpdateAsync(existingPublication);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar a publicação: {ex.Message}");
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingPublication = await _publicationRepository.GetByIdAsync(id);
            if (existingPublication == null)
                return NotFound();

            await _publicationRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] PublicationFilterDto filters)
        {
            var publications = await _publicationRepository.FilterPublicationsAsync(filters);

            if (publications == null || !publications.Any())
            {
                return NotFound("Nenhuma publicação encontrada com os critérios de busca.");
            }

            var response = _mapper.Map<List<PublicationDto>>(publications);

            return Ok(response);
        }
    }
}
