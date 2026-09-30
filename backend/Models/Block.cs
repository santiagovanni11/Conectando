namespace Conectando.Api.Models;

public class Block
{
    public Guid UserId { get; set; }
    public Guid BlockedUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Para el filtro global: si la cuenta bloqueada se dio de baja, el bloqueo
    // deja de tener sentido y no debe seguir restando en ningún conteo.
    public AppUser User { get; set; } = null!;
    public AppUser BlockedUser { get; set; } = null!;
}