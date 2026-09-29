namespace Conectando.Api.Interfaces;

public record StoredMedia(string Url, string PublicId);

public interface IMediaStorage
{
    Task<StoredMedia> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string publicId, CancellationToken cancellationToken = default);
}