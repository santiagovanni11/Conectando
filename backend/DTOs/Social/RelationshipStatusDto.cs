namespace Conectando.Api.DTOs.Social;

public class RelationshipStatusDto
{
    public Guid UserId { get; init; }
    public string Friendship { get; init; } = "none";
    public bool Following { get; init; }
    public bool FollowedBy { get; init; }
    public bool BlockedByMe { get; init; }
    public bool BlockedByThem { get; init; }
}