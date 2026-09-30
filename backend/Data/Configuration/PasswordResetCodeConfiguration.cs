using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conectando.Api.Data.Configuration;

public class PasswordResetCodeConfiguration : IEntityTypeConfiguration<PasswordResetCode>
{
    public void Configure(EntityTypeBuilder<PasswordResetCode> builder)
    {
        builder.ToTable("password_reset_codes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CodeHash)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.ExpiresAt).HasDefaultValueSql("now()");

        builder.Property(c => c.Attempts).HasDefaultValue(0);

        // Borrar la cuenta se lleva sus códigos. No tiene sentido que un
        // código siga siendo válido para alguien que ya no existe.
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        /*
         * Índice compuesto a propósito. El servicio siempre pregunta lo
         * mismo: "el código vigente de este usuario". Con el índice, esa
         * búsqueda no toca la tabla entera cuando la app acumuló códigos
         * viejos.
         */
        builder.HasIndex(c => new { c.UserId, c.ExpiresAt })
            .HasDatabaseName("IX_password_reset_codes_UserId_ExpiresAt");
    }
}
