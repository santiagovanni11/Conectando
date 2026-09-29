using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

/// <summary>
/// Borrado de mensajes y de chats. El borrado es lógico: el texto deja de
/// enviarse en lugar de desaparecer la fila.
/// </summary>
[Collection("Social")]
public class MessageDeleteTests(SocialTestFixture fixture)
{
    private ConversationService CreateService(ConectandoDbContext db) =>
        new(db, new BlockService(db));

    [Fact]
    public async Task Delete_ClearsContentForEveryone()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var message = await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, "Secreto");

        var result = await service.DeleteMessageAsync(users[0].Id, conversation.Id, message.Id);

        Assert.True(result.IsDeleted);
        // Ni el autor lo conserva: el texto deja de existir.
        Assert.Equal(string.Empty, result.Content);

        var stored = await db.Messages.AsNoTracking().FirstAsync(m => m.Id == message.Id);
        Assert.True(stored.IsDeleted);
        Assert.Equal(string.Empty, stored.Content);
    }

    [Fact]
    public async Task Delete_MessageOfOtherUser_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var message = await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, "Suyo");

        await Assert.ThrowsAsync<MessageOwnershipException>(
            () => service.DeleteMessageAsync(users[1].Id, conversation.Id, message.Id));
    }

    [Fact]
    public async Task Delete_MessageOfAnotherConversation_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var otra = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var message = await ConversationTestData.SeedMessageAsync(db, otra.Id, users[0].Id, "Este");

        // Primero falla la pertenencia a la conversación.
        await Assert.ThrowsAsync<ConversationNotFoundException>(
            () => service.DeleteMessageAsync(users[0].Id, Guid.NewGuid(), message.Id));
    }

    [Fact]
    public async Task Delete_MessageNotInConversation_Throws()
    {
        // Estando en la conversación, un id de mensaje que no es de ella
        // tampoco alcanza: esa comprobación es la segunda barrera.
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var otra = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var ajena = await ConversationTestData.SeedMessageAsync(db, otra.Id, users[0].Id, "De otro hilo");

        await Assert.ThrowsAsync<MessageNotFoundException>(
            () => service.DeleteMessageAsync(users[0].Id, conversation.Id, ajena.Id));
    }

    [Fact]
    public async Task GetMessages_DeletedMessage_ShowsNoContent()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var message = await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, "Secreto");
        await service.DeleteMessageAsync(users[0].Id, conversation.Id, message.Id);

        var page = await service.GetMessagesAsync(users[1].Id, conversation.Id, null, 10);

        var dto = Assert.Single(page.Items);
        Assert.True(dto.IsDeleted);
        Assert.Equal(string.Empty, dto.Content);
    }

    [Fact]
    public async Task DeleteConversation_HidesItOnlyForThatUser()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, "Hola");

        await service.DeleteConversationAsync(users[0].Id, conversation.Id);

        Assert.Empty(await service.GetConversationsAsync(users[0].Id));
        // La otra persona lo conserva: no se le borra nada.
        Assert.Single(await service.GetConversationsAsync(users[1].Id));
    }

    [Fact]
    public async Task SendMessage_BringBackADeletedChat()
    {
        // Si el otro escribe, el chat reaparece para quien lo había borrado.
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        await service.DeleteConversationAsync(users[0].Id, conversation.Id);

        await service.SendMessageAsync(users[1].Id, conversation.Id, "Volví");

        Assert.Single(await service.GetConversationsAsync(users[0].Id));
    }
}