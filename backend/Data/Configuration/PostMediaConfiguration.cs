using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conectando.Api.Data.Configuration;

public class PostMediaConfiguration : IEntityTypeConfiguration<PostMedia>
{
    public void Configure(EntityTypeBuilder<PostMedia> builder)
    {
        builder.ToTable("post_media");

        builder.HasQueryFilter(CuentasEliminadas.DueñoVivo);

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Url)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(m => m.PublicId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(m => m.State)
            .HasConversion<int>();

        builder.Property(m => m.SizeBytes)
            .HasDefaultValue(0L);

        builder.Property(m => m.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(m => new { m.PostId, m.DisplayOrder })
            .HasDatabaseName("IX_post_media_PostId_DisplayOrder");

        builder.HasIndex(m => new { m.UserId, m.State })
            .HasDatabaseName("IX_post_media_UserId_State");

        builder.HasOne(m => m.Post)
            .WithMany(p => p.Media)
            .HasForeignKey(m => m.PostId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}