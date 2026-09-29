using System.ComponentModel.DataAnnotations;

namespace Conectando.Api.DTOs.Account;

/// <summary>
/// Cambio de contraseña del propio usuario.
/// </summary>
/// <remarks>
/// `CurrentPassword` no es un detalle: sin ella, cualquier token robado
/// cambiaría la contraseña y se quedaría con la cuenta para siempre.
/// Repití la nueva contraseña para confirmar.
/// </remarks>
public class ChangePasswordRequest
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Repetí la nueva contraseña para confirmar.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
