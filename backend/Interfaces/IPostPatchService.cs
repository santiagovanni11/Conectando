using Conectando.Api.DTOs.Posts;

namespace Conectando.Api.Interfaces;

public interface IPostPatchService
{
    /// <summary>
    /// Actualización parcial (PATCH): solo aplica los campos provistos en el request.
    /// </summary>
    Task<PostDto> PatchAsync(Guid postId, Guid userId, PatchPostRequest request, CancellationToken cancellationToken = default);
}