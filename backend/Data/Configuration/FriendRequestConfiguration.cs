using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conectando.Api.Data.Configuration;

public class FriendRequestConfiguration : IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        builder.ToTable("friend_requests", t => t
            .HasCheckConstraint("CK_friend_requests_no_self", "\"RequesterId\" <> \"AddresseeId\""));

        builder.HasQueryFilter(CuentasEliminadas.SolicitudEntreVivas);

        builder.HasKey(r => new { r.RequesterId, r.AddresseeId });

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne(r => r.Requester)
            .WithMany()
            .HasForeignKey(r => r.RequesterId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.Addressee)
            .WithMany()
            .HasForeignKey(r => r.AddresseeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(r => r.AddresseeId);
    }
}