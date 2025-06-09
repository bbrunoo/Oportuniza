using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class CompanyAreaOfInterestConfiguration : IEntityTypeConfiguration<CompanyAreaOfInterest>
    {
        public void Configure(EntityTypeBuilder<CompanyAreaOfInterest> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Company)
                   .WithMany(u => u.CompanyAreasOfInterest)
                   .HasForeignKey(x => x.CompanyId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AreaOfInterest)
                   .WithMany()
                   .HasForeignKey(x => x.AreaOfInterestId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
