using Conectando.Api.Data;
using Conectando.Api.DTOs.Posts;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public class PostPatchService(ConectandoDbContext dbContext, PostMediaUpdater mediaUpdater) : IPostPatchService
{
    public async Task<PostDto> PatchAsync(Guid postId, Guid userId, PatchPostRequest request, CancellationToken cancellationToken = default)
    {
        var post = await dbContext.Posts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post is null) throw new PostNotFoundException();
        if (post.AuthorId != userId) throw new PostOwnershipException();

        if (request.Content.HasValue)
        {
            post.Content = PostValidation.NormalizeContent(request.Content.Value);
        }

        if (request.Privacy.HasValue)
        {
            post.Privacy = request.Privacy.Value;
        }

        var desiredIds = request.MediaIds.HasValue
            ? request.MediaIds.Value!.Distinct().ToList()
            : post.Media.Select(m => m.Id).ToList();

        PostValidation.ValidateOrThrow(post.Content, desiredIds.Count);
        post.UpdatedAt = DateTime.UtcNow;

        await mediaUpdater.ApplyAsync(post, userId, desiredIds, cancellationToken);

        return await BuildDtoAsync(post.Id, cancellationToken);
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