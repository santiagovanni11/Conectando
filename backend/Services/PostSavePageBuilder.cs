using Conectando.Api.Data;
using Conectando.Api.DTOs.Posts;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>Fila de la lista de guardados: qué post y cuándo se guardó.</summary>
public record SavedPostRow(Guid PostId, DateTime SavedAt);

/// <summary>
/// Arma la página de publicaciones guardadas.
///
/// Va aparte del servicio porque es solo proyección: traer media, likes y
/// comentarios ya lo hace <see cref="PostReadService"/>, y repetirlo aquí
/// mezclaría dos responsabilidades en un mismo archivo.
/// </summary>
public static class PostSavePageBuilder
{
    public static async Task<PostListDto> BuildAsync(
        ConectandoDbContext db,
        List<SavedPostRow> rows,
        Dictionary<Guid, Post> byId,
        Guid viewerId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var hasMore = rows.Count > limit;
        var page = hasMore ? rows.Take(limit).ToList() : rows;

        // Si alguien borró un post justo mientras se listaba, se lo saltea
        // en vez de romper la página entera.
        var posts = page
            .Select(r => byId.GetValueOrDefault(r.PostId))
            .Where(p => p is not null)
            .Select(p => p!)
            .ToList();

        var ids = posts.Select(p => p.Id).ToList();

        var media = await db.PostMedia
            .AsNoTracking()
            .Where(m => m.PostId != null && ids.Contains(m.PostId.Value) && m.State == PostMediaState.Attached)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

        var byPost = media.GroupBy(m => m.PostId!.Value).ToDictionary(g => g.Key, g => g.ToList());
        var likeCounts = await PostLikeReader.CountByPostsAsync(db, ids, cancellationToken);
        var likedByMeIds = await PostLikeReader.LikedByUserAsync(db, ids, viewerId, cancellationToken);
        var commentCounts = CommentCountService
            .CountCommentsByPosts(db.Comments, ids)
            .ToDictionary(x => x.Key, x => x.Value);

        // El cursor va por la fecha del post, no por la del guardado: es el
        // mismo formato que usa el resto de listas, así el paginado es
        // idéntico y no hace falta un tipo de cursor nuevo.
        var last = posts.LastOrDefault();
        var nextCursor = hasMore && last is not null
            ? PostListCursor.Encode(last)
            : null;

        return new PostListDto
        {
            Items = posts.Select(p => PostDtoMapping.ToDto(p, p.Author,
                byPost.TryGetValue(p.Id, out var list) ? list : [],
                likeCounts.GetValueOrDefault(p.Id),
                likedByMeIds.Contains(p.Id),
                commentCounts.GetValueOrDefault(p.Id, 0),
                true)).ToList(),
            NextCursor = nextCursor,
            HasMore = hasMore,
        };
    }
}