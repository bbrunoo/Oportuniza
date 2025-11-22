using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Oportuniza.API.Services;
using Oportuniza.Domain.DTOs;
using Oportuniza.Domain.DTOs.Publication;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Sprache;
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
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IEmailService _emailService;
        private readonly GeolocationService _geolocationService;

        public PublicationController(
            IPublicationRepository publicationRepository,
            AzureBlobService azureBlobService,
            IConfiguration configuration,
            ICompanyRepository companyRepository,
            IUserRepository userRepository,
            IMapper mapper,
            GeminiClientService geminiService,
            IVerificationCodeService verificationCodeService,
            IEmailService emailService,
            GeolocationService geolocationService)
        {
            _publicationRepository = publicationRepository;
            _azureBlobService = azureBlobService;
            _configuration = configuration;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _geminiService = geminiService;
            _verificationCodeService = verificationCodeService;
            _emailService = emailService;
            _geolocationService = geolocationService;
        }

        [HttpPost("send-verification")]
        [Authorize]
        public async Task<IActionResult> SendPublicationVerificationCode()
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Token inválido.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return NotFound("Usuário não encontrado no banco de dados.");

            var code = _verificationCodeService.GenerateCode(user.Email, "post");
            var message = "Use o código abaixo para confirmar sua postagem no Oportuniza:";

            var success = await _emailService.SendVerificationEmailAsync(
                user.Email,
                "Verificação de Postagem - Oportuniza",
                message,
                code
            );

            if (!success)
                return StatusCode(500, "Falha ao enviar e-mail de verificação de postagem.");

            return Ok(new { message = "Código de verificação de postagem enviado com sucesso.", expiresInSeconds = 60 });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var publications = await _publicationRepository.GetAllAsync(
                orderBy: q => q.OrderByDescending(c => c.CreationDate),
                includes: new Expression<Func<Publication, object>>[]
                {
                p => p.CreatedByUser,
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

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] PublicationCreateDto dto, IFormFile image, [FromForm] string verificationCode)
        {
            if (dto == null)
                return BadRequest("Dados inválidos.");

            if (image == null || image.Length == 0)
                return BadRequest("A imagem é obrigatória.");

            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Token inválido.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return NotFound("Usuário não encontrado no banco local.");

            if (!_verificationCodeService.ValidateCode(user.Email, verificationCode, "post"))
                return BadRequest("Código de verificação inválido ou expirado.");

            var publication = new Publication
            {
                Title = dto.Title,
                Description = dto.Description,
                Resumee = dto.Description, //caso tenha internet, remover para usar a IA para gerar o texto corretamente.
                Salary = dto.Salary,
                Local = dto.Local,
                Shift = dto.Shift,
                ExpirationDate = dto.ExpirationDate,
                Contract = dto.Contract,
                CreatedByUserId = user.Id,
                CreationDate = DateTime.UtcNow,
                PostAuthorName = user.Name
            };

            Guid? companyId = null;
            var companyClaim = User.FindFirst("company_id")?.Value;
            if (!string.IsNullOrEmpty(companyClaim) && Guid.TryParse(companyClaim, out var claimCompanyId))
                companyId = claimCompanyId;

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

            try
            {
                var (lat, lng) = await _geolocationService.GetCoordinatesAsync(dto.Local);
                publication.Latitude = lat;
                publication.Longitude = lng;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeolocationService] Falha ao obter coordenadas: {ex.Message}");
                publication.Latitude = null;
                publication.Longitude = null;
            }

            try
            {
                string imageUrl = await _azureBlobService.UploadPostImage(image, "publications", Guid.NewGuid());
                publication.ImageUrl = imageUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AzureBlobService] Falha ao processar imagem: {ex.Message}");
                publication.ImageUrl = null;
            }

            //try
            //{
            //    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            //    publication.Resumee = await _geminiService.CreateSummaryAsync(
            //        dto.Description, dto.Shift, dto.Local, dto.Contract, 80, dto.Salary);
            //}
            //catch (OperationCanceledException)
            //{
            //    Console.WriteLine("[GeminiService] Timeout ao gerar resumo com IA.");
            //    publication.Resumee = string.Join(" ",
            //        dto.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(30));
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"[GeminiService] Falha ao gerar resumo com IA: {ex.Message}");
            //    publication.Resumee = string.Join(" ",
            //        dto.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(30));
            //}

            await _publicationRepository.AddAsync(publication);
            var publicationDto = _mapper.Map<PublicationDto>(publication);
            return CreatedAtAction(nameof(GetById), new { id = publication.Id }, publicationDto);
        }

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

            _mapper.Map(dto, existingPublication);

            if (image != null)
            {
                var imageUrl = await _azureBlobService.UploadPostImage(image, "publications", Guid.NewGuid());
                existingPublication.ImageUrl = imageUrl;
            }

            var companyClaim = User.FindFirst("company_id")?.Value;
            if (!string.IsNullOrEmpty(companyClaim) && Guid.TryParse(companyClaim, out var companyId))
            {
                existingPublication.AuthorCompanyId = companyId;
                existingPublication.AuthorUserId = null;
            }
            else
            {
                existingPublication.AuthorUserId = user.Id;
                existingPublication.AuthorCompanyId = null;
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

        [HttpPost("validate-image")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ValidateImage([FromForm] ImageValidationDto dto)
        {
            var file = dto.File;

            if (file == null || file.Length == 0)
                return BadRequest("Arquivo inválido.");

            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                bool isSafe = await _azureBlobService.IsImageSafeAsync(memoryStream, file.ContentType);
                return Ok(new { isSafe });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ValidateImage] Falha: {ex.Message}");
                return StatusCode(500, new
                {
                    error = "Falha ao analisar imagem.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("random-samples")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRandomPublications()
        {
            var publications = await _publicationRepository.GetAllAsync(
                includes: new Expression<Func<Publication, object>>[]
                {
            p => p.CreatedByUser,
            p => p.AuthorUser,
            p => p.AuthorCompany
                },
                filter: p => p.IsActive == PublicationAvailable.Enabled
            );

            if (publications == null || !publications.Any())
                return NotFound("Nenhuma publicação disponível para amostra.");

            var rnd = new Random();
            var shuffled = publications.OrderBy(_ => rnd.Next()).ToList();

            List<Publication> result;

            if (shuffled.Count >= 2)
                result = shuffled.Take(2).ToList();
            else if (shuffled.Count == 1)
                result = shuffled.Take(1).ToList();
            else
                return NotFound("Nenhuma publicação disponível para amostra.");

            var response = _mapper.Map<List<PublicationDto>>(result);
            return Ok(response);
        }
    }
}
