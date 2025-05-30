using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.FullName)
                   .HasMaxLength(150);

            builder.Property(x => x.Email)
                   .HasMaxLength(150);

            builder.Property(x => x.Phone)
                   .HasMaxLength(20);

            builder.Property(x => x.ImageUrl)
                   .HasMaxLength(300);
        }
    }

}
