using Conectando.Api.Data;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;

namespace Conectando.Api.Tests;

/// <summary>
/// Listado del hilo con citas.
///
/// Va aparte de los tests de envío porque es otra pregunta: no si la cita se
/// arma bien, sino si la consulta que la trae deja de perder mensajes.
/// </summary>
[Collection("Social")]
public class MessageReplyListingTests(SocialTestFixture fixture)
{
    private ConversationService CreateService(ConectandoDbContext db) =>
        new(db, new BlockService(db));

    [Fact]
    public async Task GetMessages_ReturnsMessagesWithoutReply()
    {
        // La pregunta clave: un mensaje que no cita a nadie tiene que
        // aparecer igual. Agregar la cita a la proyección es el momento
        // exacto en que se puede perder toda la historia previa.
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var chat = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        await ConversationTestData.SeedMessageAsync(db, chat.Id, users[0].Id, "primero");
        await ConversationTestData.SeedMessageAsync(db, chat.Id, users[1].Id, "segundo");
        await ConversationTestData.SeedMessageAsync(db, chat.Id, users[0].Id, "tercero");

        var page = await service.GetMessagesAsync(users[0].Id, chat.Id, null, 30);

        Assert.Equal(3, page.Items.Count);
        Assert.All(page.Items, m => Assert.Null(m.ReplyTo));
    }

    [Fact]
    public async Task GetMessages_MixesMessagesWithAndWithoutReply()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var chat = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        var primero = await ConversationTestData.SeedMessageAsync(db, chat.Id, users[1].Id, "hola");
        await ConversationTestData.SeedMessageAsync(db, chat.Id, users[0].Id, "respondo", primero.Id);
        await ConversationTestData.SeedMessageAsync(db, chat.Id, users[1].Id, "chau");

        var page = await service.GetMessagesAsync(users[0].Id, chat.Id, null, 30);

        Assert.Equal(3, page.Items.Count);
        var conCita = page.Items.Single(m => m.Content == "respondo");
        Assert.NotNull(conCita.ReplyTo);
        Assert.Equal(primero.Id, conCita.ReplyTo.Id);
    }

    [Fact]
    public async Task GetMessages_PaginatesWithTheReplyJoin()
    {
        // El join de la cita no puede alterar el límite: si la consulta
        // filtra o duplica, la página del cursor trae mensajes que no
        // corresponden al rango, y "cargar más" da mensajes repetidos.
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var chat = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        for (var i = 0; i < 6; i++)
        {
            await ConversationTestData.SeedMessageAsync(db, chat.Id, users[0].Id, $"m{i}");
        }

        var primera = await service.GetMessagesAsync(users[0].Id, chat.Id, null, 3);
        var segunda = await service.GetMessagesAsync(users[0].Id, chat.Id, primera.NextCursor, 3);

        Assert.Equal(3, primera.Items.Count);
        Assert.Equal(3, segunda.Items.Count);
        Assert.Empty(primera.Items.Select(m => m.Id).Intersect(segunda.Items.Select(m => m.Id)));
    }
}
