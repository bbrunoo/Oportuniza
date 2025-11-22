using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class CompanyRoleConfiguration : IEntityTypeConfiguration<CompanyRole>
    {
        public void Configure(EntityTypeBuilder<CompanyRole> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasData(
                new CompanyRole { Id = Guid.NewGuid(), Name = "Owner" },
                new CompanyRole { Id = Guid.NewGuid(), Name = "Administrator" },
                new CompanyRole { Id = Guid.NewGuid(), Name = "Worker" });
        }
    }
}
