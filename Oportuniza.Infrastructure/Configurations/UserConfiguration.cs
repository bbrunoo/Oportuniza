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

            //builder.Property(x => x.VerifiedEmail)
            //     .IsRequired();

            builder.Property(x => x.Email)
                   .HasMaxLength(150);

            builder.Property(x => x.Phone)
                   .HasMaxLength(20);

            builder.Property(x => x.ImageUrl)
                   .HasMaxLength(300);

            builder.Property(x => x.KeycloakId)
                .HasMaxLength(100);

            builder.Property(x => x.IsAdmin)
                   .HasDefaultValue(false);

            builder.Property(x => x.Active)
                   .HasDefaultValue(true);

            builder.Property(x => x.IsProfileCompleted)
                   .HasDefaultValue(false);

            builder.Property(x => x.UserType)
                   .IsRequired();

            builder.HasMany(u => u.CompanyLinks)
                   .WithOne(e => e.User)
                   .HasForeignKey(e => e.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
