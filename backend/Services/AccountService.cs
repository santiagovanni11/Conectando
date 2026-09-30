using Conectando.Api.Data;
using Conectando.Api.DTOs.Account;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Lo que el usuario hace con su propia cuenta: cambiar la contraseña y darla
/// de baja.
/// </summary>
/// <remarks>
/// Son dos operaciones que comparten lo importante: ambas exigen la contraseña
/// actual antes de tocar nada. Esa comprobación es la que separa "pedí que
/// me cambien la contraseña" de "alguien con un token robado tomó la cuenta".
/// Por eso vive en un servicio y no en cada endpoint.
///
/// Va en parciales por responsabilidad: esta parte tiene lo compartido, la
/// baja está en <c>AccountService.Delete.cs</c> y la limpieza en
/// <c>AccountService.Purge.cs</c>.
/// </remarks>
public partial class AccountService(
    ConectandoDbContext db,
    IMediaCleaner mediaCleaner) : IAccountService
{
    private readonly ConectandoDbContext db = db;
    private readonly IMediaCleaner mediaCleaner = mediaCleaner;

    /// <summary>
    /// Busca la cuenta viva de un usuario. Falla si la fila no existe o si ya
    /// fue dada de baja, para que una operación nunca toque una cuenta
    /// eliminada por accidente.
    /// </summary>
    private async Task<AppUser> RequireActiveUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || user.DeletedAt is not null)
        {
            throw new UserNotFoundException();
        }

        return user;
    }

    /// <summary>
    /// Comprueba la contraseña que el usuario escribió. Es el mismo control
    /// para las dos operaciones, y siempre se hace igual: aunque la cuenta
    /// no exista, se devuelve el mismo error y se tarda lo mismo, para no
    /// revelar por el tiempo de respuesta qué emails existen.
    /// </summary>
    private static void RequireCurrentPassword(AppUser user, string currentPassword)
    {
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            throw new CurrentPasswordIncorrectException();
        }
    }
}
