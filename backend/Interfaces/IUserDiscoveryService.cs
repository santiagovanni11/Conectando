using Conectando.Api.DTOs.Users;

namespace Conectando.Api.Interfaces;

/// <summary>
/// Búsqueda de personas y sugerencias de amigos.
/// </summary>
public interface IUserDiscoveryService
{
    Task<List<UserCardDto>> SearchAsync(Guid currentUserId, string query, int limit, CancellationToken cancellationToken = default);
    Task<List<UserCardDto>> GetSuggestionsAsync(Guid currentUserId, int limit, CancellationToken cancellationToken = default);
}