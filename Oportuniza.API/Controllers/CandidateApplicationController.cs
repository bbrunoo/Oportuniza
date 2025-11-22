using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.API.Services;
using Oportuniza.Domain.DTOs.Candidates;
using Oportuniza.Domain.DTOs.Candidates.CandidateExtra;
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
        private readonly ICandidateExtraRepository _candidateExtraRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IMapper _mapper;
        public CandidateApplicationController(ICandidateApplicationRepository repository, ICandidateExtraRepository candidateExtraRepository, ICompanyRepository companyRepository, IUserRepository userRepository, IPublicationRepository publicationRepository, IMapper mapper)
        {
            _repository = repository;
            _candidateExtraRepository = candidateExtraRepository;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _publicationRepository = publicationRepository;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateCandidatesDTO dto)
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var companyIdClaim = User.FindFirst("company_id")?.Value;

            if (string.IsNullOrEmpty(keycloakId))
                return Error("Identificador do usuário não encontrado no token.", 401);

            if (!string.IsNullOrEmpty(companyIdClaim))
                return Error("Empresas não podem se candidatar a vagas.", 400);

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return Error("Usuário não registrado no sistema.", 401);

            var publication = await _publicationRepository.GetByIdAsync(dto.PublicationId);
            if (publication == null)
                return Error("Publicação não encontrada.", 404);

            var empresasDoUsuario = await _companyRepository.GetCompaniesByUserIdAsync(user.Id);
            
            if (publication.AuthorCompanyId.HasValue &&
                empresasDoUsuario.Any(c => c.Id == publication.AuthorCompanyId.Value))
            {
                return Error("Você não pode se candidatar a uma vaga da empresa à qual você pertence.", 400);
            }

            if (publication.AuthorUserId.HasValue && publication.AuthorUserId.Value == user.Id)
                return Error("Você não pode se candidatar à sua própria vaga.", 400);

            if (publication.CreatedByUserId == user.Id)
                return Error("Você não pode se candidatar à vaga que você mesmo postou.", 400);

            if (await _repository.HasAppliedAsync(dto.PublicationId, user.Id))
                return Error("Você já se candidatou para esta vaga.", 400);

            var entity = new CandidateApplication
            {
                PublicationId = dto.PublicationId,
                UserId = user.Id,
                UserIdKeycloak = keycloakId,
                ApplicationDate = DateTime.UtcNow,
                PublicationAuthorId = publication.AuthorUserId ?? publication.CreatedByUserId,
                Status = CandidateApplicationStatus.Pending
            };

            var created = await _repository.AddAsync(entity);
            return Ok(_mapper.Map<CandidatesDTO>(created));
        }

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var apps = await _repository.GetAllAsync(ca => ca.User, ca => ca.Publication);
            var result = _mapper.Map<IEnumerable<CandidatesDTO>>(apps);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var app = await _repository.GetByIdAsync(id, ca => ca.User, ca => ca.Publication);
            if (app == null) return NotFound();

            var result = _mapper.Map<CandidatesDTO>(app);
            return Ok(result);
        }

        [HttpGet("MyPublications/Candidates")]
        [Authorize]
        public async Task<ActionResult<List<PublicationWithCandidatesDto>>> GetMyPublicationsWithCandidates()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Identificador do usuário não encontrado no token.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(userIdClaim);
            if (user == null)
                return Unauthorized("Usuário não encontrado no banco de dados local.");

            var candidates = await _repository.GetPublicationsWithCandidatesByAuthorAsync(user.Id);
            var result = _mapper.Map<IEnumerable<PublicationWithCandidatesDto>>(candidates);

            return Ok(result);
        }

        [HttpGet("ByUser/{userId}")]
        public async Task<IActionResult> GetApplicationsByUser(Guid userId)
        {
            var apps = await _repository.GetApplicationsByUserAsync(userId);
            var result = _mapper.Map<IEnumerable<CandidatesDTO>>(apps);
            return Ok(result);
        }

        [HttpGet("MyApplications")]
        public async Task<IActionResult> GetMyApplications()
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound("Candidatura não encontrada.");

            await _repository.DeleteAsync(entity.Id);

            return NoContent();
        }

        [HttpGet("MyCompanyApplications")]
        [Authorize]
        public async Task<IActionResult> GetApplicationsByCompanyContext()
        {
            var companyIdClaim = User.FindFirst("company_id")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim))
                return BadRequest("Usuário não é uma empresa.");

            if (!Guid.TryParse(companyIdClaim, out var companyId))
                return BadRequest("CompanyId inválido.");

            var apps = await _repository.GetApplicationsByCompanyAsync(companyId);
            var result = _mapper.Map<IEnumerable<CandidateApplicationDetailDto>>(apps);
            return Ok(result);
        }

        [HttpPost("{applicationId}/extra")]
        [Authorize]
        public async Task<IActionResult> AddCandidateExtra(Guid applicationId, [FromForm] CreateCandidateExtraDTO dto, [FromServices] AzureBlobService blobService)
        {
            var application = await _repository.GetByIdAsync(applicationId);
            if (application == null)
                return NotFound("Candidatura não encontrada.");

            if (application.CandidateExtra != null)
                return BadRequest("Já existe um extra associado a esta candidatura.");

            if (dto.Resume == null)
                return BadRequest("O currículo é obrigatório.");

            var resumeUrl = await blobService.UploadResumeAsync(dto.Resume, "resumes", application.UserId);

            var extra = new CandidateExtra
            {
                CandidateApplicationId = applicationId,
                Observation = dto.Observation,
                ResumeUrl = resumeUrl
            };

            var createdExtra = await _candidateExtraRepository.AddAsync(extra);
            var result = _mapper.Map<CandidateExtraDTO>(createdExtra);

            return Ok(result);
        }
        private IActionResult Error(string message, int status)
        {
            return StatusCode(status, new { error = message });
        }
    }
}
