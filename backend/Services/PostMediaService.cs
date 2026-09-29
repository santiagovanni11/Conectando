using Conectando.Api.Data;
using Conectando.Api.DTOs.Posts;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Conectando.Api.Settings;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Subidas de fotos de publicaciones.
/// </summary>
/// <remarks>
/// Armado en parciales por responsabilidad: este coordina la subida,
/// <c>.Limits</c> decide si se acepta y <c>.Cleanup</c> junta lo que quedó a
/// medias. Juntas eran más de 150 líneas y mezclaban tres motivos distintos
/// para frenar una subida.
/// </remarks>
public partial class PostMediaService : IPostMediaService
{
    private readonly ConectandoDbContext dbContext;
    private readonly IMediaStorage storage;
    private readonly IMediaCleaner cleaner;
    private readonly IStorageQuotaService quota;

    public PostMediaService(
        ConectandoDbContext dbContext,
        IMediaStorage storage,
        IMediaCleaner cleaner,
        IStorageQuotaService quota)
    {
        this.dbContext = dbContext;
        this.storage = storage;
        this.cleaner = cleaner;
        this.quota = quota;
    }

    public async Task<List<PostMediaDto>> UploadAsync(Guid userId, IFormFile[] files, CancellationToken cancellationToken = default)
    {
        if (files is null || files.Length == 0)
        {
            throw new MediaLimitException("No se recibieron archivos.");
        }

        if (files.Length > PostConstants.MaxMediaPerPost)
        {
            throw new MediaLimitException($"Podés subir hasta {PostConstants.MaxMediaPerPost} fotos por publicación.");
        }

        // El orden importa: primero se juntan los borradores viejos, porque si
        // no cuentan contra el límite de pendientes y el usuario queda sin
        // poder subir hasta que los borre a mano.
        await CleanupStalePendingAsync(userId, cancellationToken);
        await EnsureCapacityAsync(userId, files, cancellationToken);

        var created = new List<PostMedia>();
        var uploaded = new List<StoredMedia>();
        var now = DateTime.UtcNow;
        var index = 0;

        try
        {
            foreach (var file in files)
            {
                await using var stream = file.OpenReadStream();
                var contentType = PostFileValidator.Validate(file.FileName, file.ContentType, file.Length, stream);

                var stored = await storage.UploadAsync(stream, file.FileName, contentType, cancellationToken);
                uploaded.Add(stored);

                var media = new PostMedia
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Url = stored.Url,
                    PublicId = stored.PublicId,
                    DisplayOrder = index++,
                    State = PostMediaState.Pending,
                    CreatedAt = now,
                    SizeBytes = file.Length,
                };
                created.Add(media);
                dbContext.PostMedia.Add(media);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Si la fila no llegó a guardarse, el archivo tampoco debe quedar:
            // la subida se paga igual, exista o no el registro.
            foreach (var stored in uploaded)
            {
                await cleaner.TryDeleteAsync(stored.PublicId, cancellationToken);
            }

            throw;
        }

        return created.Select(m => new PostMediaDto
        {
            Id = m.Id,
            Url = m.Url,
            DisplayOrder = m.DisplayOrder,
        }).ToList();
    }

    public async Task DeleteAsync(Guid mediaId, Guid userId, CancellationToken cancellationToken = default)
    {
        var media = await dbContext.PostMedia
            .FirstOrDefaultAsync(m => m.Id == mediaId, cancellationToken);

        if (media is null)
        {
            throw new PendingMediaNotFoundException();
        }

        if (media.State == PostMediaState.Attached)
        {
            if (media.PostId is null)
            {
                throw new PendingMediaNotFoundException();
            }

            var post = await dbContext.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == media.PostId, cancellationToken);

            if (post is null || post.AuthorId != userId)
            {
                throw new PostOwnershipException();
            }
        }
        else if (media.UserId != userId)
        {
            throw new PendingMediaNotFoundException();
        }

        dbContext.PostMedia.Remove(media);
        await dbContext.SaveChangesAsync(cancellationToken);
        await cleaner.TryDeleteAsync(media.PublicId, cancellationToken);
    }
}
