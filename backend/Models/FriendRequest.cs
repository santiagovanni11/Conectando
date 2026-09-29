namespace Conectando.Api.Models;

public class FriendRequest
{
    public Guid RequesterId { get; set; }
    public Guid AddresseeId { get; set; }
    public DateTime CreatedAt { get; set; }
}