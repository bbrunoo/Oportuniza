using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Conversations;
using Oportuniza.Domain.Models;
using Oportuniza.Infrastructure.Configurations;

namespace Oportuniza.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Conversation> Conversation { get; set; }
        public virtual DbSet<Message> Message{ get; set; }
        public virtual DbSet<Participant> Participant { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserEntityConfiguration());

            base.OnModelCreating(modelBuilder); 
        }
    }
}
