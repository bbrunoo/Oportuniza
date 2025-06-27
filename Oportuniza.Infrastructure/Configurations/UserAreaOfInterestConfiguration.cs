using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class UserAreaOfInterestConfiguration : IEntityTypeConfiguration<UserAreaOfInterest>
    {
        public void Configure(EntityTypeBuilder<UserAreaOfInterest> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.User)
                   .WithMany(u => u.UserAreasOfInterest)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AreaOfInterest)
                   .WithMany()
                   .HasForeignKey(x => x.AreaOfInterestId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
