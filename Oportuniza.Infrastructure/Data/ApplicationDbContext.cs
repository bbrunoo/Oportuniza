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
        public virtual DbSet<ChatParticipant> ChatParticipants { get; set; }
        public virtual DbSet<PrivateChat> PrivateChat { get; set; }
        public DbSet<LoginAttempt> LoginAttempt { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ChatMessageEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ChatParticipantsEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ConversationsEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LoginAttemptsEntityConfiguration());

            base.OnModelCreating(modelBuilder); 
        }
    }
}
