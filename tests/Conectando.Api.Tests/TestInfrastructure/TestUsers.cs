using Conectando.Api.Data;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests.TestInfrastructure;

public static class TestUsers
{
    /// <summary>Contraseña que cumple la política: larga, con letra y número.</summary>
    public const string ValidPassword = "ClaveSegura1";

    public static AppUser Create() => new()
    {
        Id = Guid.NewGuid(),
        UserName = $"u{Guid.NewGuid():N}"[..18],
        Email = $"{Guid.NewGuid():N}@test.local",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(ValidPassword),
        DisplayName = "Usuario de prueba",
        Bio = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    public static async Task<List<AppUser>> SeedAsync(ConectandoDbContext db, int count)
    {
        var users = Enumerable.Range(0, count).Select(_ => Create()).ToList();
        db.Users.AddRange(users);
        await db.SaveChangesAsync();
        return users;
    }
}