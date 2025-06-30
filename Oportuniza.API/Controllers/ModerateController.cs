using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Oportuniza.Domain.DTOs.Publication;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Linq.Expressions;

namespace Oportuniza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Post.Moderate")]
    public class ModerateController : ControllerBase
    {
        private readonly IPublicationRepository _publicationRepository;
        private readonly IMapper _mapper;

        public ModerateController(IPublicationRepository publicationRepository, IMapper mapper)
        {
            _publicationRepository = publicationRepository;
            _mapper = mapper;
        }

        [HttpGet("pendentes")]
        public async Task<IActionResult> GetPendingPosts()
        {
            var pendentes = await _publicationRepository.GetAllAsync(
                filter: p => p.Status == PublicationStatus.Pending,
                orderBy: q => q.OrderByDescending(p => p.CreationDate),
                includes: new Expression<Func<Publication, object>>[]
                {
                    p => p.AuthorUser,
                    p => p.AuthorCompany
                });

            if (!pendentes.Any())
                return NotFound("Nenhuma publicação pendente.");

            var response = _mapper.Map<List<PublicationDto>>(pendentes);
            return Ok(response);
        }

        [HttpGet("rejecteds")]
        public async Task<IActionResult> GetRejectedPosts()
        {
            var rejected = await _publicationRepository.GetAllAsync(
                filter: p => p.Status == PublicationStatus.Rejected,
                orderBy: q => q.OrderByDescending(p => p.CreationDate),
                includes: new Expression<Func<Publication, object>>[]
                {
                    p => p.AuthorUser,
                    p => p.AuthorCompany
                });

            if (!rejected.Any())
                return NotFound("Nenhuma publicação rejeitada.");

            var response = _mapper.Map<List<PublicationDto>>(rejected);
            return Ok(response);
        }

        [HttpPost("{id}/aprovar")]
        public async Task<IActionResult> ApprovePost(Guid id)
        {
            var post = await _publicationRepository.GetByIdAsync(id);
            if (post == null)
                return NotFound("Publicação não encontrada.");

            if (post.Status != PublicationStatus.Pending)
                return BadRequest("A publicação já foi moderada.");

            post.Status = PublicationStatus.Approved;
            await _publicationRepository.UpdateAsync(post);

            return Ok("Publicação aprovada com sucesso.");
        }

        [HttpPost("{id}/rejeitar")]
        public async Task<IActionResult> RejectPost(Guid id, [FromBody] string motivo)
        {
            var post = await _publicationRepository.GetByIdAsync(id);
            if (post == null)
                return NotFound("Publicação não encontrada.");

            if (post.Status != PublicationStatus.Pending)
                return BadRequest("A publicação já foi moderada.");

            post.Status = PublicationStatus.Rejected;
            return Ok("Publicação rejeitada com sucesso.");
        }

        [HttpGet("debug")]
        [Authorize]
        public IActionResult DebugRoles()
        {
            var roles = User.FindFirst(ClaimConstants.Role)?.Value;
            return Ok(new { roles });
        }
    }
}
