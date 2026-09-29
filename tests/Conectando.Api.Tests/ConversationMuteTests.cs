using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class ConversationMuteTests(SocialTestFixture fixture)
{
    private ConversationService CreateService(ConectandoDbContext db) =>
        new(db, new BlockService(db));

    [Fact]
    public async Task SetMuted_MarksTheMembership()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        var result = await service.SetMutedAsync(users[0].Id, conversation.Id, true);

        Assert.True(result);
        var member = await db.ConversationMembers
            .FirstAsync(m => m.ConversationId == conversation.Id && m.UserId == users[0].Id);
        Assert.NotNull(member.MutedAt);
    }

    [Fact]
    public async Task SetMuted_False_ClearsTheMute()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        await service.SetMutedAsync(users[0].Id, conversation.Id, true);
        var result = await service.SetMutedAsync(users[0].Id, conversation.Id, false);

        Assert.False(result);
        var member = await db.ConversationMembers
            .FirstAsync(m => m.ConversationId == conversation.Id && m.UserId == users[0].Id);
        Assert.Null(member.MutedAt);
    }

    [Fact]
    public async Task SetMuted_IsPerUser()
    {
        // Silenciar es personal: el otro sigue recibiendo su notificación.
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        await service.SetMutedAsync(users[0].Id, conversation.Id, true);

        var other = await db.ConversationMembers.FirstAsync(
            m => m.ConversationId == conversation.Id && m.UserId == users[1].Id);
        Assert.Null(other.MutedAt);
    }

    [Fact]
    public async Task SetMuted_NotAMember_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 3);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        await Assert.ThrowsAsync<ConversationNotFoundException>(
            () => service.SetMutedAsync(users[2].Id, conversation.Id, true));
    }

    [Fact]
    public async Task GetConversations_ReportsTheMuteState()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        await service.SetMutedAsync(users[0].Id, conversation.Id, true);

        var list = await service.GetConversationsAsync(users[0].Id);
        var found = list.First(c => c.Id == conversation.Id);

        Assert.True(found.IsMuted);
    }
}