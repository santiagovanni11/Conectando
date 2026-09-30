using System.ComponentModel.DataAnnotations;

namespace Conectando.Api.DTOs.Account;

/// <summary>
/// Paso 1: pedir un código de recuperación.
///
/// No devuelve nada sobre la cuenta. El endpoint responde siempre igual para
/// que no se pueda averiguar qué correos están registrados.
/// </summary>
public class RequestPasswordResetRequest
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ese correo no parece válido.")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Paso 2: canjear el código por una contraseña nueva.
///
/// La contraseña va dos veces por lo mismo que en el cambio desde el perfil:
/// el relleno automático del navegador la completa una vez, y con una sola
/// casilla el clic equivocado deja la cuenta con otra clave.
/// </summary>
public class ConfirmPasswordResetRequest
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ese correo no parece válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código es obligatorio.")]
    [RegularExpression(@"^\d{4,10}$", ErrorMessage = "El código son solo números.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Repetí la nueva contraseña para confirmar.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
