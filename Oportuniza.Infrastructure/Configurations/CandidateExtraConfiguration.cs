using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class CandidateExtraConfiguration : IEntityTypeConfiguration<CandidateExtra>
    {
        public void Configure(EntityTypeBuilder<CandidateExtra> builder)
        {
            builder.ToTable("CandidateExtra");

            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.CandidateApplication)
                   .WithOne(a => a.Extra)
                   .HasForeignKey<CandidateExtra>(e => e.CandidateApplicationId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.Observation)
                   .HasMaxLength(200)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(e => e.ResumeUrl)
                   .HasMaxLength(500)
                   .IsUnicode(false)
                   .IsRequired(false);

            builder.HasIndex(e => e.CandidateApplicationId)
                   .IsUnique();
        }
    }
}
