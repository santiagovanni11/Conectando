namespace Conectando.Api.Models;

public class Friendship
{
    public Guid UserLowId { get; set; }
    public Guid UserHighId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Igual que en Follow: hacen falta para el filtro global que saca la
    // amistad cuando una de las dos cuentas ya no existe.
    public AppUser UserLow { get; set; } = null!;
    public AppUser UserHigh { get; set; } = null!;
}