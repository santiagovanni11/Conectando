using Conectando.Api.Data;
using Conectando.Api.DTOs.Previews;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Arma la vista previa de un post o de un perfil.
///
/// Respeta la visibilidad: un post privado no se previsualiza, ni siquiera
/// para quien lo puede ver, porque el preview se comparte por lo público.
/// </summary>
public class PagePreviewService(ConectandoDbContext dbContext) : IPagePreviewService
{
    private const int DescriptionLength = 200;
    private readonly ConectandoDbContext _db = dbContext;

    public async Task<PagePreviewDto?> GetPostAsync(
        Guid postId,
        Guid viewerId,
        CancellationToken cancellationToken = default)
    {
        var post = await dbContext.Posts
            .AsNoTracking()
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post is null || post.Privacy != PostPrivacy.Public) return null;

        var author = post.Author;
        var image = await FirstImageAsync(postId, cancellationToken);

        return new PagePreviewDto
        {
            Type = "article",
            Title = $"{author?.DisplayName ?? "Alguien"} en Conectando",
            Description = BuildDescription(post.Content, $"por {author?.DisplayName}".Trim()),
            ImageUrl = image,
            CanonicalUrl = $"/posts/{post.Id}",
        };
    }

    public async Task<PagePreviewDto?> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null) return null;

        // Las cuentas privadas no se previsualizan: filtrarían su existencia.
        if (user.IsPrivate) return null;

        var postCount = await dbContext.Posts
            .AsNoTracking()
            .CountAsync(p => p.AuthorId == userId && p.Privacy == PostPrivacy.Public, cancellationToken);

        return new PagePreviewDto
        {
            Title = $"{user.DisplayName} (@{user.UserName}) · Conectando",
            Description = BuildDescription(
                user.Bio, $"{postCount} {(postCount == 1 ? "publicación" : "publicaciones")}"),
            ImageUrl = user.ProfileImageUrl,
            CanonicalUrl = $"/users/{user.Id}",
        };
    }

    private async Task<string?> FirstImageAsync(Guid postId, CancellationToken cancellationToken)
    {
        return await _db.PostMedia
            .AsNoTracking()
            .Where(m => m.PostId == postId)
            .OrderBy(m => m.DisplayOrder)
            .Select(m => m.Url)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string BuildDescription(string? content, string? fallback)
    {
        var text = string.IsNullOrWhiteSpace(content) ? fallback : content;
        if (string.IsNullOrWhiteSpace(text)) return "La red social donde las personas se encuentran.";

        return text!.Length <= DescriptionLength
            ? text
            : text[..DescriptionLength].TrimEnd() + "…";
    }
}