using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class CandidateExtraConfiguration : IEntityTypeConfiguration<CandidateExtra>
    {
        public void Configure(EntityTypeBuilder<CandidateExtra> builder)
        {
            builder.HasKey(ce => ce.Id);

            builder.Property(ce => ce.Observation)
                   .HasMaxLength(2000);

            builder.Property(ce => ce.ResumeUrl)
                   .HasMaxLength(500);

            builder.HasOne(ce => ce.CandidateApplication)
                   .WithOne(ca => ca.CandidateExtra)
                   .HasForeignKey<CandidateExtra>(ce => ce.CandidateApplicationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
