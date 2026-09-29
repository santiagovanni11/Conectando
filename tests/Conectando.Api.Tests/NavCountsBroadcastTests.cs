using Conectando.Api.Data;
using Conectando.Api.Hubs;
using Conectando.Api.Hubs.Broadcasting;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Conectando.Api.Tests;

/// <summary>
/// Que los números de la barra se actualicen en el momento justo.
/// </summary>
/// <remarks>
/// El bug que esto cubre era que los contadores solo se refrescaban al entrar
/// y cada 30 segundos: las notificaciones y las solicitudes de amistad no
/// disparaban nada, y por eso un número tardaba en aparecer o se quedaba ahí
/// después de abrir la pantalla que lo tenía que borrar.
/// </remarks>
[Collection("Social")]
public class NavCountsBroadcastTests(SocialTestFixture fixture)
{
    private static (MessageHub Hub, FakeNavCountsBroadcaster Nav) CreateHub(
        ConectandoDbContext db,
        Guid userId)
    {
        var nav = new FakeNavCountsBroadcaster();
        var hub = new MessageHub(
            new ConversationService(db, new BlockService(db)),
            new ConversationBroadcaster(new FakeHubContext(), NullLogger<ConversationBroadcaster>.Instance),
            nav,
            NullLogger<MessageHub>.Instance)
        {
            Context = new FakeHubCaller(userId),
        };

        return (hub, nav);
    }

    [Fact]
    public async Task Enviar_mensaje_avisa_al_otro()
    {
        // Si no se avisa, el número de sin leer del otro recién se entera
        // treinta segundos después, cuando pasa el poll.
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var (hub, nav) = CreateHub(db, users[0].Id);

        await hub.SendMessage(conversation.Id, "Hola");

        Assert.True(nav.NotifiedTo(users[1].Id));
    }

    [Fact]
    public async Task Marcar_como_leido_avisa_al_que_leyo()
    {
        // Es el otro sentido del arreglo: abrir el chat tiene que bajar el
        // número ahí mismo.
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var (hub, nav) = CreateHub(db, users[0].Id);
        await hub.SendMessage(conversation.Id, "Hola");

        await hub.MarkAsRead(conversation.Id);

        Assert.True(nav.NotifiedTo(users[0].Id));
    }

    [Fact]
    public async Task El_aviso_manda_los_numeros_al_grupo_del_usuario()
    {
        // Al grupo de la persona y no al de la conversación: cada quien está
        // conectado entra a su grupo, así que un mensaje en un chat que no
        // estás mirando también tiene que mover tu número.
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 1);
        var context = new FakeHubContext();
        var broadcaster = new NavCountsBroadcaster(
            context,
            new NavCountService(db),
            NullLogger<NavCountsBroadcaster>.Instance);

        await broadcaster.NotifyAsync(users[0].Id);

        var entregado = Assert.Single(context.Deliveries);
        Assert.Equal($"user:{users[0].Id}", entregado.Group);
        Assert.Equal("NavCountsChanged", entregado.EventName);
    }

    [Fact]
    public async Task El_aviso_nunca_falla_cuando_la_entrega_se_rompe()
    {
        // El aviso va con lo ya guardado. Si el error subiera, el usuario
        // creería que su mensaje o su like no se guardaron, y lo volvería a
        // intentar. Perder un número se arregla recargando.
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 1);
        var context = new FakeHubContext { FailOnSend = true };
        var broadcaster = new NavCountsBroadcaster(
            context,
            new NavCountService(db),
            NullLogger<NavCountsBroadcaster>.Instance);

        await broadcaster.NotifyAsync(users[0].Id);
    }
}
