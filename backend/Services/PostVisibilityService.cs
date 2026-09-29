using Conectando.Api.Data;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public class PostVisibilityService(ConectandoDbContext dbContext)
{
    public async Task<HashSet<Guid>> GetBlockedIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var ids = await dbContext.Blocks
            .AsNoTracking()
            .Where(b => b.UserId == userId || b.BlockedUserId == userId)
            .Select(b => b.UserId == userId ? b.BlockedUserId : b.UserId)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    /// <summary>
    /// Solo a quienes el usuario bloqueó, en una sola dirección.
    ///
    /// <see cref="GetBlockedIdsAsync"/> es simétrico y sirve para ocultar
    /// contenido. Para las listas de conexiones hace falta el lado exacto:
    /// si el bloqueado desapareciera también de su propia lista de amigos,
    /// el bloqueo se convertiría en un castigo público.
    /// </summary>
    public async Task<HashSet<Guid>> GetBlockedByMeIdsAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var ids = await dbContext.Blocks
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .Select(b => b.BlockedUserId)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    public async Task<HashSet<Guid>> GetFriendIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var ids = await dbContext.Friendships
            .AsNoTracking()
            .Where(f => f.UserLowId == userId || f.UserHighId == userId)
            .Select(f => f.UserLowId == userId ? f.UserHighId : f.UserLowId)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    public async Task<bool> IsVisibleAsync(Post post, Guid viewerId, CancellationToken cancellationToken = default)
    {
        if (post.AuthorId == viewerId)
        {
            return true;
        }

        if (await IsBlockedAsync(post.AuthorId, viewerId, cancellationToken))
        {
            return false;
        }

        // Una cuenta privada solo se ve si sos amigo suyo.
        if (await IsPrivateAccountAsync(post.AuthorId, cancellationToken)
            && !await AreFriendsAsync(post.AuthorId, viewerId, cancellationToken))
        {
            return false;
        }

        return post.Privacy switch
        {
            PostPrivacy.Public => true,
            PostPrivacy.Private => false,
            _ => await AreFriendsAsync(post.AuthorId, viewerId, cancellationToken),
        };
    }

    public Task<bool> IsPrivateAccountAsync(Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == userId && u.IsPrivate, cancellationToken);

    public async Task<bool> IsBlockedAsync(Guid userId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Blocks.AsNoTracking().AnyAsync(b =>
            (b.UserId == userId && b.BlockedUserId == targetUserId) ||
            (b.UserId == targetUserId && b.BlockedUserId == userId), cancellationToken);
    }

    public async Task<bool> AreFriendsAsync(Guid userId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        var (low, high) = SocialGuidPair.Normalize(userId, targetUserId);
        return await dbContext.Friendships.AsNoTracking().AnyAsync(f =>
            f.UserLowId == low && f.UserHighId == high, cancellationToken);
    }
}