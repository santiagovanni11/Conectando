using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conectando.Api.Data.Configuration;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports", t => t
            .HasCheckConstraint("CK_reports_no_self", "\"ReporterId\" <> \"TargetUserId\""));

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.Reason)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(r => r.Details)
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(r => r.TargetUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Un usuario noeden denunciar al mismo en bucle: se actualiza la
        // denuncia existente en vez de acumular filas idénticas.
        builder.HasIndex(r => new { r.ReporterId, r.TargetUserId })
            .IsUnique();
    }
}