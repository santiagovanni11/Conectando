namespace Conectando.Api.DTOs.Social;

public class UserSummaryDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? ProfileImageUrl { get; init; }
}