namespace Conectando.Api.Interfaces;

/// <summary>
/// Avisa que los contadores de la navegación cambiaron.
/// </summary>
/// <remarks>
/// Existe porque los números de la barra (mensajes, amigos, notificaciones) se
/// calculan en el servidor y viven en varios lugares: un mensaje nuevo, una
/// notificación creada, marcar algo como leído. Si cada uno de esos caminos
/// avisara por su cuenta, cada uno tendría que acordarse de hacerlo, y el que
/// se olvidara se vería como un número que tarda en aparecer o que no se borra.
/// </remarks>
public interface INavCountsBroadcaster
{
    /// <summary>
    /// Manda los contadores actualizados a un usuario. Nunca falla: si el
    /// usuario no está conectado no hay nadie a quien avisarle, y eso no es un
    /// error.
    /// </summary>
    Task NotifyAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Lo mismo, para varios usuarios. Se manda de a uno.</summary>
    Task NotifyManyAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}
