using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
    {
        public void Configure(EntityTypeBuilder<Certification> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.FileUrl)
                   .HasMaxLength(300);

            builder.HasOne(x => x.Curriculum)
                   .WithMany(c => c.Certifications)
                   .HasForeignKey(x => x.CurriculumId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
