using Conectando.Api.Data;
using Conectando.Api.Hubs;
using Conectando.Api.Hubs.Broadcasting;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Conectando.Api.Tests;

/// <summary>
/// El contrato del hub: que el cliente pueda distinguir lo que funciono de
/// lo que fallo.
/// </summary>
/// <remarks>
/// Antes el hub se tragaba los fallos y los mandaba como un evento aparte.
/// Para el que llama eso es indistinguible de exito: <c>invoke</c> se
/// resuelve y el front cree que guardo. Estos tests fijan la regla nueva:
/// si el metodo vuelve, se guardo.
/// </remarks>
[Collection("Social")]
public class MessageHubTests(SocialTestFixture fixture)
{
    private static (MessageHub Hub, FakeHubContext Context, FakeNavCountsBroadcaster Nav) CreateHub(
        ConectandoDbContext db,
        Guid userId)
    {
        var context = new FakeHubContext();
        var broadcaster = new ConversationBroadcaster(
            context,
            NullLogger<ConversationBroadcaster>.Instance);
        var nav = new FakeNavCountsBroadcaster();

        var hub = new MessageHub(
            new ConversationService(db, new BlockService(db)),
            broadcaster,
            nav,
            NullLogger<MessageHub>.Instance)
        {
            Context = new FakeHubCaller(userId),
        };

        return (hub, context, nav);
    }

    private static string GroupOf(Guid conversationId) => $"conversation:{conversationId}";

    [Fact]
    public async Task EditMessage_DifundeElCambioAlGrupoDeLaConversacion()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var message = await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, "Viejo");
        var (hub, context, _) = CreateHub(db, users[0].Id);

        await hub.EditMessage(conversation.Id, message.Id, "Nuevo");

        var sent = Assert.Single(context.Deliveries);
        Assert.Equal(GroupOf(conversation.Id), sent.Group);
        Assert.Equal("MessageUpdated", sent.EventName);
    }

    [Fact]
    public async Task EditMessage_PasadaLaVentana_LanzaParaQueElClienteLoSepa()
    {
        // Regresion: el hub se comia la excepcion y mandaba un evento de
        // error aparte, asi que invoke() resolvia y el front cerraba el
        // formulario de edicion creyendo que se habia guardado.
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var message = await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, "Viejo");

        message.CreatedAt = DateTime.UtcNow.AddMinutes(-20);
        await db.SaveChangesAsync();

        var (hub, context, _) = CreateHub(db, users[0].Id);

        await Assert.ThrowsAnyAsync<Exception>(() => hub.EditMessage(conversation.Id, message.Id, "Nuevo"));
        Assert.Empty(context.Deliveries);
    }

    [Fact]
    public async Task EditMessage_MensajeDeOtro_Lanza()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var message = await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, "Suyo");
        var (hub, _, _) = CreateHub(db, users[1].Id);

        await Assert.ThrowsAnyAsync<Exception>(() => hub.EditMessage(conversation.Id, message.Id, "Intruso"));
    }

    [Fact]
    public async Task DeleteMessage_DifundeElBorrado()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var message = await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, "Hola");
        var (hub, context, _) = CreateHub(db, users[0].Id);

        await hub.DeleteMessage(conversation.Id, message.Id);

        Assert.Equal("MessageDeleted", Assert.Single(context.Deliveries).EventName);
    }

    [Fact]
    public async Task SendMessage_DifundeAlGrupoEntero()
    {
        // Al grupo entero y no "a los otros": solo el emisor conoce su
        // connectionId, asi que excluirlo exigiria conocer el del otro.
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var (hub, context, _) = CreateHub(db, users[0].Id);

        await hub.SendMessage(conversation.Id, "Hola");

        var sent = Assert.Single(context.Deliveries);
        Assert.Equal(GroupOf(conversation.Id), sent.Group);
        Assert.Equal("MessageReceived", sent.EventName);
    }

    [Fact]
    public async Task SiLaEntregaFalla_NoSeReportaComoOperacionFallida()
    {
        // La escritura ya quedo guardada. Si el error de la entrega subiera,
        // el front caeria al REST y guardaria el mismo mensaje dos veces.
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var (hub, context, _) = CreateHub(db, users[0].Id);
        context.FailOnSend = true;

        await hub.SendMessage(conversation.Id, "Hola");

        var saved = await db.Messages.CountAsync(m => m.ConversationId == conversation.Id);
        Assert.Equal(1, saved);
    }
}
