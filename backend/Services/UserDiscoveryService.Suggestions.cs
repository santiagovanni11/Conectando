using Conectando.Api.DTOs.Users;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public partial class UserDiscoveryService
{
    /// <summary>
    /// Cuántos candidatos se descargan antes de puntuar.
    /// Un pool acotado mantiene la consulta predecible; el ranking final
    /// lo hace RelationResolver, que sí sabe calcular la intersección.
    /// </summary>
    private const int CandidatePool = 60;

    /// <summary>
    /// Sugiere personas con las que no hay relación pendiente, priorizando
    /// quienes más amigos en común comparten con el usuario actual.
    /// </summary>
    public async Task<List<UserCardDto>> GetSuggestionsAsync(
        Guid currentUserId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(limit, 1, 50);
        var excluded = await ExcludedIdsAsync(currentUserId, cancellationToken);

        var candidates = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id != currentUserId && !excluded.Contains(u.Id))
            .OrderBy(u => u.UserName)
            .Take(CandidatePool)
            .Select(u => new CandidateRow(
                u.Id,
                u.UserName,
                u.DisplayName,
                u.Bio,
                u.ProfileImageUrl))
            .ToListAsync(cancellationToken);

        var cards = await DecorateAsync(currentUserId, candidates, candidates.Count, cancellationToken);

        return [.. cards
            .OrderByDescending(c => c.MutualFriendsCount)
            .ThenBy(c => c.DisplayName)
            .Take(take)];
    }
}