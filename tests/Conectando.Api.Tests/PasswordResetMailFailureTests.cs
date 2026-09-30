using Conectando.Api.Data;
using Conectando.Api.Interfaces;
using Conectando.Api.Services;
using Conectando.Api.Settings;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Conectando.Api.Tests;

/// <summary>Remitente que siempre falla, como uno con la clave mal puesta.</summary>
public sealed class EmailQueFalla : IEmailSender
{
    public bool IsConfigured => true;

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("SMTP 535: credenciales rechazadas");
}

/// <summary>
/// Qué pasa cuando el proveedor de correo no está.
/// </summary>
[Collection("Social")]
public class PasswordResetMailFailureTests(SocialTestFixture fixture)
{
    private static PasswordResetService Crear(ConectandoDbContext db, IEmailSender email) =>
        new(db, email, new MailSettings { BrandName = "Conectando" }, NullLogger<PasswordResetService>.Instance);

    [Fact]
    public async Task Un_Correo_Roto_No_Rompe_La_Respuesta()
    {
        // Regresión de producción: con una clave mal puesta en el panel,
        // pedir el código devolvía 500 y el usuario veía "Algo salió mal".
        // El código ya estaba guardado: la operación se había hecho, solo
        // falló el aviso.
        await using var db = fixture.CreateContext();
        var service = Crear(db, new EmailQueFalla());
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await service.RequestCodeAsync(user.Email);

        // Y el código quedó guardado, así que el usuario puede pedirlo de
        // nuevo en un minuto y it'll funcionar cuando el proveedor vuelva.
        Assert.Single(await db.PasswordResetCodes
            .Where(c => c.UserId == user.Id)
            .ToListAsync());
    }

    [Fact]
    public async Task Un_Correo_Roto_No_Revela_Que_La_Cuenta_Existe()
    {
        // Lo más grave de dejar escapar la excepción: con el envío caído, un
        // correo registrado daba 500 y uno inexistente daba 202. Con solo
        // mirar el status se armaba la lista de quién tiene cuenta, que es
        // justo lo que el resto del método evita.
        await using var db = fixture.CreateContext();
        var service = Crear(db, new EmailQueFalla());
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await service.RequestCodeAsync(user.Email);
        await service.RequestCodeAsync("nadie@ejemplo.com");

        // Ninguno de los dos debió lanzar: los dos responden lo mismo.
    }
}
