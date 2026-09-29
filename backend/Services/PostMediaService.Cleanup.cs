using Conectando.Api.Models;
using Conectando.Api.Settings;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Recoge lo que las subidas a medias dejaron tirado.
/// </summary>
public partial class PostMediaService
{
    /// <summary>
    /// Borra los borradores que nadie llegó a publicar. Existe porque el
    /// archivo se sube antes que la publicación: si el usuario abandona el
    /// compositor, la foto ya está en Cloudinary y pagándola, pero sin nada
    /// que la referencie.
    /// </summary>
    private async Task CleanupStalePendingAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow - PostConstants.PendingMediaTtl;
        var stale = await dbContext.PostMedia
            .Where(m => m.UserId == userId && m.State == PostMediaState.Pending && m.CreatedAt < cutoff)
            .ToListAsync(cancellationToken);

        foreach (var item in stale)
        {
            dbContext.PostMedia.Remove(item);
            await cleaner.TryDeleteAsync(item.PublicId, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
