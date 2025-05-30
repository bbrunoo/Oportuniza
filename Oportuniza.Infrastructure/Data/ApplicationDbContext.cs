using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Configurations;

namespace Oportuniza.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ChatMessage> ChatMessage { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        public DbSet<PrivateChat> PrivateChat { get; set; }
        public DbSet<LoginAttempt> LoginAttempt { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<AreaOfInterest> AreasOfInterest { get; set; }
        public DbSet<Certification> Certification { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<CompanyUser> CompanyUser { get; set; }
        public DbSet<Curriculum> Curriculum { get; set; }
        public DbSet<Education> Education { get; set; }
        public DbSet<Experience> Experience { get; set; }
        public DbSet<Publication> Publication { get; set; }
        public DbSet<UserAreaOfInterest> UserAreaOfInterest { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ChatMessageEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ChatParticipantsEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ConversationsEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LoginAttemptsEntityConfiguration());

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new AreaOfInterestConfiguration());
            modelBuilder.ApplyConfiguration(new CertificationConfiguration());
            modelBuilder.ApplyConfiguration(new CityConfiguration());
            modelBuilder.ApplyConfiguration(new CompanyConfiguration());
            modelBuilder.ApplyConfiguration(new CompanyUserConfiguration());
            modelBuilder.ApplyConfiguration(new CurriculumConfiguration());
            modelBuilder.ApplyConfiguration(new EducationConfiguration());
            modelBuilder.ApplyConfiguration(new ExperienceConfiguration());
            modelBuilder.ApplyConfiguration(new PublicationConfiguration());
            modelBuilder.ApplyConfiguration(new UserAreaOfInterestConfiguration());


            base.OnModelCreating(modelBuilder);
        }
    }
}
