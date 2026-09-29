using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Aplica el plan de media de un post (agregar pendientes, reordenar y destruir
/// los removidos). Compartido por PUT y PATCH para evitar duplicación.
/// </summary>
public class PostMediaUpdater(ConectandoDbContext dbContext, IMediaStorage storage, ILogger<PostMediaUpdater> logger)
{
    public async Task ApplyAsync(Post post, Guid userId, List<Guid> desiredIds, CancellationToken cancellationToken = default)
    {
        var (toAddIds, toRemove) = PostMediaPlan.Plan(post, desiredIds);
        var order = PostMediaPlan.BuildOrderIndex(desiredIds);

        foreach (var item in toRemove) dbContext.PostMedia.Remove(item);
        PostMediaPlan.AttachPending(post, await LoadPendingToAttachAsync(userId, toAddIds, cancellationToken), order);
        foreach (var item in post.Media.Where(m => !toRemove.Contains(m))) item.DisplayOrder = order[item.Id];

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var item in toRemove) await DestroyAsync(item, cancellationToken);
    }

    public async Task DestroyMediaAsync(IEnumerable<PostMedia> media, CancellationToken cancellationToken = default)
    {
        foreach (var item in media) await DestroyAsync(item, cancellationToken);
    }

    private async Task<List<PostMedia>> LoadPendingToAttachAsync(Guid userId, List<Guid> toAddIds, CancellationToken cancellationToken)
    {
        if (toAddIds.Count == 0) return [];
        var set = toAddIds.ToHashSet();
        var pending = await dbContext.PostMedia
            .Where(m => m.UserId == userId && m.State == PostMediaState.Pending && set.Contains(m.Id))
            .ToListAsync(cancellationToken);

        if (pending.Count != toAddIds.Count) throw new PendingMediaNotFoundException();
        return pending;
    }

    private async Task DestroyAsync(PostMedia media, CancellationToken cancellationToken)
    {
        try
        {
            await storage.DeleteAsync(media.PublicId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo eliminar el archivo de Cloudinary (publicId {PublicId}).", media.PublicId);
        }
    }
}