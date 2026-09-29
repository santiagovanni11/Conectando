using Conectando.Api.DTOs.Social;

namespace Conectando.Api.Interfaces;

public interface IBlockService
{
    Task<List<BlockDto>> GetBlocksAsync(Guid userId, CancellationToken cancellationToken = default);
    Task BlockAsync(Guid userId, Guid targetUserId, CancellationToken cancellationToken = default);
    Task UnblockAsync(Guid userId, Guid targetUserId, CancellationToken cancellationToken = default);
    Task<bool> IsBlockedAsync(Guid userId, Guid targetUserId, CancellationToken cancellationToken = default);
}