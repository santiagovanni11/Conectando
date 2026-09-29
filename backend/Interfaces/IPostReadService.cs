using Conectando.Api.DTOs.Posts;

namespace Conectando.Api.Interfaces;

public interface IPostReadService
{
    Task<PostDto> GetByIdAsync(Guid id, Guid viewerId, CancellationToken cancellationToken = default);
    Task<PostListDto> ListByUserAsync(Guid authorId, Guid viewerId, PostListQuery query, CancellationToken cancellationToken = default);
}