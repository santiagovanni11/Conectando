using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Conectando.Api.RateLimiting;

/// <summary>Nombre de las políticas para poder referenciarlas desde los endpoints.</summary>
public static class RateLimitPolicies
{
    public const string Messages = "messages";
    public const string Writes = "writes";
    public const string Reads = "reads";
    public const string Login = "login";
    public const string Uploads = "uploads";
    public const string PasswordReset = "password-reset";
}

/// <summary>
/// Límites de peticiones por usuario.
///
/// Se usa el limitador del framework, así que no hace falta ningún paquete
/// extra. La idea es frenear automatismos y abuse (mandar 500 mensajes por
/// segundo) sin molestar a una persona normal: los números son altos para
/// el uso legítimo y bajos frente a un abuso.
/// </summary>
public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        return services.AddRateLimiter(options =>
        {
            // Si se supera el límite, se corta con 429 en vez de esperar.
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(
                RateLimitPolicies.Messages,
                http => FixedWindow(http, 30, TimeSpan.FromSeconds(10)));

            options.AddPolicy(
                RateLimitPolicies.Writes,
                http => FixedWindow(http, 60, TimeSpan.FromMinutes(1)));

            options.AddPolicy(
                RateLimitPolicies.Reads,
                http => FixedWindow(http, 300, TimeSpan.FromMinutes(1)));

            // Subir archivos es de lo más caro de la API: cada request mueve
            // hasta 8 MB por archivo y además se le paga a Cloudinary. Por eso
            // va aparte de Writes y no comparte su límite: un usuario que
            // publica diez posts en un minuto no debería poder subir cien
            // fotos en el mismo minuto.
            options.AddPolicy(
                RateLimitPolicies.Uploads,
                http => FixedWindow(http, 20, TimeSpan.FromMinutes(1)));

            // El login es el endpoint más expuesto: sin límite de tentativas
            // se puede usar para adivinar contraseñas.
            options.AddFixedWindowLimiter(RateLimitPolicies.Login, limiter =>
            {
                limiter.PermitLimit = 10;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueLimit = 0;
            });

            /*
             * Recuperación de contraseña: cinco pedidos cada diez minutos.
             *
             * El límite es del servidor y no solo de la base. El cooldown por
             * usuario evita el inundar un correo ajeno, pero sin esto, un
             * atacante con un listado de correos gastaría la cuota de envío
             * de la app entera en un rato y los códigos de los demás dejarían
             * de llegar. Quien se olvidó la clave tres veces en un minuto es
             * alguien, no un abuso.
             */
            options.AddPolicy(
                RateLimitPolicies.PasswordReset,
                http => FixedWindow(http, 5, TimeSpan.FromMinutes(10)));

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "10";
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new { message = "Demasiadas peticiones. Probá de nuevo en unos segundos." },
                    token);
            };
        });
    }

    /// <summary>
    /// Ventana fija con una cuota por usuario. Los que no estén
    /// autenticados comparten cuota: es lo correcto para login y evita que
    /// un atacante abra un hueco por cada IP falsa.
    /// </summary>
    private static RateLimitPartition<string> FixedWindow(
        HttpContext httpContext,
        int permitLimit,
        TimeSpan window) =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0,
            });
}