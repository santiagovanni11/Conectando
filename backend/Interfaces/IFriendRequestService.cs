using Conectando.Api.DTOs.Social;

namespace Conectando.Api.Interfaces;

public interface IFriendRequestService
{
    Task SendAsync(Guid requesterId, Guid addresseeId, CancellationToken cancellationToken = default);
    Task AcceptAsync(Guid actorId, Guid requesterId, CancellationToken cancellationToken = default);
    Task RejectAsync(Guid actorId, Guid requesterId, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid requesterId, Guid addresseeId, CancellationToken cancellationToken = default);
    Task<List<FriendRequestDto>> GetReceivedAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<FriendRequestDto>> GetSentAsync(Guid userId, CancellationToken cancellationToken = default);
}