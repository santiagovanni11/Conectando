namespace Conectando.Api.DTOs.Social;

public class FriendRequestDto
{
    public UserSummaryDto User { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}