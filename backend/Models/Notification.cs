namespace Conectando.Api.Models;

public class Notification
{
    public Guid Id { get; set; }
    public Guid RecipientId { get; set; }
    public Guid ActorId { get; set; }
    public NotificationType Type { get; set; }
    public Guid? PostId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public AppUser Recipient { get; set; } = null!;
    public AppUser Actor { get; set; } = null!;
    public Post? Post { get; set; }
}

public enum NotificationType
{
    Like = 1,
    Comment = 2,
    FriendRequest = 3,
    FriendAccepted = 4,
    Follow = 5,
}