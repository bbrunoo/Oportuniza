using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
    {
        public void Configure(EntityTypeBuilder<Experience> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Position)
                   .HasMaxLength(100);

            builder.Property(x => x.Company)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasOne(x => x.Curriculum)
                   .WithMany(c => c.Experiences)
                   .HasForeignKey(x => x.CurriculumId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
