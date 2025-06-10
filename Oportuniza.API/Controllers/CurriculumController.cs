using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oportuniza.Domain.DTOs.Curriculum;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;

namespace Oportuniza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurriculumController : ControllerBase
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly IMapper _mapper;
        public CurriculumController(ICurriculumRepository curriculumRepository, IMapper mapper)
        {
            _curriculumRepository = curriculumRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var curriculums = await _curriculumRepository.GetAllAsync(
                c => c.Educations,
                c => c.Experiences,
                c => c.Certifications,
                c => c.User,
                c => c.City);

            if (curriculums == null) return NotFound("Currículo não encontrado.");

            var response = _mapper.Map<List<CurriculumDto>>(curriculums);

            return StatusCode(200, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var curriculums = await _curriculumRepository.GetByIdAsync(id,
                c => c.Educations,
                c => c.Experiences,
                c => c.Certifications,
                c => c.User,
                c => c.City);

            if (curriculums == null) return NotFound("Currículo não encontrado.");

            var response = _mapper.Map<CurriculumDto>(curriculums);

            return StatusCode(200, response);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CurriculumCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto == null)
                return BadRequest("Dados inválidos.");

            if (dto.BirthDate > DateTime.Today)
                return BadRequest("Data de nascimento não pode ser no futuro.");

            var cityExists = await _curriculumRepository.CityExistsAsync(dto.CityId);
            if (!cityExists)
                return BadRequest("Cidade não encontrada.");

            var curriculum = _mapper.Map<Curriculum>(dto);
            if (curriculum == null)
                return BadRequest("Erro ao mapear currículo.");

            await _curriculumRepository.AddAsync(curriculum);

            return CreatedAtAction(nameof(GetById), new { id = curriculum.Id }, _mapper.Map<CurriculumDto>(curriculum));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] Curriculum curriculum)
        {
            if (curriculum == null || id != curriculum.Id)
                return BadRequest();

            var existingCurriculum = await _curriculumRepository.GetByIdAsync(id);
            if (existingCurriculum == null)
                return NotFound();

            await _curriculumRepository.UpdateAsync(curriculum);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingCurriculum = await _curriculumRepository.GetByIdAsync(id);
            if (existingCurriculum == null)
                return NotFound();

            await _curriculumRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{curriculumId}/education")]
        public async Task<IActionResult> AddEducation(Guid curriculumId, [FromBody] Education education)
        {
            if (education == null)
                return BadRequest();

            await _curriculumRepository.AddEducationAsync(curriculumId, education);
            return NoContent();
        }

        [HttpDelete("{curriculumId}/education/{educationId}")]
        public async Task<IActionResult> RemoveEducation(Guid curriculumId, Guid educationId)
        {
            await _curriculumRepository.RemoveEducationAsync(curriculumId, educationId);
            return NoContent();
        }

        [HttpPost("{curriculumId}/experience")]
        public async Task<IActionResult> AddExperience(Guid curriculumId, [FromBody] Experience experience)
        {
            if (experience == null)
                return BadRequest();

            await _curriculumRepository.AddExperienceAsync(curriculumId, experience);
            return NoContent();
        }

        [HttpDelete("{curriculumId}/experience/{experienceId}")]
        public async Task<IActionResult> RemoveExperience(Guid curriculumId, Guid experienceId)
        {
            await _curriculumRepository.RemoveExperienceAsync(curriculumId, experienceId);
            return NoContent();
        }

        [HttpPost("{curriculumId}/certification")]
        public async Task<IActionResult> AddCertification(Guid curriculumId, [FromBody] Certification certification)
        {
            if (certification == null)
                return BadRequest();

            await _curriculumRepository.AddCertificationAsync(curriculumId, certification);
            return NoContent();
        }

        [HttpDelete("{curriculumId}/certification/{certificationId}")]
        public async Task<IActionResult> RemoveCertification(Guid curriculumId, Guid certificationId)
        {
            await _curriculumRepository.RemoveCertificationAsync(curriculumId, certificationId);
            return NoContent();
        }
    }
}
