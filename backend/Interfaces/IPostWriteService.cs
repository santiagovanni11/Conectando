using Conectando.Api.DTOs.Posts;

namespace Conectando.Api.Interfaces;

public interface IPostWriteService
{
    Task<PostDto> CreateAsync(Guid userId, CreatePostRequest request, CancellationToken cancellationToken = default);
    Task<PostDto> UpdateAsync(Guid postId, Guid userId, UpdatePostRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
}