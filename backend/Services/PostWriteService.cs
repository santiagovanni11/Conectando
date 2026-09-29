using Conectando.Api.Data;
using Conectando.Api.DTOs.Posts;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public class PostWriteService(ConectandoDbContext dbContext, PostMediaUpdater mediaUpdater) : IPostWriteService
{
    public async Task<PostDto> CreateAsync(Guid userId, CreatePostRequest request, CancellationToken cancellationToken = default)
    {
        var content = PostValidation.NormalizeContent(request.Content);
        var mediaIds = request.MediaIds?.Distinct().ToList() ?? [];
        PostValidation.ValidateOrThrow(content, mediaIds.Count);

        var media = await dbContext.PostMedia
            .Where(m => m.UserId == userId && m.State == PostMediaState.Pending && mediaIds.Contains(m.Id))
            .ToListAsync(cancellationToken);

        if (media.Count != mediaIds.Count) throw new PendingMediaNotFoundException();

        var now = DateTime.UtcNow;
        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = userId,
            Content = content,
            Privacy = request.Privacy,
            CreatedAt = now,
            UpdatedAt = now,
        };

        PostMediaPlan.AttachPending(post, media, PostMediaPlan.BuildOrderIndex(mediaIds));
        dbContext.Posts.Add(post);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await BuildDtoAsync(post.Id, cancellationToken);
    }

    public async Task<PostDto> UpdateAsync(Guid postId, Guid userId, UpdatePostRequest request, CancellationToken cancellationToken = default)
    {
        var post = await dbContext.Posts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post is null) throw new PostNotFoundException();
        if (post.AuthorId != userId) throw new PostOwnershipException();

        var content = PostValidation.NormalizeContent(request.Content);
        var desiredIds = request.MediaIds?.Distinct().ToList() ?? post.Media.Select(m => m.Id).ToList();
        PostValidation.ValidateOrThrow(content, desiredIds.Count);

        post.Content = content;
        if (request.Privacy is not null) post.Privacy = request.Privacy.Value;
        post.UpdatedAt = DateTime.UtcNow;

        await mediaUpdater.ApplyAsync(post, userId, desiredIds, cancellationToken);

        return await BuildDtoAsync(post.Id, cancellationToken);
    }

    public async Task DeleteAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default)
    {
        var post = await dbContext.Posts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post is null) throw new PostNotFoundException();
        if (post.AuthorId != userId) throw new PostOwnershipException();

        await mediaUpdater.DestroyMediaAsync(post.Media, cancellationToken);

        dbContext.PostMedia.RemoveRange(post.Media);
        dbContext.Posts.Remove(post);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<PostDto> BuildDtoAsync(Guid postId, CancellationToken cancellationToken)
    {
        var post = await dbContext.Posts
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.Media)
            .FirstAsync(p => p.Id == postId, cancellationToken);

        return PostDtoMapping.ToDto(post, post.Author, post.Media);
    }
}