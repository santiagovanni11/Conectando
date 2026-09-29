using Conectando.Api.Data;
using Conectando.Api.DTOs.Comments;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public class CommentWriteService(ConectandoDbContext dbContext, PostVisibilityService visibility, INotificationService notifications) : ICommentService
{
    private readonly CommentReadService _readService = new(dbContext, visibility);

    public async Task<CommentPageDto> GetCommentsAsync(Guid postId, Guid userId, string? cursor, int limit, CancellationToken cancellationToken = default)
    {
        return await _readService.GetCommentsAsync(postId, userId, cursor, limit, cancellationToken);
    }

    public async Task<IReadOnlyList<CommentDto>> GetRepliesAsync(Guid parentCommentId, Guid userId, int limit, CancellationToken cancellationToken = default)
    {
        return await _readService.GetRepliesAsync(parentCommentId, userId, limit, cancellationToken);
    }

    public async Task<CommentDto> CreateCommentAsync(Guid postId, Guid userId, CreateCommentRequest request, CancellationToken cancellationToken = default)
    {
        CommentValidation.ValidateContent(request.Content);

        var post = await dbContext.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post is null || !await visibility.IsVisibleAsync(post, userId, cancellationToken))
        {
            throw new PostNotFoundException();
        }

        if (request.ParentCommentId is not null)
        {
            await CommentWriteSupport.ValidateParentAsync(dbContext, request.ParentCommentId.Value, postId, cancellationToken);
        }

        var now = DateTime.UtcNow;
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            AuthorId = userId,
            ParentCommentId = request.ParentCommentId,
            Content = request.Content.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync(cancellationToken);
        await notifications.NotifyAsync(post.AuthorId, userId, NotificationTypeRequest.Comment, postId, cancellationToken);

        var author = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
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
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            IsEdited = false,
            IsDeleted = false,
            RepliesCount = 0,
        };
    }

    public async Task<CommentDto> UpdateCommentAsync(Guid commentId, Guid userId, UpdateCommentRequest request, CancellationToken cancellationToken = default)
    {
        CommentValidation.ValidateContent(request.Content);

        var comment = await dbContext.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

        if (comment is null || comment.DeletedAt is not null)
        {
            throw new CommentNotFoundException();
        }

        if (comment.AuthorId != userId)
        {
            throw new CommentOwnershipException();
        }

        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return await CommentWriteSupport.ProjectAsync(dbContext, comment, cancellationToken);
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var comment = await dbContext.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

        if (comment is null)
        {
            throw new CommentNotFoundException();
        }

        if (comment.AuthorId != userId)
        {
            throw new CommentOwnershipException();
        }

        comment.DeletedAt = DateTime.UtcNow;
        comment.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}