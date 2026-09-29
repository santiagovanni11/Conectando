using Conectando.Api.Data;
using Conectando.Api.DTOs.Users;
using Conectando.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Conteos de un perfil, ya descontando lo que el usuario que mira no
/// debería ver.
///
/// Va aparte de <see cref="UserProfileService"/> porque necesita saber
/// QUIÉN mira: el mismo perfil puede mostrar 10 seguidores para una persona
/// y 8 para otra que bloqueó a dos de ellos. Calcularlo dentro de la
/// proyección del perfil obligaría a traer la lista de bloqueados por cada
/// conteo y a repetir la misma resta en varios lugares.
/// </summary>
public class ProfileCountService(ConectandoDbContext dbContext) : IProfileCountService
{
    private readonly ConectandoDbContext _dbContext = dbContext;

    public async Task<ProfileCountsDto> GetCountsAsync(
        Guid userId, Guid viewerId, CancellationToken cancellationToken = default)
    {
        // Solo el lado que el viewer bloqueó. Si también se descontara a
        // quien lo bloqueó a él, el número dependería de una acción que
        // este usuario no hizo.
        var hidden = await _dbContext.Blocks
            .AsNoTracking()
            .Where(b => b.UserId == viewerId)
            .Select(b => b.BlockedUserId)
            .ToListAsync(cancellationToken);

        var blocked = hidden.ToHashSet();

        var friendIds = await _dbContext.Friendships
            .AsNoTracking()
            .Where(f => f.UserLowId == userId || f.UserHighId == userId)
            .Select(f => f.UserLowId == userId ? f.UserHighId : f.UserLowId)
            .ToListAsync(cancellationToken);

        var followerIds = await _dbContext.Follows
            .AsNoTracking()
            .Where(f => f.TargetUserId == userId)
            .Select(f => f.UserId)
            .ToListAsync(cancellationToken);

        // Los posts no se filtran: se ocultan por privacidad, no por
        // bloqueo, y el autor nunca está en su propia lista de bloqueados.
        var posts = await _dbContext.Posts.CountAsync(p => p.AuthorId == userId, cancellationToken);

        return new ProfileCountsDto
        {
            PostsCount = posts,
            FriendsCount = friendIds.Count(id => !blocked.Contains(id)),
            FollowersCount = followerIds.Count(id => !blocked.Contains(id)),
        };
    }
}