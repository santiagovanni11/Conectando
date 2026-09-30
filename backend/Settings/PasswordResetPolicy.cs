namespace Conectando.Api.Settings;

/// <summary>
/// Reglas de la recuperación de contraseña.
///
/// Viven acá y no en el servicio para que el frontend pueda mostrar "el
/// código vence en 15 minutos" leyendo el mismo número que usa el servidor,
/// en vez de uno copiado a mano que se desincroniza.
/// </summary>
public static class PasswordResetPolicy
{
    /// <summary>Cuánto vive un código desde que se pide.</summary>
    public static readonly TimeSpan Expiry = TimeSpan.FromMinutes(15);

    /// <summary>Intentos antes de que el código quede inservible.</summary>
    public const int MaxAttempts = 5;

    /// <summary>
    /// Espera mínima para volver a pedir un código. Sin esto, alguien puede
    /// pedir cien códigos por minuto y llenar el correo de la víctima hasta
    /// que rebalse, y de paso gastar la cuota de envío de la app.
    /// </summary>
    public static readonly TimeSpan ResendCooldown = TimeSpan.FromMinutes(1);

    /// <summary>Dígitos del código. 6 es el equilibrio habitual: corto para tipear, largo para no adivinarse.</summary>
    public const int CodeDigits = 6;

    /// <summary>
    /// Cuánto se guardan los códigos ya vencidos antes de borrarlos. Hay que
    /// dejar margen para que la espera entre pedir y mandar se mida contra
    /// algo real.
    /// </summary>
    public static readonly TimeSpan HistoryRetention = TimeSpan.FromDays(1);
}
