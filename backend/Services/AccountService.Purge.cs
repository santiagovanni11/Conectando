using Conectando.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public partial class AccountService
{
    /// <summary>
    /// Borra todo lo que la cuenta dejó atrás.
    /// </summary>
    /// <remarks>
    /// La fila del usuario no se puede borrar: los mensajes la apuntan y el
    /// chat del otro no puede romperse. Entonces se limpia todo lo demás y la
    /// fila queda como un cascarón.
    ///
    /// <para>
    /// El orden importa y va de lo que cuelga de otras personas a lo propio.
    /// Primero se borra lo que el usuario hizo sobre contenido ajeno —likes,
    /// comentarios, compartir— y las relaciones por las dos puntas, porque
    /// esos cuelgan de filas que van a seguir existiendo. Al final van sus
    /// publicaciones, que se llevan por cascada los comentarios, los likes y
    /// las notificaciones de arriba.
    /// </para>
    ///
    /// <para>
    /// Los archivos se juntan antes de borrar las filas porque después ya no
    /// queda de dónde sacar el publicId. Si no se limpian, quedan huérfanos
    /// en Cloudinary pagando por siempre: la fila desaparece pero el archivo
    /// sigue ahí.
    /// </para>
    /// </remarks>
    private async Task PurgarAsync(Guid userId, CancellationToken cancellationToken)
    {
        await BorrarRelacionesAsync(userId, cancellationToken);

        // IgnoreQueryFilters en todo esto, y no por descuido: al marcar la
        // cuenta como dada de baja, los filtros globales la esconden, y justo
        // esta es la única operación que tiene que seguir viéndola para poder
        // borrar lo que dejó.
        await db.PostSaves.IgnoreQueryFilters()
            .Where(s => s.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        await db.Reports.IgnoreQueryFilters()
            .Where(r => r.ReporterId == userId || r.TargetUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await db.PasswordResetCodes.IgnoreQueryFilters()
            .Where(c => c.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        await db.Comments.IgnoreQueryFilters()
            .Where(c => c.AuthorId == userId).ExecuteDeleteAsync(cancellationToken);

        await db.Likes.IgnoreQueryFilters()
            .Where(l => l.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        await db.Shares.IgnoreQueryFilters()
            .Where(s => s.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        var archivos = await db.PostMedia.IgnoreQueryFilters()
            .Where(m => m.UserId == userId)
            .Select(m => m.PublicId)
            .ToListAsync(cancellationToken);

        await db.PostMedia.IgnoreQueryFilters()
            .Where(m => m.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        await db.Posts.IgnoreQueryFilters()
            .Where(p => p.AuthorId == userId).ExecuteDeleteAsync(cancellationToken);

        await LimpiarArchivosAsync(archivos, cancellationToken);
    }

    /// <summary>
    /// Borra las relaciones en las que la cuenta estaba de las dos puntas.
    ///
    /// <para>
    /// Van de las dos porque si solo se borrara del lado de quien las creó,
    /// la cuenta dada de baja seguiría apareciendo como seguidor o amigo en
    /// la lista del otro, que es justo lo que hay que evitar.
    /// </para>
    /// </summary>
    private async Task BorrarRelacionesAsync(Guid userId, CancellationToken cancellationToken)
    {
        await db.Follows.IgnoreQueryFilters()
            .Where(f => f.UserId == userId || f.TargetUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await db.Friendships.IgnoreQueryFilters()
            .Where(f => f.UserLowId == userId || f.UserHighId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await db.FriendRequests.IgnoreQueryFilters()
            .Where(r => r.RequesterId == userId || r.AddresseeId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await db.Blocks.IgnoreQueryFilters()
            .Where(b => b.UserId == userId || b.BlockedUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Pide al almacenamiento que borre los archivos.
    /// </summary>
    /// <para>
    /// Uno por uno y sin cortar el resto si uno falla: el limpiador ya
    /// absorbe el error. Si un archivo no se puede borrar, se pierde ese
    /// archivo, no la baja de la cuenta entera.
    /// </para>
    /// </summary>
    private async Task LimpiarArchivosAsync(List<string> publicIds, CancellationToken cancellationToken)
    {
        foreach (var publicId in publicIds)
        {
            await mediaCleaner.TryDeleteAsync(publicId, cancellationToken);
        }
    }
}