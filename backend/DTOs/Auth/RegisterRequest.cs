using System.ComponentModel.DataAnnotations;

namespace Conectando.Api.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 30 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9_]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números y guión bajo.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [StringLength(255, ErrorMessage = "El email no puede superar los 255 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [MaxLength(64, ErrorMessage = "La contraseña no puede superar los 64 caracteres.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "La contraseña debe contener al menos una letra y un número.")]
    public string Password { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "El nombre visible no puede superar los 50 caracteres.")]
    public string? DisplayName { get; set; }
}