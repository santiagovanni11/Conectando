namespace Conectando.Api.Interfaces;

/// <summary>
/// Un correo para enviar.
///
/// Va con cuerpo en texto y en HTML porque los dos se necesitan: el HTML
/// para que se vea bien, y el texto plano porque es el que leen los lectores
/// de pantalla y los clientes que no renderizan HTML. Un mail accesible es
/// el que trae las dos versiones.
/// </summary>
public record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string TextBody);

/// <summary>
/// Cómo manda la app sus correos.
///
/// Existe la interfaz para que el proveedor no se mezcle con el resto: hoy
/// habla SMTP con un relay, y mañana puede ser otro sin tocar ni una línea
/// del servicio de recuperación.
/// </summary>
public interface IEmailSender
{
    /// <summary>Manda un correo. Lanza si el proveedor falla.</summary>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Si está configurado. Cuando es false, la app funciona igual pero
    /// deja el contenido en el log en vez de mandarlo: sirve para poder
    /// probar el flujo completo sin depender de un servicio externo.
    /// </summary>
    bool IsConfigured { get; }
}
