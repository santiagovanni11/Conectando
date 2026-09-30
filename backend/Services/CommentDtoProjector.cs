using Conectando.Api.Data;
using Conectando.Api.DTOs.Comments;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public static class CommentDtoProjector
{
    public static async Task<List<CommentDto>> ProjectAsync(ConectandoDbContext dbContext, IReadOnlyList<Comment> comments, CancellationToken cancellationToken)
    {
        if (comments.Count == 0)
        {
            return new List<CommentDto>();
        }

        var authorIds = comments.Select(c => c.AuthorId).Distinct().ToList();
        var authors = await dbContext.Users
            .AsNoTracking()
            .Where(u => authorIds.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName, u.DisplayName, u.ProfileImageUrl, u.DeletedAt })
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        return comments.Select(c =>
        {
            var author = authors.GetValueOrDefault(c.AuthorId);
            return new CommentDto
            {
                Id = c.Id,
                PostId = c.PostId,
                AuthorId = c.AuthorId,
                Author = new CommentAuthorDto
                {
                    Id = author?.Id ?? c.AuthorId,
                    UserName = author?.UserName ?? string.Empty,
                    DisplayName = author?.DisplayName ?? string.Empty,
                    ProfileImageUrl = author?.ProfileImageUrl,
                    IsDeleted = author is null || author.DeletedAt is not null,
                },
                ParentCommentId = c.ParentCommentId,
                Content = c.DeletedAt is not null ? "Este comentario fue eliminado." : c.Content,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                IsEdited = c.DeletedAt is null && c.UpdatedAt > c.CreatedAt,
                IsDeleted = c.DeletedAt is not null,
            };
        }).ToList();
    }
}