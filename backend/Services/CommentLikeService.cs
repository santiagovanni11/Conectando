using Conectando.Api.Data;
using Conectando.Api.DTOs.Comments;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Me gusta de un comentario.
///
/// <para>
/// Es el mismo criterio que el de las publicaciones, con una diferencia que
/// importa: solo se puede dar like a un comentario que el usuario puede ver.
/// Por eso valida contra el post que lo contiene, con el mismo servicio de
/// visibilidad que usa el feed. Sin eso, alguien podría dar like a un
/// comentario de un post privado y enterarse de que existe.
/// </para>
/// </summary>
public class CommentLikeService(
    ConectandoDbContext dbContext,
    PostVisibilityService visibility) : ICommentLikeService
{
    public async Task<CommentLikeStatusDto> LikeAsync(
        Guid commentId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await RequireVisibleCommentAsync(commentId, userId, cancellationToken);

        // Se comprueba antes de agregar en vez de confiar en la clave y
        // atrapar el duplicado. La base igual lo impide, pero entre que llega
        // el INSERT y lo rechaza, el change tracker de EF ya tiró: la clave
        // repetida rompe antes de llegar a Postgres.
        var yaExiste = await dbContext.CommentLikes
            .AnyAsync(l => l.CommentId == commentId && l.UserId == userId, cancellationToken);

        if (!yaExiste)
        {
            dbContext.CommentLikes.Add(new CommentLike
            {
                CommentId = commentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            });

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return await EstadoAsync(commentId, userId, cancellationToken);
    }

    public async Task<CommentLikeStatusDto> UnlikeAsync(
        Guid commentId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await RequireVisibleCommentAsync(commentId, userId, cancellationToken);

        await dbContext.CommentLikes
            .Where(l => l.CommentId == commentId && l.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        return await EstadoAsync(commentId, userId, cancellationToken);
    }

    /// <summary>
    /// El comentario tiene que existir, no estar borrado y estar en un post que
    /// el usuario puede ver.
    /// </summary>
    private async Task RequireVisibleCommentAsync(
        Guid commentId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var comment = await dbContext.Comments
            .AsNoTracking()
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

        if (comment is null || comment.DeletedAt is not null)
        {
            throw new CommentNotFoundException();
        }

        if (!await visibility.IsVisibleAsync(comment.Post, userId, cancellationToken))
        {
            throw new CommentNotFoundException();
        }
    }

    private async Task<CommentLikeStatusDto> EstadoAsync(
        Guid commentId,
        Guid userId,
        CancellationToken cancellationToken) =>
        new()
        {
            LikesCount = await dbContext.CommentLikes.CountAsync(l => l.CommentId == commentId, cancellationToken),
            LikedByMe = await dbContext.CommentLikes.AnyAsync(
                l => l.CommentId == commentId && l.UserId == userId, cancellationToken),
        };
}