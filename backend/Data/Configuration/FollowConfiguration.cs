using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conectando.Api.Data.Configuration;

public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.ToTable("follows", t => t
            .HasCheckConstraint("CK_follows_no_self", "\"UserId\" <> \"TargetUserId\""));

        builder.HasKey(f => new { f.UserId, f.TargetUserId });

        builder.Property(f => f.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(f => f.TargetUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(f => f.TargetUserId);
    }
}