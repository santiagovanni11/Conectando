using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conectando.Api.Data.Configuration;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public const int MaxContentLength = 2000;

    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");

        builder.HasQueryFilter(CuentasEliminadas.AutorVivo);

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Content)
            .HasMaxLength(MaxContentLength);

        builder.Property(p => p.Privacy)
            .HasConversion<int>();

        builder.HasIndex(p => new { p.AuthorId, p.CreatedAt })
            .HasDatabaseName("IX_posts_AuthorId_CreatedAt");

        builder.HasOne(p => p.Author)
            .WithMany()
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(p => p.Media)
            .WithOne(m => m.Post)
            .HasForeignKey(m => m.PostId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}