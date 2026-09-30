using System.Linq.Expressions;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Data.Configuration;

/// <summary>
/// Filtros que hacen desaparecer una cuenta dada de baja.
///
/// <para>
/// Al borrarse una cuenta se limpia lo que dejó —publicaciones, vínculos,
/// archivos— y la fila queda vacía porque los mensajes la apuntan. Eso alcanza
/// para las cuentas nuevas, pero no para las que ya estaban dadas de baja
/// cuando se limpió: sus publicaciones seguían en el feed y la cuenta
/// seguía en las listas.
/// </para>
///
/// <para>
/// Por eso, además de limpiar, cada tabla filtra por acá. Declarado en el
/// modelo y no en cada consulta, porque una cuenta dada de baja tiene que
/// ser invisible en todas partes, y una consulta nueva que se olvide de
/// filtrar la volvería a mostrar sin que nadie se entere.
/// </para>
///
/// <para>
/// No filtran los mensajes ni las conversaciones: esos se quedan a propósito,
/// porque el chat es también historial de la persona que no se dio de baja.
/// </para>
/// </summary>
public static class CuentasEliminadas
{
    /// <summary>La cuenta existe y no fue dada de baja.</summary>
    public static readonly Expression<Func<AppUser, bool>> Viva = u => u.DeletedAt == null;

    public static readonly Expression<Func<Post, bool>> AutorVivo = p => p.Author.DeletedAt == null;

    public static readonly Expression<Func<Comment, bool>> ComentaristaVivo = c => c.Author.DeletedAt == null;

    public static readonly Expression<Func<Like, bool>> QuienPusoMeGustaVivo = l => l.User.DeletedAt == null;

    public static readonly Expression<Func<Share, bool>> QuienCompartioVivo = s => s.User.DeletedAt == null;

    public static readonly Expression<Func<PostMedia, bool>> DueñoVivo = m => m.User.DeletedAt == null;

    public static readonly Expression<Func<PasswordResetCode, bool>> ConCuentaViva = c => c.User.DeletedAt == null;

    public static readonly Expression<Func<Follow, bool>> SeguimientoEntreVivas =
        f => f.User.DeletedAt == null && f.TargetUser.DeletedAt == null;

    public static readonly Expression<Func<Friendship, bool>> AmistadEntreVivas =
        f => f.UserLow.DeletedAt == null && f.UserHigh.DeletedAt == null;

    public static readonly Expression<Func<FriendRequest, bool>> SolicitudEntreVivas =
        r => r.Requester.DeletedAt == null && r.Addressee.DeletedAt == null;

    public static readonly Expression<Func<Block, bool>> BloqueoEntreVivas =
        b => b.User.DeletedAt == null && b.BlockedUser.DeletedAt == null;

    public static readonly Expression<Func<PostSave, bool>> QuienGuardoVivo = s => s.User.DeletedAt == null;

    public static readonly Expression<Func<Notification, bool>> ActorVivo =
        n => n.Actor == null || n.Actor.DeletedAt == null;
}