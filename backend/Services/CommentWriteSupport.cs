using Conectando.Api.Data;
using Conectando.Api.DTOs.Comments;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Helpers de escritura de comentarios: validación de parent y proyección a DTO.
/// </summary>
public static class CommentWriteSupport
{
    public static async Task<CommentDto> ProjectAsync(
        ConectandoDbContext dbContext, Comment comment, CancellationToken cancellationToken)
    {
        var author = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == comment.AuthorId)
            .Select(u => new { u.Id, u.UserName, u.DisplayName, u.ProfileImageUrl })
            .FirstAsync(cancellationToken);

        return new CommentDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            AuthorId = comment.AuthorId,
            Author = new CommentAuthorDto
            {
                Id = author.Id,
                UserName = author.UserName,
                DisplayName = author.DisplayName,
                ProfileImageUrl = author.ProfileImageUrl,
            },
            ParentCommentId = comment.ParentCommentId,
            Content = comment.DeletedAt is not null ? "Este comentario fue eliminado." : comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            IsEdited = comment.DeletedAt is null && comment.UpdatedAt > comment.CreatedAt,
            IsDeleted = comment.DeletedAt is not null,
            RepliesCount = 0,
        };
    }

    public static async Task ValidateParentAsync(
        ConectandoDbContext dbContext, Guid parentId, Guid postId, CancellationToken cancellationToken)
    {
        var parent = await dbContext.Comments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == parentId, cancellationToken);

        if (parent is null)
        {
            throw new InvalidParentCommentException("El comentario al que intentás responder no existe.");
        }

        if (parent.DeletedAt is not null)
        {
            throw new InvalidParentCommentException("No podés responder a un comentario eliminado.");
        }

        if (parent.PostId != postId)
        {
            throw new InvalidParentCommentException("El comentario pertenece a otra publicación.");
        }

        if (parent.ParentCommentId is not null)
        {
            throw new InvalidParentCommentException("No podés responder a una respuesta. La profundidad máxima es 1 nivel.");
        }
    }
}
