using Oportuniza.Domain.Models;

namespace Oportuniza.Domain.Interfaces
{
    public interface ICurriculumRepository : IRepository<Curriculum>
    {
        Task AddEducationAsync(Guid curriculumId, Education education);
        Task RemoveEducationAsync(Guid curriculumId, Guid educationId);
        Task AddExperienceAsync(Guid curriculumId, Experience experience);
        Task RemoveExperienceAsync(Guid curriculumId, Guid experienceId);
        Task AddCertificationAsync(Guid curriculumId, Certification certification);
        Task RemoveCertificationAsync(Guid curriculumId, Guid certificationId);
        Task<bool> CityExistsAsync(Guid cityId);
    }
}
