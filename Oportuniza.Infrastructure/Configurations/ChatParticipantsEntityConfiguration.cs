using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class ChatParticipantsEntityConfiguration : IEntityTypeConfiguration<ChatParticipant>
    {
        public void Configure(EntityTypeBuilder<ChatParticipant> builder)
        {
            builder.HasKey(cp => cp.Id);

            builder.Property(cp => cp.Id)
                .ValueGeneratedOnAdd();

            builder.Property(cp => cp.ChatId)
                .IsRequired();

            builder.Property(cp => cp.UserId)
                .IsRequired();

            builder.Property(cp => cp.UserName)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(cp => cp.ChatId);
            builder.HasIndex(cp => cp.UserId);
        }
    }
}
