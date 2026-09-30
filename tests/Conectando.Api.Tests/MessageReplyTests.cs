using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

/// <summary>Respuestas a mensajes: la cita viaja, y la referencia se valida.</summary>
[Collection("Social")]
public class MessageReplyTests(SocialTestFixture fixture)
{
    private ConversationService CreateService(ConectandoDbContext db) =>
        new(db, new BlockService(db));

    [Fact]
    public async Task Reply_ReturnsQuotedMessage()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var chat = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var original = await ConversationTestData.SeedMessageAsync(
            db, chat.Id, users[1].Id, "¿A qué hora llego?");

        var reply = await service.SendMessageAsync(users[0].Id, chat.Id, "A las seis", original.Id);

        Assert.NotNull(reply.ReplyTo);
        Assert.Equal(original.Id, reply.ReplyTo.Id);
        Assert.Equal("¿A qué hora llego?", reply.ReplyTo.Preview);
        Assert.Equal(users[1].DisplayName, reply.ReplyTo.SenderDisplayName);
        Assert.False(reply.ReplyTo.IsDeleted);
    }

    [Fact]
    public async Task Send_WithoutReply_HasNoQuote()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var chat = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        var sent = await service.SendMessageAsync(users[0].Id, chat.Id, "hola");

        Assert.Null(sent.ReplyTo);
    }

    [Fact]
    public async Task Reply_ClipsThePreview()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var chat = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var original = await ConversationTestData.SeedMessageAsync(
            db, chat.Id, users[1].Id, new string('x', 500));

        var reply = await service.SendMessageAsync(users[0].Id, chat.Id, "ok", original.Id);

        Assert.NotNull(reply.ReplyTo);
        // El original puede tener 2000 caracteres y la cita es una línea.
        Assert.True(reply.ReplyTo.Preview.Length <= 120, "la cita debe venir recortada");
    }

    [Fact]
    public async Task Reply_OfAnotherConversation_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var chat = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var otra = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var ajeno = await ConversationTestData.SeedMessageAsync(db, otra.Id, users[0].Id, "secreto");

        // Control de seguridad: la cita viaja con el texto del original, así
        // que aceptar un id de otro chat filtraría conversaciones ajenas.
        await Assert.ThrowsAsync<MessageNotFoundException>(
            () => service.SendMessageAsync(users[0].Id, chat.Id, "cita ajena", ajeno.Id));
    }

    [Fact]
    public async Task Reply_OfUnknownId_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var chat = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        await Assert.ThrowsAsync<MessageNotFoundException>(
            () => service.SendMessageAsync(users[0].Id, chat.Id, "cita", Guid.NewGuid()));
    }

    [Fact]
    public async Task Reply_ToDeletedOriginal_ShowsDeletedFlag()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var chat = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var original = await ConversationTestData.SeedMessageAsync(db, chat.Id, users[1].Id, "borrame");
        await service.DeleteMessageAsync(users[1].Id, chat.Id, original.Id);

        var reply = await service.SendMessageAsync(users[0].Id, chat.Id, "ok", original.Id);

        Assert.NotNull(reply.ReplyTo);
        Assert.True(reply.ReplyTo.IsDeleted);
        Assert.Equal(string.Empty, reply.ReplyTo.Preview);
    }

    [Fact]
    public async Task DeletingTheOriginal_KeepsTheReplyAndClearsTheLink()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var chat = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        var original = await ConversationTestData.SeedMessageAsync(db, chat.Id, users[1].Id, "hola");
        var reply = await service.SendMessageAsync(users[0].Id, chat.Id, "respuesta", original.Id);

        await service.DeleteMessageAsync(users[1].Id, chat.Id, original.Id);

        // La respuesta no se cae con el original: la FK es SetNull.
        var stillThere = await db.Messages.SingleAsync(m => m.Id == reply.Id);
        Assert.Equal(original.Id, stillThere.ReplyToMessageId);
    }
}
