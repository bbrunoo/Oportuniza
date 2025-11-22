using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class CandidateApplicationConfiguration : IEntityTypeConfiguration<CandidateApplication>
    {
        public void Configure(EntityTypeBuilder<CandidateApplication> builder)
        {
            builder.HasKey(ca => ca.Id);

            builder.Property(ca => ca.ApplicationDate)
                   .IsRequired();

            builder.Property(ca => ca.Status)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(ca => ca.UserIdKeycloak)
                   .HasMaxLength(128);

            builder.Property(ca => ca.PublicationId)
                   .IsRequired();

            builder.HasOne(ca => ca.User)
                   .WithMany(u => u.CandidateApplication)
                   .HasForeignKey(ca => ca.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ca => ca.Publication)
                   .WithMany(p => p.CandidateApplication)
                   .HasForeignKey(ca => ca.PublicationId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ca => ca.CandidateExtra)
                   .WithOne(ce => ce.CandidateApplication)
                   .HasForeignKey<CandidateExtra>(ce => ce.CandidateApplicationId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("CandidateApplications");
        }
    }
}
