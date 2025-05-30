using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Data;

namespace Oportuniza.Infrastructure.Repositories
{
    public class CurriculumRepository : Repository<Curriculum>, ICurriculumRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Curriculum> _dbSet;

        public CurriculumRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<Curriculum>();
        }
        public async Task AddEducationAsync(Guid curriculumId, Education education)
        {
            var curriculum = await _dbSet
                .Include(c => c.Educations)
                .FirstOrDefaultAsync(c => c.Id == curriculumId);

            if (curriculum == null) throw new Exception("Curriculum not found");

            curriculum.Educations.Add(education);
            await _context.SaveChangesAsync();
        }
        public async Task RemoveEducationAsync(Guid curriculumId, Guid educationId)
        {
            var curriculum = await _dbSet
                .Include(c => c.Educations)
                .FirstOrDefaultAsync(C => C.Id == curriculumId);

            if (curriculum == null) throw new Exception("Curriculum not found");
            
            var education = curriculum.Educations.FirstOrDefault(e => e.Id == educationId);

            if (education != null)
            {
                curriculum.Educations.Remove(education);
                await _context.SaveChangesAsync();
            }
        }
        public async Task AddCertificationAsync(Guid curriculumId, Certification certification)
        {
            var curriculum = await _dbSet
               .Include(c => c.Educations)
               .FirstOrDefaultAsync(c => c.Id == curriculumId);

            if (curriculum == null) throw new Exception("Curriculum not found");

            curriculum.Certifications.Add(certification);
            await _context.SaveChangesAsync();
        }
        public async Task RemoveCertificationAsync(Guid curriculumId, Guid certificationId)
        {
            var curriculum = await _dbSet
                .Include(c => c.Certifications)
                .FirstOrDefaultAsync(C => C.Id == curriculumId);

            if (curriculum == null) throw new Exception("Curriculum not found");

            var certification = curriculum.Certifications.FirstOrDefault(e => e.Id == certificationId);

            if (certification != null)
            {
                curriculum.Certifications.Remove(certification);
                await _context.SaveChangesAsync();
            }
        }
        public async Task AddExperienceAsync(Guid curriculumId, Experience experience)
        {
            var curriculum = await _dbSet
               .Include(c => c.Experiences)
               .FirstOrDefaultAsync(c => c.Id == curriculumId);

            if (curriculum == null) throw new Exception("Curriculum not found");

            curriculum.Experiences.Add(experience);
            await _context.SaveChangesAsync();
        }
        public async Task RemoveExperienceAsync(Guid curriculumId, Guid experienceId)
        {
            var curriculum = await _dbSet
                .Include(c => c.Experiences)
                .FirstOrDefaultAsync(C => C.Id == curriculumId);

            if (curriculum == null) throw new Exception("Curriculum not found");

            var experience = curriculum.Experiences.FirstOrDefault(e => e.Id == experienceId);

            if (experience != null)
            {
                curriculum.Experiences.Remove(experience);
                await _context.SaveChangesAsync();
            }
        }
    }
}
