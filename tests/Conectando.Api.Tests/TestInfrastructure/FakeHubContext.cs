using Conectando.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Conectando.Api.Tests.TestInfrastructure;

/// <summary>
/// Un hub de mentira que anota a qué grupo fue cada evento.
/// </summary>
/// <remarks>
/// Sirve para comprobar las dos caras de la difusión sin levantar sockets:
/// a quién le llega el evento, y qué pasa cuando la entrega falla. Lo
/// segundo es lo que evita que una escritura ya guardada se reporte como
/// fallida y el cliente la vuelva a guardar por REST.
/// </remarks>
public sealed class FakeHubContext : IHubContext<MessageHub>
{
    private readonly List<Delivered> _deliveries = new();

    public IReadOnlyList<Delivered> Deliveries => _deliveries;

    /// <summary>Hace fallar la entrega, como si el socket estuviera caido.</summary>
    public bool FailOnSend { get; set; }

    public IHubClients Clients { get; }

    public IGroupManager Groups => throw new NotSupportedException(
        "Las pruebas del hub no suscriben: solo difunden.");

    public FakeHubContext() => Clients = new FakeHubClients(this);

    private void Deliver(string group, string eventName, object? payload)
    {
        _deliveries.Add(new Delivered(group, eventName, payload));

        if (FailOnSend)
        {
            throw new InvalidOperationException("No se pudo entregar el evento en vivo.");
        }
    }

    public sealed record Delivered(string Group, string EventName, object? Payload);

    private sealed class FakeHubClients(FakeHubContext owner) : IHubClients
    {
        private IClientProxy To(string? group) => new FakeClientProxy(owner, group);

        public IClientProxy All => To(null);

        public IClientProxy Group(string groupName) => To(groupName);

        public IClientProxy User(string userId) => To($"user:{userId}");

        // El hub solo difunde a grupos. El resto de las sobrecargas se
        // dejan sin usar a proposito: si alguna vez hacen falta, que falle
        // el test y no pase inadvertida.
        public IClientProxy AllExcept(IReadOnlyList<string> excluded) => throw new NotSupportedException();

        public IClientProxy Client(string connectionId) => throw new NotSupportedException();

        public IClientProxy Clients(IReadOnlyList<string> connections) => throw new NotSupportedException();

        public IClientProxy Groups(IReadOnlyList<string> groupNames) => throw new NotSupportedException();

        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excluded) =>
            throw new NotSupportedException();

        public IClientProxy Users(IReadOnlyList<string> userIds) => throw new NotSupportedException();

        public IClientProxy UserExcept(string userId, IReadOnlyList<string> excluded) =>
            throw new NotSupportedException();
    }

    private sealed class FakeClientProxy(FakeHubContext owner, string? group) : IClientProxy
    {
        public Task SendCoreAsync(string method, object?[] args, CancellationToken token = default)
        {
            owner.Deliver(group ?? "todos", method, args.Length > 0 ? args[0] : null);
            return Task.CompletedTask;
        }
    }
}
