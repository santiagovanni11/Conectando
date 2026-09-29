using Conectando.Api.Data;
using Conectando.Api.DTOs.Users;
using Conectando.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Búsqueda por nombre o usuario, y sugerencias de amigos.
/// Devuelve siempre el estado de relación para que el front no tenga
/// que pedir un endpoint extra por cada persona.
/// </summary>
public partial class UserDiscoveryService(ConectandoDbContext dbContext) : IUserDiscoveryService
{
    private readonly ConectandoDbContext _dbContext = dbContext;

    public async Task<List<UserCardDto>> SearchAsync(Guid currentUserId, string query, int limit, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var term = query.Trim().ToLowerInvariant();
        var take = Math.Clamp(limit, 1, 50);

        var candidates = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id != currentUserId)
            .Where(u =>
                EF.Functions.Like(u.UserName.ToLower(), $"%{term}%") ||
                EF.Functions.Like(u.DisplayName.ToLower(), $"%{term}%"))
            .OrderBy(u => u.DisplayName)
            .Take(take * 2)
            .Select(u => new CandidateRow(
                u.Id,
                u.UserName,
                u.DisplayName,
                u.Bio,
                u.ProfileImageUrl))
            .ToListAsync(cancellationToken);

        // A quien bloqueaste no se lo ofrece como resultado de búsqueda: si
        // apareciera, el bloqueo sería solo decorativo.
        var blocked = await _dbContext.Blocks
            .AsNoTracking()
            .Where(b => b.UserId == currentUserId || b.BlockedUserId == currentUserId)
            .Select(b => b.UserId == currentUserId ? b.BlockedUserId : b.UserId)
            .ToListAsync(cancellationToken);

        var hidden = blocked.ToHashSet();
        candidates.RemoveAll(c => hidden.Contains(c.Id));

        return await DecorateAsync(currentUserId, candidates, take, cancellationToken);
    }

    /// <summary>Ids que no deben aparecer como sugerencias: amigos, bloqueos y solicitudes abiertas.</summary>
    private async Task<List<Guid>> ExcludedIdsAsync(Guid currentUserId, CancellationToken cancellationToken)
    {
        var me = currentUserId;
        var pair = await _dbContext.Friendships
            .AsNoTracking()
            .Where(f => f.UserLowId == me)
            .Select(f => f.UserHighId)
            .Union(_dbContext.Friendships.AsNoTracking().Where(f => f.UserHighId == me).Select(f => f.UserLowId))
            .ToListAsync(cancellationToken);

        var requests = await _dbContext.FriendRequests
            .AsNoTracking()
            .Where(r => r.RequesterId == me || r.AddresseeId == me)
            .Select(r => r.RequesterId == me ? r.AddresseeId : r.RequesterId)
            .ToListAsync(cancellationToken);

        var blocked = await _dbContext.Blocks
            .AsNoTracking()
            .Where(b => b.UserId == me)
            .Select(b => b.BlockedUserId)
            .Union(_dbContext.Blocks.AsNoTracking().Where(b => b.BlockedUserId == me).Select(b => b.UserId))
            .ToListAsync(cancellationToken);

        return [.. pair.Union(requests).Union(blocked)];
    }

    /// <summary>
    /// Cruza los candidatos con su estado de relación y los ordena como el front los necesita.
    /// </summary>
    private async Task<List<UserCardDto>> DecorateAsync(
        Guid currentUserId,
        List<CandidateRow> candidates,
        int take,
        CancellationToken cancellationToken)
    {
        if (candidates.Count == 0)
        {
            return [];
        }

        var me = currentUserId;
        var ids = candidates.Select(c => c.Id).ToList();

        // Incluye las amistades del propio usuario: sin ellas no se pueden
        // calcular los amigos en común con cada candidato.
        var friendships = await _dbContext.Friendships.AsNoTracking()
            .Where(f => ids.Contains(f.UserLowId) || ids.Contains(f.UserHighId)
                        || f.UserLowId == me || f.UserHighId == me)
            .Select(f => new FriendshipRow(f.UserLowId, f.UserHighId))
            .ToListAsync(cancellationToken);

        var requests = await _dbContext.FriendRequests.AsNoTracking()
            .Where(r => ids.Contains(r.RequesterId) || ids.Contains(r.AddresseeId))
            .Select(r => new FriendRequestRow(r.RequesterId, r.AddresseeId))
            .ToListAsync(cancellationToken);

        var relations = candidates.ToDictionary(
            c => c.Id,
            c => RelationResolver.Resolve(c.Id, me, friendships, requests));

        return candidates
            .Take(take)
            .Select(c => Map(c, relations[c.Id]))
            .ToList();
    }

    private static UserCardDto Map(CandidateRow row, CandidateRelations relations) => new()
    {
        Id = row.Id,
        UserName = row.UserName,
        DisplayName = row.DisplayName,
        Bio = row.Bio,
        ProfileImageUrl = row.ProfileImageUrl,
        Friendship = RelationResolver.FriendshipLabel(relations),
        FriendsCount = relations.FriendsCount,
        MutualFriendsCount = relations.MutualFriendsCount,
    };
}