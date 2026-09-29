using Conectando.Api.Interfaces;

namespace Conectando.Api.Services;

public class MediaCleaner(IMediaStorage storage, ILogger<MediaCleaner> logger) : IMediaCleaner
{
    public async Task TryDeleteAsync(string? publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return;
        }

        try
        {
            await storage.DeleteAsync(publicId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo eliminar un archivo de Cloudinary (publicId {PublicId}).", publicId);
        }
    }
}
