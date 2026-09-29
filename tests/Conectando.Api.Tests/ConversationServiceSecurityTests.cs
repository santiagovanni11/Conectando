using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class ConversationServiceSecurityTests(SocialTestFixture fixture)
{
    private ConversationService CreateService(ConectandoDbContext db) =>
        new(db, new BlockService(db));

    [Fact]
    public async Task GetMessages_NotAMember_ThrowsConversationNotFound()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 3);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        // El tercero no participa: no debe poder leer ni aunque conozca el id.
        await Assert.ThrowsAsync<ConversationNotFoundException>(
            () => service.GetMessagesAsync(users[2].Id, conversation.Id, null, 30));
    }

    [Fact]
    public async Task SendMessage_NotAMember_ThrowsConversationNotFound()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 3);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        await Assert.ThrowsAsync<ConversationNotFoundException>(
            () => service.SendMessageAsync(users[2].Id, conversation.Id, "hola"));

        Assert.Empty(await db.Messages.Where(m => m.ConversationId == conversation.Id).ToListAsync());
    }

    [Fact]
    public async Task SendMessage_ByBlockedUser_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        await BlockTestData.SeedBlockAsync(db, blockerId: users[0].Id, blockedId: users[1].Id);

        await Assert.ThrowsAsync<BlockedActionException>(
            () => service.SendMessageAsync(users[1].Id, conversation.Id, "hola"));
    }

    [Fact]
    public async Task SendMessage_EmptyContent_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        await Assert.ThrowsAsync<EmptyMessageException>(
            () => service.SendMessageAsync(users[0].Id, conversation.Id, "   "));
    }

    [Fact]
    public async Task SendMessage_TooLong_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        await Assert.ThrowsAsync<MessageTooLongException>(
            () => service.SendMessageAsync(users[0].Id, conversation.Id, new string('a', 2001)));
    }

    [Fact]
    public async Task StartDirect_WithSelf_ThrowsSelfAction()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<SelfActionException>(
            () => service.StartDirectAsync(users[0].Id, users[0].Id));
    }
}