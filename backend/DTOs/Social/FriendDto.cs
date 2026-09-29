namespace Conectando.Api.DTOs.Social;

public class FriendDto
{
    public UserSummaryDto User { get; init; } = null!;
    public DateTime Since { get; init; }
}