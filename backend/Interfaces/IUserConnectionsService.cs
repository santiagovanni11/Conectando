using Conectando.Api.DTOs.Users;

namespace Conectando.Api.Interfaces;

/// <summary>
/// Listas de conexiones de un usuario: amigos y seguidores.
/// </summary>
public interface IUserConnectionsService
{
    Task<List<UserCardDto>> GetFriendsAsync(Guid userId, Guid viewerId, CancellationToken cancellationToken = default);
    Task<List<UserCardDto>> GetFollowersAsync(Guid userId, Guid viewerId, CancellationToken cancellationToken = default);
    Task<int> GetFollowsYouCountAsync(Guid userId, CancellationToken cancellationToken = default);
}