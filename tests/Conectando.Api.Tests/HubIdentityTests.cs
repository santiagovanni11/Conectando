using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Conectando.Api.Hubs;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;

namespace Conectando.Api.Tests;

/// <summary>
/// Cómo lee el hub la identidad del token.
/// </summary>
/// <remarks>
/// El bug que estos tests cubren era invisible desde el navegador: la página
/// cargaba, el login funcionaba, y el WebSocket moría solo un instante después
/// de conectarse, sin dejar error en pantalla. La app entera andaba; solo el
/// tiempo real estaba muerto.
/// </remarks>
public class HubIdentityTests
{
    private sealed class TestHub : ConectandoHub
    {
        public TestHub() : base(NullLogger.Instance) { }

        public Guid UserId => GetUserId();

        public string? Name => GetDisplayName();
    }

    /// <summary>Contexto mínimo: al hub solo le interesan los claims del usuario.</summary>
    private sealed class TestContext(ClaimsPrincipal user) : HubCallerContext
    {
        public override string ConnectionId => "c1";

        public override string? UserIdentifier =>
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public override ClaimsPrincipal? User { get; } = user;

        public override IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();

        public override IFeatureCollection Features { get; } = new FeatureCollection();

        public override CancellationToken ConnectionAborted => CancellationToken.None;

        public override void Abort() { }
    }

    private static TestHub Hub(params Claim[] claims)
    {
        var hub = new TestHub();
        hub.Context = new TestContext(new ClaimsPrincipal(new ClaimsIdentity(claims, "test")));
        return hub;
    }

    [Fact]
    public void Lee_el_identificador_del_claim_nameid()
    {
        var id = Guid.NewGuid();
        var hub = Hub(new Claim(ClaimTypes.NameIdentifier, id.ToString()));

        Assert.Equal(id, hub.UserId);
    }

    [Fact]
    public void Lee_el_identificador_del_claim_sub_cuando_no_hay_nameid()
    {
        // El token viaja con los claims cortos de JWT. Si el servidor no los
        // mapea al esquema de ASP.NET, nameid no existe: buscar solo ese
        // claim rompe la conexión en OnConnectedAsync.
        var id = Guid.NewGuid();
        var hub = Hub(new Claim(JwtRegisteredClaimNames.Sub, id.ToString()));

        Assert.Equal(id, hub.UserId);
    }

    [Fact]
    public void El_token_sin_identificador_falla_con_un_error_claro()
    {
        // Mejor una excepción con mensaje que un NullReferenceException en
        // vivo: el primero dice qué pasó, el segundo solo corta el socket.
        var hub = Hub(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

        var error = Assert.Throws<ArgumentException>(() => hub.UserId);

        Assert.Contains("identificador de usuario", error.Message);
    }

    [Fact]
    public void Lee_el_nombre_visible_sin_romperse_si_no_esta()
    {
        // El indicador de "está escribiendo" tolerate no saber el nombre.
        Assert.Null(Hub(new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())).Name);
    }
}
