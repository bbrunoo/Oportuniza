using Microsoft.EntityFrameworkCore;
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

            builder.Property(x => x.Desc)
                   .HasMaxLength(500);

            builder.Property(x => x.Email)
               .HasMaxLength(150);

            builder.Property(x => x.ImageUrl)
                   .HasMaxLength(300);
        }
    }
}
