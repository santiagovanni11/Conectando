namespace Conectando.Api.Models;

public class Share
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public DateTime CreatedAt { get; set; }

    public AppUser User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}