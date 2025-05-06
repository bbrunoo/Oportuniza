using Microsoft.EntityFrameworkCore;
using Oportuniza.Domain.Models;
using Oportuniza.Domain.Models.ChatModel;
using Oportuniza.Infrastructure.Configurations;
using Oportuniza.Infrastructure.Configurations.ChatConfiguration;

namespace Oportuniza.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base() { }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Message> Message { get; set; }
        public virtual DbSet<Chat> Chat { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ChatEntityConfiguration());
            modelBuilder.ApplyConfiguration(new MessageEntityConfiguration());
            base.OnModelCreating(modelBuilder); 
        }
    }
}
