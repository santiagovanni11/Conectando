using Conectando.Api.Data;
using Conectando.Api.Models;
using Conectando.Api.Services;

namespace Conectando.Api.Tests.TestInfrastructure;

/// <summary>
/// Helpers para sembrar vínculos sociales en los tests. Existen porque
/// `friendships` guarda los ids normalizados y `follows` es direccional: sin
/// estos helpers cada test reimplementaría la misma normalización.
/// </summary>
public static class SocialLinkTestData
{
    /// <summary>Crea una amistad. Los ids se normalizan como en producción.</summary>
    public static async Task SeedFriendshipAsync(ConectandoDbContext db, Guid a, Guid b)
    {
        // Se usa el mismo helper que el servicio: si el orden difiere de la
        // comparación, la fila no se encuentra y el conteo da 0.
        var (low, high) = SocialGuidPair.Normalize(a, b);
        db.Friendships.Add(new Friendship { UserLowId = low, UserHighId = high, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }

    /// <summary>Crea un follow: <paramref name="follower"/> sigue a <paramref name="target"/>.</summary>
    public static async Task SeedFollowAsync(ConectandoDbContext db, Guid follower, Guid target)
    {
        db.Follows.Add(new Follow { UserId = follower, TargetUserId = target, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }
}