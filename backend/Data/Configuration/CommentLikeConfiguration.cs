using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conectando.Api.Data.Configuration;

public class CommentLikeConfiguration : IEntityTypeConfiguration<CommentLike>
{
    public void Configure(EntityTypeBuilder<CommentLike> builder)
    {
        builder.ToTable("comment_likes");

        // Una cuenta dada de baja no puede seguir dejando me gusta: si lo
        // hiciera, el número no bajaría nunca aunque se borrara el comentario.
        builder.HasQueryFilter(CuentasEliminadas.QuemPusoMeGustaEnComentarioVivo);

        builder.HasKey(l => new { l.CommentId, l.UserId });

        builder.Property(l => l.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne(l => l.Comment)
            .WithMany()
            .HasForeignKey(l => l.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // El índice es por comentario: es la forma en que se cuenta.
        builder.HasIndex(l => l.CommentId)
            .HasDatabaseName("IX_comment_likes_CommentId");
    }
}