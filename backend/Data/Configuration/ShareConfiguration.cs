using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conectando.Api.Data.Configuration;

public class ShareConfiguration : IEntityTypeConfiguration<Share>
{
    public void Configure(EntityTypeBuilder<Share> builder)
    {
        builder.ToTable("shares");

        builder.HasQueryFilter(CuentasEliminadas.QuienCompartioVivo);

        builder.HasKey(s => new { s.UserId, s.PostId });

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(s => s.Post)
            .WithMany()
            .HasForeignKey(s => s.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.PostId, s.CreatedAt, s.UserId })
            .HasDatabaseName("IX_shares_PostId_CreatedAt_UserId");

        builder.HasIndex(s => new { s.PostId, s.UserId })
            .HasDatabaseName("IX_shares_PostId_UserId");
    }
}