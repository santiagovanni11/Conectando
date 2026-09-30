using Conectando.Api.Data;
using Conectando.Api.Models;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests.TestInfrastructure;

/// <summary>
/// Deja una cuenta como dada de baja sin pasar por la limpieza.
///
/// <para>
/// A mano y no con el servicio porque la limpieza es justamente lo que no se
/// prueba en los tests del filtro: el punto es que las filas viejas queden
/// ahí, como quedaron en las cuentas borradas antes de que existiera.
/// </para>
///
/// <para>
/// Vive en la infraestructura y no en cada archivo de pruebas porque la
/// usan varios, y porque repetirla en cada uno invitaría a que una se desvíe de
/// la otra justo en el detalle que importa.
/// </para>
/// </summary>
public static class BajaSinLimpiar
{
    public static async Task AplicarAsync(ConectandoDbContext db, AppUser user)
    {
        user.DeletedAt = DateTime.UtcNow;
        user.Email = $"eliminado-{user.Id:N}@conectando.invalid";
        user.UserName = $"eliminado-{user.Id:N}";
        user.DisplayName = "Usuario eliminado";
        user.ProfileImageUrl = null;
        user.Bio = null;
        await db.SaveChangesAsync();
    }
}