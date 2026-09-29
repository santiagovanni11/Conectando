namespace Conectando.Api.DTOs.Messages;

public class ConversationDto
{
    public Guid Id { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int UnreadCount { get; init; }
    public List<ConversationPeerDto> Peers { get; init; } = [];
    public MessageDto? LastMessage { get; init; }

    /// <summary>El usuario silenció el chat: no llegan notificaciones.</summary>
    public bool IsMuted { get; init; }
}

public class ConversationPeerDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? ProfileImageUrl { get; init; }
    public bool IsOnline { get; init; }
}