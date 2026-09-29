namespace Conectando.Api.Models;

public class Block
{
    public Guid UserId { get; set; }
    public Guid BlockedUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}