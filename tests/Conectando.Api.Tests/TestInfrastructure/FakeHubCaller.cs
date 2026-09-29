using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace Conectando.Api.Tests.TestInfrastructure;

/// <summary>
/// El contexto de quien llama, con la identidad que el hub lee del token.
/// </summary>
/// <remarks>
/// El hub saca el usuario del <c>NameIdentifier</c> de los claims, asi que
/// sin esto no se puede ni siquiera exercises un metodo.
/// </remarks>
public sealed class FakeHubCaller(Guid userId) : HubCallerContext
{
    public override string ConnectionId => "test-connection";

    public override string? UserIdentifier => userId.ToString();

    public override ClaimsPrincipal User { get; } = new(
        new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            authenticationType: "test"));


    public override IFeatureCollection Features { get; } = new FeatureCollection();

    public override IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();

    public override CancellationToken ConnectionAborted => CancellationToken.None;

    public override void Abort()
    {
    }
}
