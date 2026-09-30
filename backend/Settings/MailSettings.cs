namespace Conectando.Api.Settings;

/// <summary>
/// Cómo se mandan los correos.
///
/// Viene de las variables de entorno con el prefijo `MailSettings__`, que es
/// el patrón que ya usa el proyecto para Jwt, Cloudinary y el resto: en
/// Render se cargan una vez en el panel y quedan cifradas, nunca en el
/// código.
///
/// Si <see cref="ApiKey"/> viene vacía, la app no está rota: entra el envío en
/// modo prueba y deja el contenido en el log. Así el flujo de recuperación se
/// puede recorrer entero sin depender de que el proveedor externo esté listo.
/// </summary>
public class MailSettings
{
    public const string SectionName = "MailSettings";

    /// <summary>
    /// Clave de API de Brevo.
    ///
    /// <para>
    /// Es la clave de la pestaña <c>SMTP &amp; API → API</c>, no la de SMTP.
    /// No es un detalle: Render corta el tráfico saliente hacia los puertos
    /// SMTP en los planes gratuitos, así que la clave de SMTP no sirve para
    /// nada ahí. La de API sale por el 443 y funciona.
    /// </para>
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Remitente, con formato "Nombre &lt;correo&gt;".</summary>
    public string From { get; set; } = string.Empty;

    /// <summary>Nombre sin correo, para el saludo del mensaje.</summary>
    public string BrandName { get; set; } = "Conectando";
}
