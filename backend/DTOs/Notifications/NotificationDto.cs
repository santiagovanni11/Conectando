namespace Conectando.Api.DTOs.Notifications;

public class NotificationDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid? PostId { get; init; }
    public NotificationActorDto Actor { get; init; } = null!;
}

public class NotificationActorDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? ProfileImageUrl { get; init; }
}

public class NotificationPageDto
{
    public List<NotificationDto> Items { get; init; } = [];
    public string? NextCursor { get; init; }
    public bool HasMore { get; init; }
    public int UnreadCount { get; init; }
}

public class UnreadCountDto
{
    public int Count { get; init; }
}