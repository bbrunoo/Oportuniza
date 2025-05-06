using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models.ChatModel;

namespace Oportuniza.Infrastructure.Configurations.ChatConfiguration
{
    public class RoomEntityConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.ToTable("Room");
            builder.HasKey(room => room.Id);
            builder.Property(room => room.Id)
                   .ValueGeneratedOnAdd();
            builder.Property(room => room.CreatedOn)
                   .HasDefaultValueSql("GetUtcDate()");
        }
    }
}
