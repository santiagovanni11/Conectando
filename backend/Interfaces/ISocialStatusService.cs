using Conectando.Api.DTOs.Social;

namespace Conectando.Api.Interfaces;

public interface ISocialStatusService
{
    Task<RelationshipStatusDto> GetStatusAsync(Guid userId, Guid targetUserId, CancellationToken cancellationToken = default);
}