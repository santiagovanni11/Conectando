namespace Conectando.Api.Models;

public class FriendRequest
{
    public Guid RequesterId { get; set; }
    public Guid AddresseeId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Para el filtro global: una solicitud de alguien que ya no existe no
    // puede seguir esperando respuesta en la bandeja del otro.
    public AppUser Requester { get; set; } = null!;
    public AppUser Addressee { get; set; } = null!;
}