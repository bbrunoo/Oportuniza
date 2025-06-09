using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

            builder.Property(p => p.Expired)
                .IsRequired();

            builder.Property(p => p.AuthorId)
                .IsRequired();

            builder.Property(p => p.AuthorType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

        }
    }

}
