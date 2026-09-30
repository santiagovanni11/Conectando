using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Conectando.Api.Settings;

namespace Conectando.Api.Services;

/// <summary>
/// Arma el correo con el código de recuperación.
///
/// Va aparte del servicio porque es una pieza de presentación, no de
/// negocio: acá no hay ninguna regla de seguridad, solo cómo se ve el
/// mensaje. Si mañana cambia el diseño, se toca este archivo y no la lógica
/// que decide a quién se le manda.
///
/// El código va en mayúsculas y con espacios de a cuatro: se lee mucho mejor
/// anotado que escrito, y al tipear el espacio se ignora.
/// </summary>
public partial class PasswordResetService
{
    private const int MinutosDeVigencia = 15;

    /// <summary>
    /// Pistas para el usuario, escritas una sola vez.
    ///
    /// <para>
    /// Van en una lista y no repetidas en cada versión del mensaje a propósito:
    /// el correo sale dos veces, en HTML y en texto plano, y copiar la misma
    /// frase en los dos lugares es la forma segura de que se desincronicen.
    /// Con la lista, agregar una pista es agregar una línea y las dos
    /// versiones la muestran.
    /// </para>
    ///
    /// <para>
    /// La del spam está porque el correo sale desde una casilla de consumo y
    /// los filtros lo mandan a spam seguido. Avisarlo evita el peor escenario
    /// posible: que la persona pida el código otra vez, espere un correo que
    /// ya llegó, y siga sin verlo.
    /// </para>
    /// </summary>
    private static readonly string[] Pistas =
    [
        "Si no lo encontrás, revisá la carpeta de spam o promociones: a veces el correo llega ahí.",
    ];

    private EmailMessage ConstruirMensaje(string destinatario, string codigo) =>
        ConstruirMensaje(destinatario, codigo, _email.IsConfigured);

    private EmailMessage ConstruirMensaje(string destinatario, string codigo, bool enviado) => new(
        To: destinatario,
        Subject: $"{Marca} · Tu código para recuperar la contraseña",
        HtmlBody: CuerpoHtml(codigo, enviado),
        TextBody: CuerpoTexto(codigo, enviado));

    private string Marca => _marca;

    private string CuerpoHtml(string codigo, bool enviado) => $"""
        <div style="font-family:system-ui,-apple-system,'Segoe UI',sans-serif;max-width:34rem;margin:0 auto;padding:1.5rem;color:#1f2933">
          <h1 style="font-size:1.25rem;margin:0 0 1rem">{_marca}</h1>
          <p>Tu código para recuperar la contraseña es:</p>
          <p style="font-size:2rem;font-weight:700;letter-spacing:.35rem;background:#f4f6f8;padding:1rem;border-radius:.5rem;text-align:center;margin:1.5rem 0">
            {Formatear(codigo)}
          </p>
          <p>Vence en {MinutosDeVigencia} minutos y sirve una sola vez.</p>
          {AvisoSinEnvio(enviado)}
          {PistasHtml()}
          <p style="color:#5b6770;font-size:.875rem;margin-top:2rem">
            Si no pediste esto, no hagas nada: nadie puede cambiar tu contraseña
            con este correo solo.
          </p>
        </div>
        """;

    private string CuerpoTexto(string codigo, bool enviado) => $"""
        {_marca}

        Tu código para recuperar la contraseña es: {Formatear(codigo)}

        Vence en {MinutosDeVigencia} minutos y sirve una sola vez.
        {(enviado ? string.Empty : "ATENCIÓN: el envío no está configurado, este código se está mostrando solo en el servidor.")}
        {PistasTexto()}

        Si no pediste esto, no hagas nada: nadie puede cambiar tu contraseña con este correo solo.
        """;

    /// <summary>Las pistas para la versión con formato.</summary>
    private static string PistasHtml() =>
        $"<p style=\"color:#5b6770;font-size:.875rem\">{string.Join(" ", Pistas)}</p>";

    /// <summary>Las mismas pistas para la versión de texto plano.</summary>
    private static string PistasTexto() => string.Join(Environment.NewLine, Pistas);

    /// <summary>
    /// Aviso solo cuando el correo no llegó. Es una red de seguridad: si el
    /// envío falla por una razón que no depende de la app, el usuario al menos
    /// tiene una pista de por qué.
    /// </summary>
    private static string AvisoSinEnvio(bool enviado) => enviado
        ? string.Empty
        : "<p style=\"color:#b45309;background:#fef3c7;padding:.75rem;border-radius:.5rem\">El envío no está configurado en el servidor. Tu código aparece acá, pero no te va a llegar por correo.</p>";

    /// <summary>
    /// Agrupa el código de a cuatro para que se lea mejor anotado.
    ///
    /// El último grupo puede ser más corto: con 6 dígitos son "1234 56". Con
    /// división entera se perdía la cola —"1234"— y el usuario recibía un
    /// código que nunca iba a validar. Un test lo detectó antes de que
    /// llegara a producción.
    /// </summary>
    private static string Formatear(string codigo) => string.Join(' ',
        Enumerable.Range(0, (codigo.Length + 3) / 4)
            .Select(i => codigo.Substring(i * 4, Math.Min(4, codigo.Length - i * 4))));
}
