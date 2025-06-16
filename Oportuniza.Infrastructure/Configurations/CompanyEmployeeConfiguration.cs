using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class CompanyEmployeeConfiguration : IEntityTypeConfiguration<CompanyEmployee>
    {
        public void Configure(EntityTypeBuilder<CompanyEmployee> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.User)
                  .WithMany(u => u.CompanyLinks) 
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Company)
                  .WithMany(c => c.Employees)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Roles)
                   .IsRequired();

            builder.Property(e => e.CanPostJobs)
                   .HasDefaultValue(false);
        }
    }
}
