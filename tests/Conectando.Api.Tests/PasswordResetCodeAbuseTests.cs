using Conectando.Api.Data;
using Conectando.Api.DTOs.Account;
using Conectando.Api.Exceptions;
using Conectando.Api.Services;
using Conectando.Api.Settings;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

/// <summary>
/// Las defensas del código: que no sirva vencido, ni después de un solo uso,
/// ni después de que alguien lo haya～」probado muchas veces.
///
/// Va aparte del camino feliz porque responde a una pregunta distinta: no
/// "¿funciona?", sino "¿aguanta que lo ataquen?".
/// </summary>
[Collection("Social")]
public class PasswordResetCodeAbuseTests(SocialTestFixture fixture)
{
    private static PasswordResetService Crear(ConectandoDbContext db, FakeEmailSender email) =>
        new(db, email, new MailSettings { BrandName = "Conectando" });

    private static ConfirmPasswordResetRequest Pedir(string email, string code) => new()
    {
        Email = email,
        Code = code,
        NewPassword = "NuevaClave9",
        ConfirmPassword = "NuevaClave9",
    };

    private static async Task<string> Emitir(
        PasswordResetService service,
        FakeEmailSender mail,
        string correo)
    {
        await service.RequestCodeAsync(correo);
        var linea = mail.Last!.TextBody.Split('\n')
            .First(l => l.Contains("recuperar la contraseña es"));
        return string.Concat(linea.Where(char.IsDigit));
    }

    [Fact]
    public async Task Un_Codigo_Vencido_No_Sirve()
    {
        await using var db = fixture.CreateContext();
        var mail = new FakeEmailSender();
        var service = Crear(db, mail);
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        var codigo = await Emitir(service, mail, user.Email);

        var guardado = await db.PasswordResetCodes.SingleAsync(c => c.UserId == user.Id);
        guardado.ExpiresAt = DateTime.UtcNow.AddSeconds(-1);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidResetCodeException>(
            () => service.ConfirmAsync(Pedir(user.Email, codigo)));
    }

    [Fact]
    public async Task Agotados_Los_Intentos_El_Codigo_Se_Quema()
    {
        // Seis números de un millón: sin tope, se prueba hasta que sale.
        await using var db = fixture.CreateContext();
        var mail = new FakeEmailSender();
        var service = Crear(db, mail);
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        var codigo = await Emitir(service, mail, user.Email);

        for (var i = 0; i < PasswordResetPolicy.MaxAttempts; i++)
        {
            await Assert.ThrowsAsync<InvalidResetCodeException>(
                () => service.ConfirmAsync(Pedir(user.Email, "000000")));
        }

        // Agotados los intentos, tampoco sirve el código correcto.
        await Assert.ThrowsAsync<InvalidResetCodeException>(
            () => service.ConfirmAsync(Pedir(user.Email, codigo)));
    }

    [Fact]
    public async Task Cada_Intento_Fallido_Se_Cuenta()
    {
        // Sin contar, el tope de intentos no existiría: se podría seguir
        // probando contra el mismo código hasta que saliera.
        await using var db = fixture.CreateContext();
        var mail = new FakeEmailSender();
        var service = Crear(db, mail);
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await Emitir(service, mail, user.Email);
        await Assert.ThrowsAsync<InvalidResetCodeException>(
            () => service.ConfirmAsync(Pedir(user.Email, "000000")));

        var guardado = await db.PasswordResetCodes.SingleAsync(c => c.UserId == user.Id);
        Assert.Equal(1, guardado.Attempts);
    }

    [Fact]
    public async Task Un_Codigo_Erroneo_Y_Una_Cuenta_Inexistente_Dicen_Lo_Mismo()
    {
        // Si los dos errores se distinguieran, el segundo confirmaría que esa
        // casilla está registrada. Con el mismo texto, no hay nada que leer.
        await using var db = fixture.CreateContext();
        var mail = new FakeEmailSender();
        var service = Crear(db, mail);
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        var codigo = await Emitir(service, mail, user.Email);

        var conCodigoMalo = await Assert.ThrowsAsync<InvalidResetCodeException>(
            () => service.ConfirmAsync(Pedir(user.Email, "000000")));
        var conMailInventado = await Assert.ThrowsAsync<InvalidResetCodeException>(
            () => service.ConfirmAsync(Pedir("nadie@ejemplo.com", codigo)));

        Assert.Equal(conCodigoMalo.Message, conMailInventado.Message);
    }
}
