namespace Conectando.Api.DTOs.Social;

public class BlockDto
{
    public UserSummaryDto User { get; init; } = null!;
    public DateTime BlockedAt { get; init; }
}