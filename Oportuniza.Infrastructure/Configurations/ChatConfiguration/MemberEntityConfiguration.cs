using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;
using Oportuniza.Domain.Models.ChatModel;

namespace Oportuniza.Infrastructure.Configurations.ChatConfiguration
{
    public class MemberEntityConfiguration : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            builder.ToTable("Members");
            builder.HasKey(member => member.Id);
            builder.Property(member => member.Id)
                   .ValueGeneratedOnAdd();
            builder.Property(member => member.UserId);
            builder.Property(member => member.RoomId);
            builder.Property(member => member.JoinedOn)
                   .HasDefaultValueSql("GetUtcDate()");
            builder.HasIndex(member => new { member.UserId, member.RoomId })
                   .IsUnique();
            builder.HasOne<User>(member => member.User)
                   .WithMany()
                   .HasForeignKey(member => member.UserId)
                   .OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<Room>()
                   .WithMany(room => room.Members)
                   .HasForeignKey(member => member.RoomId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
