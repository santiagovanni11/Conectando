namespace Conectando.Api.DTOs.Social;

public class FollowDto
{
    public UserSummaryDto User { get; init; } = null!;
    public DateTime Since { get; init; }
}