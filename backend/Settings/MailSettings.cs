namespace Conectando.Api.Settings;

/// <summary>
/// Cómo se mandan los correos.
///
/// Viene de las variables de entorno con el prefijo `MailSettings__`, que es
/// el patrón que ya usa el proyecto para Jwt, Cloudinary y el resto: en
/// Render se cargan una vez en el panel y quedan cifradas, nunca en el
/// código.
///
/// Si <see cref="Host"/> viene vacía, la app no está rota: entra el envío en
/// modo prueba y deja el contenido en el log. Así el flujo de recuperación se
/// puede recorrer entero sin depender de que el proveedor externo esté listo.
/// </summary>
public class MailSettings
{
    public const string SectionName = "MailSettings";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>Remitente, con formato "Nombre &lt;correo&gt;".</summary>
    public string From { get; set; } = string.Empty;

    /// <summary>Nombre sin correo, para el saludo del mensaje.</summary>
    public string BrandName { get; set; } = "Conectando";

    /// <summary>
    /// Si está completo para mandar. Se verifican las cuatro cosas: con una
    /// sola faltando el envío fallaría recién al primer uso, en producción,
    /// que es la peor forma de enterarse.
    /// </summary>
    public bool CanSend =>
        !string.IsNullOrWhiteSpace(Host) &&
        Port > 0 &&
        !string.IsNullOrWhiteSpace(User) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(From);
}
