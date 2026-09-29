using Conectando.Api.Interfaces;

namespace Conectando.Api.Tests.TestInfrastructure;

/// <summary>
/// Doble del que manda los contadores de la barra al hub.
/// </summary>
/// <remarks>
/// Guarda a quién se le avisa para que los tests puedan comprobar que el
/// número se actualiza justo cuando corresponde. Con un doble mudo, un hub
/// que no avisara a nadie pasaría igual los tests que solo miran el mensaje.
/// </remarks>
public class FakeNavCountsBroadcaster : INavCountsBroadcaster
{
    public List<Guid> Notified { get; } = [];

    public Task NotifyAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        Notified.Add(userId);
        return Task.CompletedTask;
    }

    public Task NotifyManyAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        Notified.AddRange(userIds);
        return Task.CompletedTask;
    }

    public bool NotifiedTo(Guid userId) => Notified.Contains(userId);
}
