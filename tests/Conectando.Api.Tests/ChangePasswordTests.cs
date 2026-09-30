using Conectando.Api.Data;
using Conectando.Api.DTOs.Account;
using Conectando.Api.Exceptions;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

/// <summary>
/// Cambio de contraseña.
/// </summary>
[Collection("Social")]
public class ChangePasswordTests(SocialTestFixture fixture)
{
    private static AccountService CreateService(ConectandoDbContext db) => new(db, new FakeMediaCleaner());

    private static ChangePasswordRequest Request(
        string current = TestUsers.ValidPassword,
        string next = "NuevaClave9",
        string? confirm = null) => new()
        {
            CurrentPassword = current,
            NewPassword = next,
            ConfirmPassword = confirm ?? next,
        };

    [Fact]
    public async Task ConPasswordValida_GuardaLaNueva()
    {
        await using var db = fixture.CreateContext();
        var user = (await TestUsers.SeedAsync(db, 1))[0];
        var service = CreateService(db);

        await service.ChangePasswordAsync(user.Id, Request());

        var saved = await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        Assert.True(BCrypt.Net.BCrypt.Verify("NuevaClave9", saved.PasswordHash));
        Assert.False(BCrypt.Net.BCrypt.Verify(TestUsers.ValidPassword, saved.PasswordHash));
    }

    [Fact]
    public async Task ConPasswordValida_RotaElSelloParaCerrarLasOtrasSesiones()
    {
        // Sin esto, cambiar la contraseña no cerraría nada: los tokens viejos
        // seguirían valiendo hasta vencerse.
        await using var db = fixture.CreateContext();
        var user = (await TestUsers.SeedAsync(db, 1))[0];
        var before = user.SecurityStamp;

        await CreateService(db).ChangePasswordAsync(user.Id, Request());

        var saved = await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        Assert.NotEqual(before, saved.SecurityStamp);
    }

    [Fact]
    public async Task ConPasswordIncorrecta_NoCambiaNada()
    {
        await using var db = fixture.CreateContext();
        var user = (await TestUsers.SeedAsync(db, 1))[0];
        var stampBefore = user.SecurityStamp;

        var error = await Assert.ThrowsAsync<CurrentPasswordIncorrectException>(() =>
            CreateService(db).ChangePasswordAsync(user.Id, Request(current: "NoEsLaMia9")));

        Assert.Equal(401, error.StatusCode);

        var saved = await db.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
        Assert.True(BCrypt.Net.BCrypt.Verify(TestUsers.ValidPassword, saved.PasswordHash));
        Assert.Equal(stampBefore, saved.SecurityStamp);
    }

    [Theory]
    [InlineData("corta1")]        // menos de 8
    [InlineData("Sololetras")]    // sin número
    [InlineData("123456789")]     // sin letra
    public async Task ConPasswordDebil_LaRechaza(string debil)
    {
        await using var db = fixture.CreateContext();
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await Assert.ThrowsAsync<WeakPasswordException>(() =>
            CreateService(db).ChangePasswordAsync(user.Id, Request(next: debil)));
    }

    [Fact]
    public async Task ConLaConfirmacionDistinta_NoCambiaNada()
    {
        await using var db = fixture.CreateContext();
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await Assert.ThrowsAsync<PasswordConfirmationMismatchException>(() =>
            CreateService(db).ChangePasswordAsync(
                user.Id,
                Request(next: "NuevaClave9", confirm: "OtraClave9")));
    }

    [Fact]
    public async Task ConLaMismaPassword_NoLaDejaRepetir()
    {
        await using var db = fixture.CreateContext();
        var user = (await TestUsers.SeedAsync(db, 1))[0];

        await Assert.ThrowsAsync<SamePasswordException>(() =>
            CreateService(db).ChangePasswordAsync(
                user.Id,
                Request(next: TestUsers.ValidPassword)));
    }

    [Fact]
    public async Task EnUnaCuentaBaja_NoDejaCambiarLaPassword()
    {
        await using var db = fixture.CreateContext();
        var user = (await TestUsers.SeedAsync(db, 1))[0];
        var service = CreateService(db);
        await service.DeleteAccountAsync(
            user.Id,
            new DeleteAccountRequest
            {
                CurrentPassword = TestUsers.ValidPassword,
                ConfirmPassword = TestUsers.ValidPassword,
            });

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            service.ChangePasswordAsync(user.Id, Request()));
    }
}
