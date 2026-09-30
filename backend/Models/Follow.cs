namespace Conectando.Api.Models;

public class Follow
{
    public Guid UserId { get; set; }
    public Guid TargetUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Las dos puntas del vínculo. Están para poder decir "ninguna de las dos
    // cuentas está dada de baja" en el filtro global; sin ellas la condición
    // no se puede escribir y la cuenta eliminada se cuela en la lista.
    public AppUser User { get; set; } = null!;
    public AppUser TargetUser { get; set; } = null!;
}