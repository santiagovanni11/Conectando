using System.ComponentModel.DataAnnotations;

namespace Conectando.Api.DTOs.Account;

/// <summary>
/// Baja de la cuenta del propio usuario.
/// </summary>
/// <remarks>
/// Pide la contraseña dos veces a propósito. Una sola deja pasar el clic
/// accidental y el relleno automático: con dos, quien tome un teclado ajeno
/// tiene que conocer la contraseña de verdad. El backend compara ambas y con
/// la actual, así que la comprobación no vive solo en la pantalla.
/// </remarks>
public class DeleteAccountRequest
{
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Repetí la contraseña para confirmar.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
