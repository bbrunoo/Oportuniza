using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Configurations;

namespace Oportuniza.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public virtual DbSet<User> User { get; set; }

        public virtual DbSet<ChatMessage> ChatMessage { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ChatMessageEntityConfiguration());

            base.OnModelCreating(modelBuilder); 
        }
    }
}
