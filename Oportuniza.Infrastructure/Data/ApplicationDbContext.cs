using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Configurations;

namespace Oportuniza.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<LoginAttempt> LoginAttempt { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<CandidateApplication> CandidateApplication { get; set; }
        public DbSet<Publication> Publication { get; set; }
        public DbSet<CompanyEmployee> CompanyEmployee { get; set; }
        public DbSet<CompanyRole> CompanyRole { get; set; }
        public DbSet<CandidateExtra> CandidateExtra { get; set; }
        public DbSet<CNPJCache> CnpjCache { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new LoginAttemptsEntityConfiguration());

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new CityConfiguration());
            modelBuilder.ApplyConfiguration(new CompanyConfiguration());
            modelBuilder.ApplyConfiguration(new PublicationConfiguration());
            modelBuilder.ApplyConfiguration(new CompanyEmployeeConfiguration());
            modelBuilder.ApplyConfiguration(new CandidateApplicationConfiguration());
            modelBuilder.ApplyConfiguration(new CompanyRoleConfiguration());
            modelBuilder.ApplyConfiguration(new CandidateExtraConfiguration());
            modelBuilder.ApplyConfiguration(new CnpjCacheConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
