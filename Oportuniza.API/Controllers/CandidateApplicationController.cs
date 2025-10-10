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
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var companyIdClaim = User.FindFirst("company_id")?.Value;

            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Identificador do usuário não encontrado no token.");

            if (!string.IsNullOrEmpty(companyIdClaim))
                return BadRequest("Empresas não podem se candidatar a vagas.");

            if (!string.IsNullOrEmpty(companyIdClaim))
                return BadRequest("Empresas não podem se candidatar a vagas.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return Unauthorized("Usuário não registrado no sistema.");
                Console.WriteLine("id do user", user.Id);
                Console.WriteLine("id do keycloak", keycloakId);

            var publication = await _publicationRepository.GetByIdAsync(dto.PublicationId);
            if (publication == null)
                return NotFound("Publicação não encontrada.");

            if (publication.AuthorUserId == Guid.Parse(keycloakId))
                return BadRequest("Você não pode se candidatar na sua própria postagem.");

            if (await _repository.HasAppliedAsync(dto.PublicationId, user.Id))
                return BadRequest("Você já se candidatou para esta vaga.");

            var entity = new CandidateApplication
            {
                PublicationId = dto.PublicationId,
                UserId = user.Id,
                UserIdKeycloak = keycloakId,
                ApplicationDate = DateTime.UtcNow,
                PublicationAuthorId = publication.AuthorUserId ?? Guid.Empty,
                Status = CandidateApplicationStatus.Pending
            };

            var created = await _repository.AddAsync(entity);
            var result = _mapper.Map<CandidatesDTO>(created);

            return Ok(result);
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

        [HttpGet("ByPublication/{publicationId}")]
        public async Task<IActionResult> GetCandidatesByPublication(Guid publicationId)
        {
            var apps = await _repository.GetCandidatesByPublicationAsync(publicationId);
            var result = _mapper.Map<IEnumerable<CandidatesDTO>>(apps);
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

        // ---------- USAR DEPOIS ----------

        //[HttpGet("MyApplications")]
        //[Authorize]
        //public async Task<IActionResult> GetMyApplications()
        //{
        //    var companyIdClaim = User.FindFirst("company_id")?.Value;
        //    var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    if (string.IsNullOrEmpty(keycloakId))
        //        return Unauthorized("Usuário não encontrado no token.");

        //    // 1. CONTEXTO DE EMPRESA (COMPANY_ID PRESENTE)
        //    if (!string.IsNullOrEmpty(companyIdClaim))
        //    {
        //        if (!Guid.TryParse(companyIdClaim, out var companyId))
        //            return BadRequest("CompanyId inválido.");

        //        // Retorna as candidaturas para as vagas desta empresa
        //        var appsForCompany = await _repository.GetApplicationsByCompanyAsync(companyId);

        //        // Se a empresa não tiver candidaturas (ou publicações), retorna NoContent
        //        if (appsForCompany == null || !appsForCompany.Any())
        //            return NoContent(); // <--- Retornar 204 NoContent para indicar "sem dados"

        //        var result = _mapper.Map<IEnumerable<CandidatesDTO>>(appsForCompany);
        //        return Ok(result);
        //    }
        //    // 2. CONTEXTO DE USUÁRIO (CANDIDATO)
        //    else
        //    {
        //        var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
        //        if (user == null)
        //            return NotFound("Usuário não encontrado.");

        //        // Retorna as candidaturas que o usuário fez
        //        var appsForUser = await _repository.GetApplicationsLoggedUser(user.Id);

        //        // Se o usuário não tiver candidaturas, retorna NoContent
        //        if (appsForUser == null || !appsForUser.Any())
        //            return NoContent(); // <--- Retornar 204 NoContent para indicar "sem dados"

        //        var result = _mapper.Map<IEnumerable<UserApplicationDto>>(appsForUser);
        //        return Ok(result);
        //    }
        //}

        [HttpGet("HasApplied")]
        public async Task<IActionResult> HasApplied(Guid publicationId, Guid userId)
        {
            var exists = await _repository.HasAppliedAsync(publicationId, userId);
            return Ok(new { hasApplied = exists });
        }

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
            var result = _mapper.Map<IEnumerable<CandidatesDTO>>(apps);
            return Ok(result);
        }

        [HttpGet("MyUserApplications")]
        [Authorize]
        public async Task<IActionResult> GetApplicationsByUserContext()
        {
            var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(keycloakId))
                return Unauthorized("Usuário não encontrado no token.");

            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            var apps = await _repository.GetApplicationsLoggedUser(user.Id);
            var result = _mapper.Map<IEnumerable<CandidatesDTO>>(apps);
            return Ok(result);
        }
    }
}
