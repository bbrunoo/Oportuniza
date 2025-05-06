using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

namespace Oportuniza.Infrastructure.Configurations
{
    public class ChatMessageEntityConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(cm => cm.Id);

            builder.Property(cm => cm.Id)
                .ValueGeneratedOnAdd();

            builder.Property(cm => cm.ChatId)
                .IsRequired();

            builder.Property(cm => cm.SenderId)
                .IsRequired();

            builder.Property(cm => cm.SenderName)
                .IsRequired();

            builder.Property(cm => cm.Message)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(cm => cm.SentAt)
                .IsRequired();
        }
    }
}
