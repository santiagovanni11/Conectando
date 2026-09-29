using Conectando.Api.Data;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class ConversationServiceWriteTests(SocialTestFixture fixture)
{
    private ConversationService CreateService(ConectandoDbContext db) =>
        new(db, new BlockService(db));

    [Fact]
    public async Task SendMessage_SavesAndUpdatesConversation()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        var result = await service.SendMessageAsync(users[0].Id, conversation.Id, "  Hola  ");

        Assert.Equal("Hola", result.Content);
        Assert.Equal(users[0].Id, result.Sender.Id);

        var saved = await db.Messages
            .SingleAsync(m => m.ConversationId == conversation.Id && m.Content == "Hola");
        Assert.Equal("Hola", saved.Content);

        // La conversación tiene que subir para reordenar la lista.
        var updated = await db.Conversations.AsNoTracking().SingleAsync(c => c.Id == conversation.Id);
        Assert.Equal(saved.CreatedAt, updated.UpdatedAt);
    }

    [Fact]
    public async Task StartDirect_Twice_ReturnsSameConversation()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);

        var first = await service.StartDirectAsync(users[0].Id, users[1].Id);
        var second = await service.StartDirectAsync(users[1].Id, users[0].Id);

        // Importante: da igual el orden desde el que se pida.
        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await db.Conversations.CountAsync(c => c.Id == first.Id));
    }

    [Fact]
    public async Task MarkAsRead_StoresLastReadAt()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        await service.MarkAsReadAsync(users[0].Id, conversation.Id);

        var member = await db.ConversationMembers
            .SingleAsync(m => m.ConversationId == conversation.Id && m.UserId == users[0].Id);

        Assert.NotNull(member.LastReadAt);
    }

    [Fact]
    public async Task SendMessage_FromEachUser_KeepsItsOwnSender()
    {
        // Reproduce el caso real: A manda, B responde, y cada mensaje
        // debe conservar quién lo mandó.
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        var deA = await service.SendMessageAsync(users[0].Id, conversation.Id, "Hola, soy A");
        var deB = await service.SendMessageAsync(users[1].Id, conversation.Id, "Hola, soy B");

        Assert.Equal(users[0].Id, deA.Sender.Id);
        Assert.Equal(users[1].Id, deB.Sender.Id);

        // Y el historial tiene que devolver cada uno con su autor.
        var historial = await service.GetMessagesAsync(users[0].Id, conversation.Id, null, 10);

        Assert.Equal(2, historial.Items.Count);
        Assert.Equal(users[1].Id, historial.Items[0].Sender.Id);
        Assert.Equal("Hola, soy B", historial.Items[0].Content);
        Assert.Equal(users[0].Id, historial.Items[1].Sender.Id);
        Assert.Equal("Hola, soy A", historial.Items[1].Content);
    }

    [Fact]
    public async Task MarkAsRead_OneUser_DoesNotAffectTheOther()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, "Hola");

        await service.MarkAsReadAsync(users[1].Id, conversation.Id);

        var leyoA = await db.ConversationMembers
            .SingleAsync(m => m.ConversationId == conversation.Id && m.UserId == users[0].Id);

        Assert.Null(leyoA.LastReadAt);
    }

    [Fact]
    public async Task MarkAsRead_DoesNotBlockTheOtherFromReading()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        await service.MarkAsReadAsync(users[1].Id, conversation.Id);

        // Marcar como leído no cambia los permisos: A todavía puede leer.
        var historial = await service.GetMessagesAsync(users[0].Id, conversation.Id, null, 10);
        Assert.Empty(historial.Items);
    }
}