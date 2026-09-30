using Conectando.Api.Data;
using Conectando.Api.DTOs.Account;
using Conectando.Api.Exceptions;
using Conectando.Api.Services;
using Conectando.Api.Settings;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

/// <summary>
/// Canje del código: cambiar la contraseña de verdad y cerrarle las puertas
/// al que no debería.
/// </summary>
[Collection("Social")]
public class PasswordResetConfirmTests(SocialTestFixture fixture)
{
    /// <summary>Pide un código para ese correo y lo saca del mensaje, como haría el usuario.</summary>
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

    private static PasswordResetService Crear(ConectandoDbContext db, FakeEmailSender email) =>
        new(db, email, new MailSettings { BrandName = "Conectando" });

    private static ConfirmPasswordResetRequest Pedir(string email, string code) => new()
    {
        Email = email,
        Code = code,
        NewPassword = "NuevaClave9",
        ConfirmPassword = "NuevaClave9",
    };

    [Fact]
    public async Task El_Codigo_Correcto_Cambia_La_Contrasena()
    {
        await using var db = fixture.CreateContext();
        var mail = new FakeEmailSender();
        var service = Crear(db, mail);
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        var codigo = await Emitir(service, mail, user.Email);
        await service.ConfirmAsync(Pedir(user.Email, codigo));

        var guardado = await db.Users.SingleAsync(u => u.Id == user.Id);
        Assert.True(BCrypt.Net.BCrypt.Verify("NuevaClave9", guardado.PasswordHash));
    }

    [Fact]
    public async Task Cambiar_La_Contrasena_Cierra_Las_Sesiones_Abiertas()
    {
        // Lo que hace SecurityStamp: si alguien tenía la sesión iniciada en
        // otro dispositivo, deja de servir en el momento del cambio y no
        // cuando caduque el token.
        await using var db = fixture.CreateContext();
        var mail = new FakeEmailSender();
        var service = Crear(db, mail);
        var user = (await TestUsers.SeedAsync(db, 1))[0];
        var selloAntes = user.SecurityStamp;

        var codigo = await Emitir(service, mail, user.Email);
        await service.ConfirmAsync(Pedir(user.Email, codigo));

        var guardado = await db.Users.SingleAsync(u => u.Id == user.Id);
        Assert.NotEqual(selloAntes, guardado.SecurityStamp);
    }

    [Fact]
    public async Task El_Codigo_Sirve_Una_Sola_Vez()
    {
        // Si no, un código que quedó en un historial de correo permitiría
        // cambiar la contraseña muchas veces.
        await using var db = fixture.CreateContext();
        var mail = new FakeEmailSender();
        var service = Crear(db, mail);
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        var codigo = await Emitir(service, mail, user.Email);
        await service.ConfirmAsync(Pedir(user.Email, codigo));

        await Assert.ThrowsAsync<InvalidResetCodeException>(
            () => service.ConfirmAsync(Pedir(user.Email, codigo)));
    }
}
