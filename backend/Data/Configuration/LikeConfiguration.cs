using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conectando.Api.Data.Configuration;

public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.ToTable("likes");

        builder.HasKey(l => new { l.UserId, l.PostId });

        builder.Property(l => l.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(l => l.Post)
            .WithMany()
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => new { l.PostId, l.UserId })
            .HasDatabaseName("IX_likes_PostId_UserId");

        builder.HasIndex(l => new { l.PostId, l.CreatedAt, l.UserId })
            .HasDatabaseName("IX_likes_PostId_CreatedAt_UserId");
    }
}