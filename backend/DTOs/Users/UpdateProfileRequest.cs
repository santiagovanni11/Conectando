using System.ComponentModel.DataAnnotations;

namespace Conectando.Api.DTOs.Users;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "El nombre visible es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre visible debe tener entre 2 y 50 caracteres.")]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(160, ErrorMessage = "La biografía no puede superar los 160 caracteres.")]
    public string? Bio { get; set; }

    // La foto ya no se tipea: se sube con POST /api/users/me/profile-image.
    // Solo se conserva el largo máximo para no persistir basura.
    [StringLength(2048, ErrorMessage = "La URL de la foto no puede superar los 2048 caracteres.")]
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Encuadre del avatar. Viene null cuando el usuario no lo tocó, y en ese
    /// caso no se modifica: así guardar el nombre o la biografía no pisa por
    /// accidente el ajuste que el usuario hizo antes.
    /// </summary>
    [Range(1, 4, ErrorMessage = "El acercamiento de la foto va entre 1 y 4.")]
    public double? ProfileImageZoom { get; set; }

    [Range(-4000, 4000, ErrorMessage = "El desplazamiento horizontal es demasiado grande.")]
    public int? ProfileImageOffsetX { get; set; }

    [Range(-4000, 4000, ErrorMessage = "El desplazamiento vertical es demasiado grande.")]
    public int? ProfileImageOffsetY { get; set; }

    /// <summary>Cuenta privada: solo los amigos ven publicaciones y listas.</summary>
    public bool IsPrivate { get; set; }
}