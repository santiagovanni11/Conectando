namespace Conectando.Api.DTOs.Users;

/// <summary>
/// La foto de perfil y cómo se encuadra.
/// </summary>
/// <remarks>
/// Va aparte porque el encuadre lo necesitan todos los lugares donde aparece
/// un avatar —propio, ajeno, en comentarios, en conversaciones— y repetir los
/// cuatro campos en cada DTO es la forma más rápida de que se queden
/// desincronizados entre sí.
/// </remarks>
public abstract class ProfileImageDto
{
    public string? ProfileImageUrl { get; init; }

    /// <summary>Cuánto se acerca el recorte. 1 es sin acercar.</summary>
    public double ProfileImageZoom { get; init; } = 1;

    /// <summary>Desplazamiento horizontal del recorte, en píxeles de la original.</summary>
    public int ProfileImageOffsetX { get; init; }

    /// <summary>Desplazamiento vertical del recorte, en píxeles de la original.</summary>
    public int ProfileImageOffsetY { get; init; }
}
