﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Description)
                   .HasMaxLength(500);

            builder.Property(x => x.ImageUrl)
                   .HasMaxLength(200);

            builder.Property(c => c.Active)
                 .HasDefaultValue(true);

            builder.HasOne(c => c.Manager)
               .WithMany(u => u.CompaniesOwned) // Cria essa coleção no User
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Employees)
                   .WithOne(e => e.Company)
                   .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
