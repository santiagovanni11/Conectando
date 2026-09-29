using Conectando.Api.Data;
using Conectando.Api.DTOs.Posts;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Publicaciones guardadas por el usuario.
///
/// Guardar es personal: no avisa al autor ni genera notificación, porque en
/// Instagram tampoco lo hace. Es una lista privada, no un like.
/// </summary>
public class PostSaveService(
    ConectandoDbContext dbContext,
    PostVisibilityService visibility) : IPostSaveService
{
    private readonly ConectandoDbContext _dbContext = dbContext;

    public async Task<PostSaveStatusDto> ToggleAsync(
        Guid userId, Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _dbContext.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post is null || !await visibility.IsVisibleAsync(post, userId, cancellationToken))
        {
            throw new PostNotFoundException();
        }

        // Se comprueba con una proyección mínima: pedir la entidad completa
        // hace que EF intente unir con "posts" y genere un alias que la base
        // no reconoce.
        var exists = await _dbContext.PostSaves
            .AsNoTracking()
            .AnyAsync(s => s.UserId == userId && s.PostId == postId, cancellationToken);

        if (exists)
        {
            await _dbContext.PostSaves
                .Where(s => s.UserId == userId && s.PostId == postId)
                .ExecuteDeleteAsync(cancellationToken);

            return new PostSaveStatusDto { SavedByMe = false };
        }

        _dbContext.PostSaves.Add(new PostSave { UserId = userId, PostId = postId, CreatedAt = DateTime.UtcNow });
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PostSaveStatusDto { SavedByMe = true };
    }

    public async Task<PostListDto> ListSavedAsync(
        Guid userId, PostListQuery query, CancellationToken cancellationToken = default)
    {
        var limit = Math.Clamp(query.Limit, 1, 20);

        // Solo el lado que el usuario bloqueó: si el bloqueado desapareciera
        // de los guardados del que bloqueó, el bloqueo sería efectivo, pero
        // al revés dejaría publicaciones suyas visibles para quien te
        // bloqueó. Se usa el lado unidireccional.
        var blockedIds = await visibility.GetBlockedByMeIdsAsync(userId, cancellationToken);

        IQueryable<PostSave> baseQuery = _dbContext.PostSaves
            .AsNoTracking()
            .Where(s => s.UserId == userId);

        // El bloqueo se filtra con una subconsulta sobre Posts, no con
        // s.Post.AuthorId: nombrar la navegación dentro del Where obliga a EF
        // a generar un alias ("PostId1") que la base no reconoce.
        if (blockedIds.Count > 0)
        {
            var allowedPosts = _dbContext.Posts
                .AsNoTracking()
                .Where(p => !blockedIds.Contains(p.AuthorId))
                .Select(p => p.Id);

            baseQuery = baseQuery.Where(s => allowedPosts.Contains(s.PostId));
        }

        // El cursor ordena por la fecha de guardado, no por la del post:
        // así la lista no se reordina sola cuando editan una publicación.
        var cursor = PostListCursor.TryDecode(query.Cursor);
        if (cursor is not null)
        {
            baseQuery = baseQuery.Where(s => s.CreatedAt < cursor.Value.CreatedAt
                || (s.CreatedAt == cursor.Value.CreatedAt && s.PostId < cursor.Value.Id));
        }

        var ordered = baseQuery
            .OrderByDescending(s => s.CreatedAt)
            .ThenByDescending(s => s.PostId);

        // No se proyecta la entidad completa: en la consulta SQL eso genera
        // un alias "PostId1" que no existe. Se traen solo los datos de la
        // fila guardada y los posts se cargan aparte.
        var saved = await ordered
            .Select(s => new SavedPostRow(s.PostId, s.CreatedAt))
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var posts = await _dbContext.Posts
            .AsNoTracking()
            .Include(p => p.Author)
            .Where(p => saved.Select(r => r.PostId).Contains(p.Id))
            .ToListAsync(cancellationToken);

        var byId = posts.ToDictionary(p => p.Id, p => p);

        return await PostSavePageBuilder
            .BuildAsync(_dbContext, saved, byId, userId, limit, cancellationToken);
    }
}