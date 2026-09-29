using Conectando.Api.Data;
using Conectando.Api.DTOs.Users;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Listas de conexiones de un usuario. Si la cuenta es privada, solo
/// el propio usuario y sus amigos pueden verlas.
/// </summary>
public class UserConnectionsService(
    ConectandoDbContext dbContext,
    PostVisibilityService visibility) : IUserConnectionsService
{
    private const int MaxLimit = 200;

    public Task<List<UserCardDto>> GetFriendsAsync(Guid userId, Guid viewerId, CancellationToken cancellationToken = default) =>
        LoadAsync(userId, viewerId, cancellationToken, isFollowers: false);

    public Task<List<UserCardDto>> GetFollowersAsync(Guid userId, Guid viewerId, CancellationToken cancellationToken = default) =>
        LoadAsync(userId, viewerId, cancellationToken, isFollowers: true);

    public async Task<int> GetFollowsYouCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var ids = await FollowerIdsAsync(userId, cancellationToken);
        return ids.Count;
    }

    private async Task<List<UserCardDto>> LoadAsync(
        Guid userId,
        Guid viewerId,
        CancellationToken cancellationToken,
        bool isFollowers)
    {
        await EnsureVisibleAsync(userId, viewerId, cancellationToken);

        // Los bloqueos se sacan antes de armar las tarjetas. Se usa el lado
        // unidireccional: quien bloquea deja de ver al otro, pero el
        // bloqueado conserva su propia lista. Un filtro simétrico convertiría
        // el bloqueo en un castigo público.
        var blocked = await visibility.GetBlockedByMeIdsAsync(viewerId, cancellationToken);
        if (userId != viewerId) blocked.Add(userId);

        var ids = isFollowers
            ? await FollowerIdsAsync(userId, cancellationToken)
            : await FriendIdsAsync(userId, cancellationToken);

        var visible = ids.Where(id => !blocked.Contains(id)).ToList();

        if (visible.Count == 0)
        {
            return [];
        }

        var users = await dbContext.Users
            .AsNoTracking()
            .Where(u => visible.Contains(u.Id))
            .OrderBy(u => u.DisplayName)
            .Take(MaxLimit)
            .Select(u => new CandidateRow(u.Id, u.UserName, u.DisplayName, u.Bio, u.ProfileImageUrl))
            .ToListAsync(cancellationToken);

        var cards = await BuildCardsAsync(users, viewerId, cancellationToken);
        return [.. cards.Where(c => !isFollowers || c.Id != viewerId)];
    }

    private async Task EnsureVisibleAsync(Guid userId, Guid viewerId, CancellationToken cancellationToken)
    {
        if (userId == viewerId) return;

        if (await visibility.IsBlockedAsync(userId, viewerId, cancellationToken))
        {
            throw new BlockedActionException();
        }

        if (await visibility.IsPrivateAccountAsync(userId, cancellationToken)
            && !await visibility.AreFriendsAsync(userId, viewerId, cancellationToken))
        {
            throw new UserNotFoundException();
        }
    }

    private Task<List<Guid>> FriendIdsAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Friendships.AsNoTracking()
            .Where(f => f.UserLowId == userId || f.UserHighId == userId)
            .Select(f => f.UserLowId == userId ? f.UserHighId : f.UserLowId)
            .ToListAsync(cancellationToken);

    private Task<List<Guid>> FollowerIdsAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Follows.AsNoTracking()
            .Where(f => f.TargetUserId == userId)
            .Select(f => f.UserId)
            .ToListAsync(cancellationToken);

    /// <summary>Cruza los usuarios con su relación respecto de quien mira.</summary>
    private async Task<List<UserCardDto>> BuildCardsAsync(
        List<CandidateRow> users,
        Guid viewerId,
        CancellationToken cancellationToken)
    {
        var ids = users.Select(u => u.Id).ToList();
        var friendships = await dbContext.Friendships.AsNoTracking()
            .Where(f => ids.Contains(f.UserLowId) || ids.Contains(f.UserHighId))
            .Select(f => new FriendshipRow(f.UserLowId, f.UserHighId))
            .ToListAsync(cancellationToken);

        var requests = await dbContext.FriendRequests.AsNoTracking()
            .Where(r => ids.Contains(r.RequesterId) || ids.Contains(r.AddresseeId))
            .Select(r => new FriendRequestRow(r.RequesterId, r.AddresseeId))
            .ToListAsync(cancellationToken);

        return users
            .Select(u => UserConnectionMapper.Map(
                u,
                RelationResolver.Resolve(u.Id, viewerId, friendships, requests)))
            .ToList();
    }
}