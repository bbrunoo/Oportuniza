using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.Candidates;
using Oportuniza.Domain.DTOs.Extra;
using Oportuniza.Domain.DTOs.Publication;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Repositories;
using System.Security.Claims;

namespace Oportuniza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateApplicationController : ControllerBase
    {
        private readonly ICandidateApplicationRepository _repository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IMapper _mapper;
        public CandidateApplicationController(ICandidateApplicationRepository repository, ICompanyRepository companyRepository, IUserRepository userRepository, IPublicationRepository publicationRepository, IMapper mapper)
        {
            _repository = repository;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _publicationRepository = publicationRepository;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateCandidatesDTO dto)
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Identificador do usuário não encontrado no token.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return Unauthorized("Usuário não registrado no sistema.");

            var publication = await _publicationRepository.GetByIdAsync(dto.PublicationId);
            if (publication == null)
                return NotFound("Publicação não encontrada.");

            if (publication.AuthorUserId == user.Id)
                return BadRequest("Você não pode se candidatar na sua própria postagem.");

            if (publication.AuthorCompanyId is Guid companyId)
            {
                var company = await _companyRepository.GetByIdAsync(companyId);
                if (company != null)
                {
                    if (company.UserId == user.Id)
                        return BadRequest("Você não pode se candidatar em vagas da sua própria empresa.");
                }
            }

            if (await _repository.HasAppliedAsync(dto.PublicationId, user.Id))
                return BadRequest("Você já se candidatou para esta vaga.");

            var authorId = publication.AuthorUserId ?? Guid.Empty;

            var entity = new CandidateApplication
            {
                PublicationId = dto.PublicationId,
                UserId = user.Id,
                UserIdKeycloak = keycloakId,
                ApplicationDate = DateTime.UtcNow,
                PublicationAuthorId = authorId,
                Status = CandidateApplicationStatus.Pending
            };

            var created = await _repository.AddAsync(entity);
            var result = _mapper.Map<CandidatesDTO>(created);

            return Ok(result);
        }

        // PUT: api/CandidateApplication/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PutCandidatesDTO dto)
        {
            if (id != dto.Id) return BadRequest("Id mismatch.");

            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound();

            _mapper.Map(dto, entity);
            var updated = await _repository.UpdateAsync(entity);
            var result = _mapper.Map<CandidatesDTO>(updated);

            return Ok(result);
        }

        // GET: api/CandidateApplication
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var apps = await _repository.GetAllAsync(ca => ca.User, ca => ca.Publication);
            var result = _mapper.Map<IEnumerable<CandidatesDTO>>(apps);
            return Ok(result);
        }

        // GET: api/CandidateApplication/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var app = await _repository.GetByIdAsync(id, ca => ca.User, ca => ca.Publication);
            if (app == null) return NotFound();

            var result = _mapper.Map<CandidatesDTO>(app);
            return Ok(result);
        }

        // GET: api/CandidateApplication/ByPublication/{publicationId}
        [HttpGet("ByPublication/{publicationId}")]
        public async Task<IActionResult> GetCandidatesByPublication(Guid publicationId)
        {
            var apps = await _repository.GetCandidatesByPublicationAsync(publicationId);
            var result = _mapper.Map<IEnumerable<CandidatesDTO>>(apps);
            return Ok(result);
        }
        //}

        [HttpGet("MyPublications/Candidates")]
        [Authorize]
        public async Task<ActionResult<List<PublicationWithCandidatesDto>>> GetMyPublicationsWithCandidates()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Identificador do usuário não encontrado no token.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(userIdClaim);
            if (user == null)
            {
                return Unauthorized("Usuário não encontrado no banco de dados local.");
            }

            var candidates = await _repository.GetPublicationsWithCandidatesByAuthorAsync(user.Id);
            var result = _mapper.Map<IEnumerable<PublicationWithCandidatesDto>>(candidates);

            return Ok(result);
        }

        // GET: api/CandidateApplication/ByUser/{userId}
        [HttpGet("ByUser/{userId}")]
        public async Task<IActionResult> GetApplicationsByUser(Guid userId)
        {
            var apps = await _repository.GetApplicationsByUserAsync(userId);
            var result = _mapper.Map<IEnumerable<CandidatesDTO>>(apps);
            return Ok(result);
        }

        // GET: api/CandidateApplication/ByUser/{userId}
        [HttpGet("MyApplications")]
        public async Task<IActionResult> GetMyApplications()
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(keycloakId))
            {
                return Unauthorized("Token 'sub' claim is missing.");
            }

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);

            if (user == null)
            {
                return NotFound("Usuário não encontrado no banco de dados local.");
            }

            var apps = await _repository.GetApplicationsLoggedUser(user.Id);
            var result = _mapper.Map<IEnumerable<UserApplicationDto>>(apps);
            return Ok(result);
        }

        // GET: api/CandidateApplication/HasApplied?publicationId=xxx&userId=yyy
        [HttpGet("HasApplied")]
        public async Task<IActionResult> HasApplied(Guid publicationId, Guid userId)
        {
            var exists = await _repository.HasAppliedAsync(publicationId, userId);
            return Ok(new { hasApplied = exists });
        }

        // GET: api/CandidateApplication/Statistics/{publicationId}
        [HttpGet("Statistics/{publicationId}")]
        public async Task<IActionResult> GetPublicationStatistics(Guid publicationId)
        {
            var stats = await _repository.GetPublicationStatisticsAsync(publicationId);
            return Ok(stats);
        }

        [HttpGet("Status")]
        public async Task<IActionResult> GetApplicationStatus(Guid publicationId, Guid userId)
        {
            var application = await _repository.GetApplicationByPublicationAndUserAsync(publicationId, userId);

            if (application == null)
            {
                // Retorna null ou um valor específico se não houver candidatura
                return Ok(new { status = (int?)null });
            }

            return Ok(new { status = (int)application.Status });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound("Candidatura não encontrada.");

            await _repository.DeleteAsync(entity.Id);

            return NoContent();
        }
    }
}
