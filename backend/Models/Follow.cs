namespace Conectando.Api.Models;

public class Follow
{
    public Guid UserId { get; set; }
    public Guid TargetUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}