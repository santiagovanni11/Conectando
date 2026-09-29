using Conectando.Api.DTOs.Posts;

namespace Conectando.Api.Interfaces;

public interface IFeedService
{
    /// <summary>
    /// Página del feed principal: propias ∪ amigos ∪ seguidos, menos bloqueados,
    /// con keyset pagination (CreatedAt DESC, Id DESC).
    /// </summary>
    Task<PostListDto> GetPageAsync(Guid viewerId, string? cursor, int limit, CancellationToken cancellationToken = default);
}
