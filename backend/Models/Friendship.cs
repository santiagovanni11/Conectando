namespace Conectando.Api.Models;

public class Friendship
{
    public Guid UserLowId { get; set; }
    public Guid UserHighId { get; set; }
    public DateTime CreatedAt { get; set; }
}