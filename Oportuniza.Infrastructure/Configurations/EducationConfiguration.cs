using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class EducationConfiguration : IEntityTypeConfiguration<Education>
    {
        public void Configure(EntityTypeBuilder<Education> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Institution)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Course)
                   .HasMaxLength(100);

            builder.HasOne(x => x.Curriculum)
                   .WithMany(c => c.Educations)
                   .HasForeignKey(x => x.CurriculumId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
