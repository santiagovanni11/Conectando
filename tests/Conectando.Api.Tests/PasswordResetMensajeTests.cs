using Conectando.Api.Data;
using Conectando.Api.Interfaces;
using Conectando.Api.Services;
using Conectando.Api.Settings;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace Conectando.Api.Tests;

/// <summary>
/// El texto del correo de recuperación.
///
/// Va en un archivo propio porque es una pieza de presentación, y el resto de
/// las pruebas del flujo están ocupadas con lo que de verdad importa: quién
/// recibe el código y si se puede reutilizar.
///
/// <para>
/// El foco es que la versión con formato y la de texto plano no se separen.
/// Las dos se arman por su cuenta, y si a una de las dos se le olvida algo —
/// la pista del spam, el vencimiento— la mitad de los lectores queda sin
/// información. Estos tests se apoyan en la lista compartida que usan ambas:
/// si mañana se agrega una pista, aparece en las dos versiones, y estos
/// tests lo confirman.
/// </para>
/// </summary>
[Collection("Social")]
public class PasswordResetMensajeTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Las_dos_versiones_dicen_que_revisen_el_spam()
    {
        // El correo sale desde una casilla de consumo y los filtros lo tiran a
        // spam seguido. Sin esta frase, la gente pide el código otra vez
        // esperando algo que ya llegó.
        var mensaje = await ConstruirMensaje();

        Assert.Contains("spam", mensaje.HtmlBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("spam", mensaje.TextBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task El_codigo_va_en_las_dos_versiones()
    {
        // El código sale aleatorio, así que no se puede escribir el esperado:
        // se saca del propio mensaje y se busca en las dos versiones.
        var mensaje = await ConstruirMensaje();
        var codigo = CodigoDelMensaje(mensaje);

        Assert.Contains(codigo, mensaje.HtmlBody);
        Assert.Contains(codigo, mensaje.TextBody);
    }

    [Fact]
    public async Task El_codigo_va_agrupado_de_a_cuatro()
    {
        // "1234 56" y no "123456": se lee mucho mejor anotado, y el espacio se
        // ignora al tipearlo. Además descarta el error de truncar el último
        // grupo, que entregaba un código que nunca iba a validar.
        var mensaje = await ConstruirMensaje();
        var digitos = string.Concat(CodigoDelMensaje(mensaje).Where(char.IsDigit));

        Assert.Contains($" {digitos[4..]}", mensaje.TextBody);
        Assert.Equal(6, digitos.Length);
    }

    [Fact]
    public async Task Las_dos_versiones_dicen_cuanto_vence()
    {
        // Si el texto plano no lo dice, quien lo lee desde un lector de
        // pantalla no tiene forma de saber si el código sigue sirviendo.
        var mensaje = await ConstruirMensaje();

        Assert.Contains("15 minutos", mensaje.HtmlBody);
        Assert.Contains("15 minutos", mensaje.TextBody);
    }

    [Fact]
    public async Task El_correo_no_aclara_que_se_pedir_otro_codigo()
    {
        // El aviso del spam sirve solo si no se infiere que el código está mal.
        // Invite a pedir otro, y quedaría una ambigüedad sin resolver.
        var mensaje = await ConstruirMensaje();

        Assert.DoesNotContain("solicita otro", mensaje.HtmlBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("solicita otro", mensaje.TextBody, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Saca el código del texto del correo, que es donde queda escrito.
    ///
    /// Devuelve el código como aparece, con el espacio de por medio: así los
    /// tests pueden afirmar sobre el formato y no solo sobre los dígitos.
    /// </summary>
    private static string CodigoDelMensaje(EmailMessage mensaje) =>
        mensaje.TextBody
            .Split('\n')
            .First(l => l.Contains("recuperar la contraseña es"))
            .Split(':', 2)[1]
            .Trim();

    private async Task<EmailMessage> ConstruirMensaje()
    {
        await using var db = fixture.CreateContext();
        var email = new FakeEmailSender();
        var service = new PasswordResetService(
            db,
            email,
            new MailSettings { BrandName = "Conectando" },
            NullLogger<PasswordResetService>.Instance);
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await service.RequestCodeAsync(user.Email);

        return email.Last!;
    }
}