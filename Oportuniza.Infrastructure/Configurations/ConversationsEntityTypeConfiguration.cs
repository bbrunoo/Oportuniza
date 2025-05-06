using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class ConversationsEntityTypeConfiguration : IEntityTypeConfiguration<PrivateChat>
    {
        public void Configure(EntityTypeBuilder<PrivateChat> builder)
        {
            builder.HasKey(pc => pc.Id);

            builder.Property(pc => pc.User1Id)
                .IsRequired();

            builder.Property(pc => pc.User2Id)
                .IsRequired();

            builder.Property(pc => pc.CreatedAt)
                .IsRequired();
        }
    }
}
