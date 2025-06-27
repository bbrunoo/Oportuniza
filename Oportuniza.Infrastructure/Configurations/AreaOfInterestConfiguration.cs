using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class AreaOfInterestConfiguration : IEntityTypeConfiguration<AreaOfInterest>
    {
        public void Configure(EntityTypeBuilder<AreaOfInterest> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.InterestArea)
                   .IsRequired()
                   .HasMaxLength(100);
        }
    }

}
