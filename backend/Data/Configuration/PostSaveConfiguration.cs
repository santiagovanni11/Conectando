using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conectando.Api.Data.Configuration;

public class PostSaveConfiguration : IEntityTypeConfiguration<PostSave>
{
    public void Configure(EntityTypeBuilder<PostSave> builder)
    {
        builder.ToTable("post_saves");

        // Clave propia en vez de compuesta: con (UserId, PostId) como PK, EF
        // Core genera un alias "PostId1" al proyectar o filtrar, y Postgres
        // lo rechaza. Un Id propio evita ese problema y deja el índice único
        // como garantía de no duplicar.
        builder.HasQueryFilter(CuentasEliminadas.QuienGuardoVivo);

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Post)
            .WithMany()
            .HasForeignKey(s => s.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Un post no se guarda dos veces por la misma persona.
        builder.HasIndex(s => new { s.UserId, s.PostId })
            .IsUnique();

        // El índice por fecha sirve para listar lo guardado de más nuevo a
        // más viejo; el único arranca por UserId, que es lo que se filtra.
        builder.HasIndex(s => new { s.UserId, s.CreatedAt });
    }
}