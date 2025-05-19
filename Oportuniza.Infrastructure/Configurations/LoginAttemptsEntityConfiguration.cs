using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
