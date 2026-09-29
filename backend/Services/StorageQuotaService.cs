using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Conectando.Api.Services;

/// <summary>
/// Calcula el espacio usado sumando lo que hay en la base.
/// </summary>
/// <remarks>
/// Podría llevar un contador acumulado en el usuario, que se lee en O(1), pero
/// ese número se desincroniza solo: si un borrado falla, o una cuenta se da
/// de baja, el contador queda mintiendo y nadie se entera hasta que llega la
/// factura. La suma se recalcula sola y no puede mentir. Con el índice por
/// <c>(UserId, State)</c> la lectura es un recorrido de índice sobre las filas
/// del usuario, que son pocas comparadas con las de toda la tabla.
/// </remarks>
public class StorageQuotaService(
    ConectandoDbContext dbContext,
    IOptions<StorageQuotaSettings> settings) : IStorageQuotaService
{
    public async Task<long> GetUsageAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var mediaBytes = await dbContext.PostMedia
            .Where(m => m.UserId == userId)
            .SumAsync(m => m.SizeBytes, cancellationToken);

        var avatarBytes = await dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => u.ProfileImageSizeBytes)
            .SingleOrDefaultAsync(cancellationToken);

        return mediaBytes + avatarBytes;
    }

    public async Task EnsureCanStoreAsync(Guid userId, long additionalBytes, CancellationToken cancellationToken = default)
    {
        var limit = settings.Value.MaxBytesPerUser;
        var attempted = await GetUsageAsync(userId, cancellationToken) + additionalBytes;

        if (attempted > limit)
        {
            throw new StorageQuotaExceededException(attempted, limit);
        }
    }
}
