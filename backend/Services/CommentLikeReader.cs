using Conectando.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Cuántos me gusta lleva un comentario y si los puso quien mira.
/// </summary>
/// <remarks>
/// Es <c>struct</c> y no <c>class</c> a propósito: así
/// <c>GetValueOrDefault</c> devuelve el valor por omisión —cero me gusta,
/// ninguno del que mira— para los comentarios que todavía no tienen ninguno,
/// en vez de <c>null</c> y un <c>NullReferenceException</c> al leer.
/// </remarks>
public readonly record struct CommentLikeState(int Count, bool LikedByMe);

/// <summary>
/// Lee el estado de los me gusta de varios comentarios en una sola pasada.
///
/// <para>
/// Existe para no consultar por comentario. Un listado trae veinte y sus
/// respuestas, y con una consulta por cada uno el costo crece con el tamaño de
/// la página. Acá son dos en total: una sola lectura y el agrupado en memoria.
/// </para>
/// </summary>
public static class CommentLikeReader
{
    public static async Task<Dictionary<Guid, CommentLikeState>> GetByCommentsAsync(
        ConectandoDbContext db,
        List<Guid> commentIds,
        Guid viewerId,
        CancellationToken cancellationToken = default)
    {
        if (commentIds.Count == 0)
        {
            return [];
        }

        var likes = await db.CommentLikes
            .AsNoTracking()
            .Where(l => commentIds.Contains(l.CommentId))
            .Select(l => new { l.CommentId, l.UserId })
            .ToListAsync(cancellationToken);

        return likes
            .GroupBy(l => l.CommentId)
            .ToDictionary(
                g => g.Key,
                g => new CommentLikeState(g.Count(), g.Any(l => l.UserId == viewerId)));
    }
}