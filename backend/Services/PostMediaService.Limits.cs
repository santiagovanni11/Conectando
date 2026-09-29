using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Conectando.Api.Settings;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Las dos razones por las que una subida se frena antes de gastar un byte.
/// </summary>
public partial class PostMediaService
{
    /// <summary>
    /// Mira el lote entero de una sola vez. Si se evaluara archivo por
    /// archivo, alguien al borde del tope subiría el primero y vería el
    /// segundo rechazado: pagaría una subida que no le sirvió de nada.
    /// </summary>
    private async Task EnsureCapacityAsync(Guid userId, IFormFile[] files, CancellationToken cancellationToken)
    {
        var pending = await dbContext.PostMedia
            .CountAsync(m => m.UserId == userId && m.State == PostMediaState.Pending, cancellationToken);

        if (pending + files.Length > PostConstants.MaxPendingMediaPerUser)
        {
            throw new MediaLimitException("Tenés demasiadas fotos sin publicar. Eliminá las que no uses.");
        }

        // Va antes de subir a propósito. Cloudinary cobra por lo que entra, y
        // lo que se va a rechazar no tiene sentido pagarlo.
        var incoming = files.Sum(file => file.Length);
        await quota.EnsureCanStoreAsync(userId, incoming, cancellationToken);
    }
}
