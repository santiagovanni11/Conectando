using Conectando.Api.DTOs.Social;

namespace Conectando.Api.Interfaces;

public interface IFriendshipService
{
    Task<List<FriendDto>> GetFriendsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RemoveFriendAsync(Guid userId, Guid friendUserId, CancellationToken cancellationToken = default);
}