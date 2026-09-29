using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conectando.Api.Data.Configuration;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Bio)
            .HasMaxLength(160);

        builder.Property(u => u.ProfileImageUrl)
            .HasMaxLength(2048);

        builder.Property(u => u.ProfileImagePublicId)
            .HasMaxLength(256);

        builder.Property(u => u.ProfileImageSizeBytes)
            .HasDefaultValue(0L);

        builder.Property(u => u.ProfileImageZoom)
            .HasDefaultValue(1.0);

        builder.Property(u => u.CreatedAt);

        builder.Property(u => u.UpdatedAt);
    }
}