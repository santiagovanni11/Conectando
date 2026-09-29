using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conectando.Api.Data.Configuration;

public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable("friendships", t => t
            .HasCheckConstraint("CK_friendships_normalized", "\"UserLowId\" < \"UserHighId\""));

        builder.HasKey(f => new { f.UserLowId, f.UserHighId });

        builder.Property(f => f.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(f => f.UserLowId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(f => f.UserHighId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(f => f.UserHighId);
    }
}