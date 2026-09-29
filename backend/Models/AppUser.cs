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

    /// <summary>
    /// Encuadre del avatar, en.decimales de la imagen original.
    /// </summary>
    /// <remarks>
    /// No se recorta la imagen: se guarda dónde mirar y Cloudinary aplica el
    /// recorte al mostrarla. Así "ajustar" es instantáneo, se puede repetir
    /// las veces que haga falta sin volver a subir nada, y la foto original
    /// queda intacta por si un día se quiere mostrar entera.
    /// Zoom 1 es sin acercar; 2 es el doble de cerca.
    /// </remarks>
    public double ProfileImageZoom { get; set; } = 1;

    /// <summary>Desplazamiento horizontal del recorte, en píxeles de la original.</summary>
    public int ProfileImageOffsetX { get; set; }

    /// <summary>Desplazamiento vertical del recorte, en píxeles de la original.</summary>
    public int ProfileImageOffsetY { get; set; }

    /// <summary>Vuelve el avatar al encuadre de siempre, sin acercar y centrado.</summary>
    public void ResetAvatarFraming()
    {
        ProfileImageZoom = 1;
        ProfileImageOffsetX = 0;
        ProfileImageOffsetY = 0;
    }

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