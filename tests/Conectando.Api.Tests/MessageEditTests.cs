using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

/// <summary>Edición de mensajes propios.</summary>
[Collection("Social")]
public class MessageEditTests(SocialTestFixture fixture)
{
    private ConversationService CreateService(ConectandoDbContext db) =>
        new(db, new BlockService(db));

    [Fact]
    public async Task Edit_OwnMessage_UpdatesTextAndMarksEdited()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var message = await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, "Texto viejo");

        var result = await service.EditMessageAsync(users[0].Id, conversation.Id, message.Id, "  Texto nuevo  ");

        Assert.Equal("Texto nuevo", result.Content);
        Assert.True(result.IsEdited);
        Assert.False(result.IsDeleted);
    }

    [Fact]
    public async Task Edit_OtherUsersMessage_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var message = await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, "Suyo");

        await Assert.ThrowsAsync<MessageOwnershipException>(
            () => service.EditMessageAsync(users[1].Id, conversation.Id, message.Id, "Mío"));
    }

    [Fact]
    public async Task Edit_EmptyContent_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var message = await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, "Hola");

        await Assert.ThrowsAsync<EmptyMessageException>(
            () => service.EditMessageAsync(users[0].Id, conversation.Id, message.Id, "   "));
    }

    [Fact]
    public async Task Edit_AfterWindow_Throws()
    {
        // Ventana de 15 minutos como en WhatsApp: si no, se podría
        // reescribir la conversación a posteriori.
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var message = await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, "Viejo");

        var stored = await db.Messages.FirstAsync(m => m.Id == message.Id);
        stored.CreatedAt = DateTime.UtcNow.AddHours(-2);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<MessageEditExpiredException>(
            () => service.EditMessageAsync(users[0].Id, conversation.Id, message.Id, "Nuevo"));
    }

    [Fact]
    public async Task Edit_MessageOfAnotherConversation_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var otra = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var message = await ConversationTestData.SeedMessageAsync(db, otra.Id, users[0].Id, "Este");

        // Falla antes por no pertenecer a esa conversación: el id del
        // mensaje nunca alcanza para tocar mensajes de otro lado.
        await Assert.ThrowsAsync<ConversationNotFoundException>(
            () => service.EditMessageAsync(users[0].Id, Guid.NewGuid(), message.Id, "Nuevo"));
    }
}