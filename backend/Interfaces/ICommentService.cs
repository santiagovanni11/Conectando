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

/// <summary>
/// Me gusta de un comentario.
///
/// <para>
/// Va aparte de <see cref="ICommentService"/> porque es una operación
/// distinta: no crea ni borra el comentario, solo lo marca. Mezclarla
/// obligaría a los que llaman a la interfaz de comentarios a conocer también
/// los me gusta.
/// </para>
/// </summary>
public interface ICommentLikeService
{
    Task<CommentLikeStatusDto> LikeAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);

    Task<CommentLikeStatusDto> UnlikeAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}