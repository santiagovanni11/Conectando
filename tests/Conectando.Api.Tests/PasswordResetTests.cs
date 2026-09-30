using Conectando.Api.Data;
using Conectando.Api.Services;
using Conectando.Api.Settings;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

/// <summary>
/// Pedir un código de recuperación.
///
/// El foco no es que el flujo ande, sino que no se pueda usar para.
/// </summary>
[Collection("Social")]
public class PasswordResetTests(SocialTestFixture fixture)
{
    private static PasswordResetService Crear(ConectandoDbContext db, FakeEmailSender email) =>
        new(db, email, new MailSettings { BrandName = "Conectando" });

    /// <summary>Saca el código del correo, que es donde queda escrito.</summary>
    private static string CodigoEnviado(FakeEmailSender email)
    {
        var linea = email.Last!.TextBody.Split('\n')
            .First(l => l.Contains("recuperar la contraseña es"));
        return string.Concat(linea.Where(char.IsDigit));
    }

    [Fact]
    public async Task Pide_Codigo_Y_Llega_Al_Correo()
    {
        await using var db = fixture.CreateContext();
        var email = new FakeEmailSender();
        var service = Crear(db, email);
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await service.RequestCodeAsync(user.Email);

        Assert.Single(email.Sent);
        Assert.Equal(user.Email, email.Last!.To);
        Assert.Equal(6, CodigoEnviado(email).Length);
    }

    [Fact]
    public async Task El_Codigo_No_Se_Guarda_En_Texto()
    {
        // Si alguien lee la base —una copia, un dump— no tiene que poder
        // resetear cuentas ajenas con lo que encuentre.
        await using var db = fixture.CreateContext();
        var email = new FakeEmailSender();
        var service = Crear(db, email);
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await service.RequestCodeAsync(user.Email);
        var codigo = CodigoEnviado(email);

        // Se busca por usuario y no con SingleAsync a secas: el esquema lo
        // comparten todos los tests de la colección, así que hay códigos de
        // otros casos dando vueltas.
        var guardado = await db.PasswordResetCodes.SingleAsync(c => c.UserId == user.Id);

        // No está en texto, pero sí verifica lo que se envió: si no
        // verificara, el flujo no andaría.
        Assert.NotEqual(codigo, guardado.CodeHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(codigo, guardado.CodeHash));
    }

    [Fact]
    public async Task No_Dice_Si_El_Correo_Existe()
    {
        // La regla que más importa. Si un correo desconocido hiciera algo
        // distinto, cualquiera podría recorrer la base probando direcciones y
        // armarse una lista de quién usa la app.
        await using var db = fixture.CreateContext();
        var email = new FakeEmailSender();
        var service = Crear(db, email);

        var antes = await db.PasswordResetCodes.CountAsync();

        await service.RequestCodeAsync("nadie@ejemplo.com");

        Assert.Empty(email.Sent);

        // Se compara con la cuenta previa y no con cero: el esquema lo
        // comparten los tests de la colección.
        Assert.Equal(antes, await db.PasswordResetCodes.CountAsync());
    }

    [Fact]
    public async Task Encuentra_El_Correo_Escrito_Como_Vincula()
    {
        // El correo no distingue mayúsculas: si se trataran distinto, el
        // usuario escribiría su dirección y no le llegaría nada, sin
        // explicación.
        await using var db = fixture.CreateContext();
        var email = new FakeEmailSender();
        var service = Crear(db, email);
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await service.RequestCodeAsync(user.Email.ToUpperInvariant());

        Assert.Single(email.Sent);
    }

    [Fact]
    public async Task No_Manda_Dos_Codigos_Seguidos()
    {
        // El cooldown evita que alguien inunde el correo de otra persona
        // pidiendo códigos en cadena.
        await using var db = fixture.CreateContext();
        var email = new FakeEmailSender();
        var service = Crear(db, email);
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await service.RequestCodeAsync(user.Email);
        await service.RequestCodeAsync(user.Email);

        Assert.Single(email.Sent);
    }

    [Fact]
    public async Task El_Correo_Llega_Todavia_Con_Mayusculas_O_Espacios()
    {
        // La gente copia y pega, y a veces arrastra un espacio sin verlo.
        await using var db = fixture.CreateContext();
        var email = new FakeEmailSender();
        var service = Crear(db, email);
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await service.RequestCodeAsync($"  {user.Email}  ");

        Assert.Equal(user.Email, email.Last!.To);
    }
}
