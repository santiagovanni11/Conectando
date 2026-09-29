using Conectando.Api.DTOs.Posts;

namespace Conectando.Api.Interfaces;

public interface ILikeService
{
    Task<PostLikeStatusDto> LikeAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
    Task<PostLikeStatusDto> UnlikeAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
    Task<PostLikeStatusDto> GetStatusAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
    Task<LikePageDto> GetLikesAsync(Guid postId, Guid userId, string? cursor, int limit, CancellationToken cancellationToken = default);
}