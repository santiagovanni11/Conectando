using System.Text.Json;

namespace Conectando.Api.Services;

/// <summary>
/// Lectura de lo que devuelve Brevo.
///
/// Va aparte porque es puro manejo de texto —JSON de la respuesta, formato del
/// remitente— y no el envío. Mezclarlo en el remitente lo dejaría con dos
/// responsabilidades y bastante más difícil de leer de un vistazo.
/// </summary>
public sealed partial class BrevoEmailSender
{
    /// <summary>
    /// Saca el messageId de la respuesta.
    ///
    /// <para>
    /// Si el cuerpo no viene o no es el esperado devuelve "sin id" en vez de
    /// tirar. El correo ya fue aceptado: que no se pueda anotar el
    /// identificador no es motivo para romper algo que salió bien.
    /// </para>
    /// </summary>
    private static async Task<string> LeerMessageId(
        HttpResponseMessage respuesta,
        CancellationToken cancellationToken)
    {
        try
        {
            var cuerpo = await respuesta.Content.ReadAsStringAsync(cancellationToken);

            return JsonDocument.Parse(cuerpo)
                .RootElement
                .TryGetProperty("messageId", out var id)
                ? id.GetString() ?? "sin id"
                : "sin id";
        }
        catch (Exception)
        {
            return "sin id";
        }
    }

    /// <summary>
    /// Parte el "From" de la configuración, que viene como "Nombre &lt;correo&gt;".
    /// Si no trae los ángulos se usa tal cual: hay gente que pega solo la
    /// dirección y romper ahí sería tirar abajo el envío por un detalle de
    /// formato.
    /// </summary>
    private (string Nombre, string Correo) SepararRemitente()
    {
        var from = _settings.From;
        var abre = from.IndexOf('<');
        var cierra = from.IndexOf('>');

        if (abre < 0 || cierra < abre)
        {
            return (from, from);
        }

        return (from[..abre].Trim(), from[(abre + 1)..cierra].Trim());
    }

    /// <summary>Recorta el detalle: puede traer HTML largo y no sirve de más.</summary>
    private static string Recortar(string detalle) =>
        detalle.Length <= 400 ? detalle : detalle[..400];
}