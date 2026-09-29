using Conectando.Api.DTOs.Posts;

namespace Conectando.Api.Interfaces;

public interface IPostMediaService
{
    Task<List<PostMediaDto>> UploadAsync(Guid userId, IFormFile[] files, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid mediaId, Guid userId, CancellationToken cancellationToken = default);
}