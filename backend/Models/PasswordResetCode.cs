namespace Conectando.Api.Models;

/// <summary>
/// Código de recuperación de contraseña.
///
/// Es de un solo uso y tiene vencimiento. El código nunca se guarda en
/// texto: lo que se guarda es su hash, como la contraseña. Si alguien lee
/// la base —una copia de seguridad, un dump, un acceso indebido— con los
/// hashes no puede resetear cuentas ajenas, igual que no puede entrar con
/// las contraseñas.
/// </summary>
public class PasswordResetCode
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>Hash del código entregado, nunca el código en claro.</summary>
    public string CodeHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Cuándo deja de servir. Un código de 6 dígitos son un millón de
    /// combinaciones: sin vencimiento, se adivina en un rato.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Cuándo se usó. Null mientras está vigente. Sirve una sola vez, aunque
    /// el usuario cambie la contraseña muchas veces.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Intentos fallidos. Al superar el tope el código queda inservible: así
    /// un atacante no puede quedarse probando hasta que le salga.
    /// </summary>
    public int Attempts { get; set; }

    public AppUser User { get; set; } = null!;
}
