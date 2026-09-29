namespace Conectando.Api.Models;

public class AppUser
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Identificador del avatar en Cloudinary. Se guardaba solo la URL, y con
    /// solo la URL no se puede borrar el archivo anterior al cambiar el
    /// avatar: cada cambio dejaba una imagen huérfana pagando por siempre.
    /// Con el publicId se puede limpiar.
    /// </summary>
    public string? ProfileImagePublicId { get; set; }

    /// <summary> Peso del avatar, para que entre en la cuenta de espacio.</summary>
    public long ProfileImageSizeBytes { get; set; }
    /// <summary>Si es true, solo sus amigos ven sus publicaciones y sus listas.</summary>
    public bool IsPrivate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Sello que viaja en el token. Cambiar la contraseña lo rota y con eso
    /// mueren los tokens viejos: sin esto, cambiar la contraseña no cerraría
    /// las demás sesiones y quien haya copiado un token seguiría entrando.
    /// </summary>
    public Guid SecurityStamp { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Cuándo se eliminó la cuenta. La fila no se borra: mensajes y comentarios
    /// la apuntan, y si desapareciera, el contenido ajeno quedaría huérfano o
    /// se iría por cascada. Se anonimiza en su lugar.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}