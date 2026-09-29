using Conectando.Api.DTOs.Comments;

namespace Conectando.Api.Interfaces;

public interface ICommentService
{
    Task<CommentPageDto> GetCommentsAsync(Guid postId, Guid userId, string? cursor, int limit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommentDto>> GetRepliesAsync(Guid parentCommentId, Guid userId, int limit, CancellationToken cancellationToken = default);
    Task<CommentDto> CreateCommentAsync(Guid postId, Guid userId, CreateCommentRequest request, CancellationToken cancellationToken = default);
    Task<CommentDto> UpdateCommentAsync(Guid commentId, Guid userId, UpdateCommentRequest request, CancellationToken cancellationToken = default);
    Task DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}