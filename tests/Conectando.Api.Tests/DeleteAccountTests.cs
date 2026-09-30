using Conectando.Api.Data;
using Conectando.Api.DTOs.Account;
using Conectando.Api.Exceptions;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

/// <summary>
/// Baja de cuenta.
/// </summary>
/// <remarks>
/// El caso que más importa es el último: comprueba que el mensaje del otro
/// sigue existiendo. Borrar la fila entera reventaría por clave foránea, y
/// cambiar las relaciones a cascada se llevaría el contenido ajeno.
/// </remarks>
[Collection("Social")]
public class DeleteAccountTests(SocialTestFixture fixture)
{
    private static AccountService CreateService(ConectandoDbContext db) => new(db, new FakeMediaCleaner());

    private static DeleteAccountRequest Request(
        string current = TestUsers.ValidPassword,
        string? confirm = null) => new()
        {
            CurrentPassword = current,
            ConfirmPassword = confirm ?? current,
        };

    [Fact]
    public async Task ConPasswordValida_AnonimizaLaCuenta()
    {
        await using var db = fixture.CreateContext();
        var user = (await TestUsers.SeedAsync(db, 1))[0];
        user.Bio = "Algo personal";
        user.ProfileImageUrl = "https://ejemplo.com/avatar.png";
        await db.SaveChangesAsync();

        await CreateService(db).DeleteAccountAsync(user.Id, Request());

        var saved = await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        Assert.NotNull(saved.DeletedAt);
        Assert.Equal("Usuario eliminado", saved.DisplayName);
        Assert.Null(saved.Bio);
        Assert.Null(saved.ProfileImageUrl);
        Assert.DoesNotContain("Algo personal", saved.Email);
    }

    [Fact]
    public async Task ConPasswordValida_BorraLaFormaDeEntrar()
    {
        await using var db = fixture.CreateContext();
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await CreateService(db).DeleteAccountAsync(user.Id, Request());

        var saved = await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        Assert.False(BCrypt.Net.BCrypt.Verify(TestUsers.ValidPassword, saved.PasswordHash));
    }

    [Fact]
    public async Task ConPasswordIncorrecta_NoAnonimizaNada()
    {
        await using var db = fixture.CreateContext();
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await Assert.ThrowsAsync<CurrentPasswordIncorrectException>(() =>
            CreateService(db).DeleteAccountAsync(user.Id, Request(current: "NoEsLaMia9")));

        var saved = await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        Assert.Null(saved.DeletedAt);
    }

    [Fact]
    public async Task ConLaConfirmacionDistinta_NoAnonimizaNada()
    {
        // La segunda casilla es la que evita el borrado sin querer.
        await using var db = fixture.CreateContext();
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await Assert.ThrowsAsync<PasswordConfirmationMismatchException>(() =>
            CreateService(db).DeleteAccountAsync(
                user.Id,
                Request(confirm: "OtraClave9")));

        var saved = await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        Assert.Null(saved.DeletedAt);
    }

    [Fact]
    public async Task Anonimizada_UsaUnEmailUnicoPorCuenta()
    {
        // Si compartieran "eliminado", la segunda baja no se podría guardar.
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var service = CreateService(db);

        await service.DeleteAccountAsync(users[0].Id, Request());
        await service.DeleteAccountAsync(users[1].Id, Request());

        // La consulta se acota a estas dos cuentas: la base es compartida y
        // puede haber bajas de otros tests.
        var ids = users.Select(u => u.Id).ToList();
        var saved = await db.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToListAsync();

        Assert.Equal(2, saved.Count);
        Assert.All(saved, u => Assert.NotNull(u.DeletedAt));
        Assert.Equal(2, saved.Select(u => u.Email).Distinct().Count());
        Assert.Equal(2, saved.Select(u => u.UserName).Distinct().Count());
    }

    [Fact]
    public async Task Anonimizada_DejaIntactoElMensajeDelOtro()
    {
        // El motivo de no borrar la fila: el chat del otro no puede romperse.
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[1].Id, "Hola, ¿seguimos?");

        await CreateService(db).DeleteAccountAsync(users[0].Id, Request());

        // Acotado a esta conversación: la base la comparten todos los tests.
        var mensajes = await db.Messages
            .AsNoTracking()
            .CountAsync(m => m.ConversationId == conversation.Id);

        Assert.Equal(1, mensajes);
    }

    [Fact]
    public async Task DosVeces_SoloAnonimizaLaPrimera()
    {
        await using var db = fixture.CreateContext();
        var user = (await TestUsers.SeedAsync(db, 1))[0];
        var service = CreateService(db);
        await service.DeleteAccountAsync(user.Id, Request());
        var baja = await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            service.DeleteAccountAsync(user.Id, Request()));

        Assert.Equal(baja.DeletedAt, (await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id)).DeletedAt);
    }
}
