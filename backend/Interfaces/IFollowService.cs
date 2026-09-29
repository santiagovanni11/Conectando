using Conectando.Api.DTOs.Social;

namespace Conectando.Api.Interfaces;

public interface IFollowService
{
    Task FollowAsync(Guid userId, Guid targetUserId, CancellationToken cancellationToken = default);
    Task UnfollowAsync(Guid userId, Guid targetUserId, CancellationToken cancellationToken = default);
    Task<List<FollowDto>> GetFollowersAsync(Guid targetUserId, CancellationToken cancellationToken = default);
    Task<List<FollowDto>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken = default);
}