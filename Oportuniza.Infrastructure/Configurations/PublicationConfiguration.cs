using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class PublicationConfiguration : IEntityTypeConfiguration<Publication>
    {
        public void Configure(EntityTypeBuilder<Publication> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasColumnType("TEXT");

            builder.Property(p => p.CreationDate)
                .IsRequired();

            builder.Property(p => p.ImageUrl)
                .HasMaxLength(300);

            builder.Property(p => p.Salary)
                .HasMaxLength(20);

            builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(PublicationStatus.Pending);

            builder.Property(p => p.Expired)
                .IsRequired();

            builder.HasOne(p => p.CreatedByUser)
               .WithMany(u => u.CreatedPublications)
               .HasForeignKey(p => p.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.AuthorUser)
                .WithMany(u => u.AuthoredAsUserPublications)
                .HasForeignKey(p => p.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.HasOne(p => p.AuthorCompany)
                .WithMany(c => c.AuthoredPublications)
                .HasForeignKey(p => p.AuthorCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
