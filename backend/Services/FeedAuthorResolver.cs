using Conectando.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Contexto de relaciones sociales usado por el feed: conjunto de autores
/// permitidos (propias ∪ amigos ∪ seguidos, sin bloqueados) y amigos del viewer.
/// </summary>
public record FeedAuthorContext(Guid ViewerId, HashSet<Guid> FriendIds, HashSet<Guid> AuthorIds);

/// <summary>
/// Calcula el conjunto de autores visibles para el feed. La deduplicación
/// (amigo + seguido, propio + relación) ocurre aquí, en memoria, mediante
/// HashSet: la consulta posterior nunca puede repetir autores ni posts.
/// </summary>
public class FeedAuthorResolver(PostVisibilityService visibility)
{
    public async Task<FeedAuthorContext> ResolveAsync(
        ConectandoDbContext dbContext, Guid viewerId, CancellationToken cancellationToken = default)
    {
        var friends = await visibility.GetFriendIdsAsync(viewerId, cancellationToken);
        var blocked = await visibility.GetBlockedIdsAsync(viewerId, cancellationToken);

        var following = await dbContext.Follows
            .AsNoTracking()
            .Where(f => f.UserId == viewerId)
            .Select(f => f.TargetUserId)
            .ToListAsync(cancellationToken);

        var authors = following.ToHashSet();
        authors.UnionWith(friends);
        authors.Add(viewerId);
        authors.ExceptWith(blocked);

        return new FeedAuthorContext(viewerId, friends, authors);
    }
}
