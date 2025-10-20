using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class LoginAttemptsEntityConfiguration : IEntityTypeConfiguration<LoginAttempt>
    {
        public void Configure(EntityTypeBuilder<LoginAttempt> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.IPAddress)
                   .IsRequired()
                   .HasMaxLength(45); 

            builder.Property(l => l.FailedAttempts)
                   .IsRequired();

            builder.Property(l => l.LockoutEnd)
                   .IsRequired(false);
        }
    }
}
