using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conectando.Api.Data.Configuration;

public class BlockConfiguration : IEntityTypeConfiguration<Block>
{
    public void Configure(EntityTypeBuilder<Block> builder)
    {
        builder.ToTable("blocks", t => t
            .HasCheckConstraint("CK_blocks_no_self", "\"UserId\" <> \"BlockedUserId\""));

        builder.HasKey(b => new { b.UserId, b.BlockedUserId });

        builder.Property(b => b.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(b => b.BlockedUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(b => b.BlockedUserId);
    }
}