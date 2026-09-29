using Conectando.Api.Data;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class NavCountServiceTests(SocialTestFixture fixture)
{
    private NavCountService CreateService(ConectandoDbContext db) => new(db);

    [Fact]
    public async Task GetCounts_NewUser_ReturnsZero()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var user = await TestUsers.SeedAsync(db, 1);

        var counts = await service.GetCountsAsync(user[0].Id);

        Assert.Equal(0, counts.UnreadMessages);
        Assert.Equal(0, counts.PendingFriendRequests);
        Assert.Equal(0, counts.UnreadNotifications);
    }

    [Fact]
    public async Task GetCounts_CountsUnreadMessagesFromOthers()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[1].Id, "Hola");
        await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[1].Id, "Segundo");

        var counts = await service.GetCountsAsync(users[0].Id);

        Assert.Equal(2, counts.UnreadMessages);
    }

    [Fact]
    public async Task GetCounts_IgnoresOwnMessages()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, "Me lo mando");

        var counts = await service.GetCountsAsync(users[0].Id);

        Assert.Equal(0, counts.UnreadMessages);
    }

    [Fact]
    public async Task GetCounts_DropsMessagesOnceRead()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[1].Id, "Hola");

        await new ConversationService(db, new BlockService(db))
            .MarkAsReadAsync(users[0].Id, conversation.Id);

        var counts = await service.GetCountsAsync(users[0].Id);

        Assert.Equal(0, counts.UnreadMessages);
    }

    [Fact]
    public async Task GetCounts_CountsPendingRequestsAddressedToMe()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 3);

        // Una solicitud recibida por el usuario y otra que él envió.
        await PostTestData.SeedFriendshipAsync(db, users[0].Id, users[1].Id);
        db.FriendRequests.Add(new FriendRequest
        {
            RequesterId = users[1].Id,
            AddresseeId = users[0].Id,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var counts = await service.GetCountsAsync(users[0].Id);

        Assert.Equal(1, counts.PendingFriendRequests);
    }

    [Fact]
    public async Task GetCounts_NotifiesArePerUser()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);

        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientId = users[0].Id,
            ActorId = users[1].Id,
            Type = NotificationType.FriendRequest,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        Assert.Equal(1, (await service.GetCountsAsync(users[0].Id)).UnreadNotifications);
        Assert.Equal(0, (await service.GetCountsAsync(users[1].Id)).UnreadNotifications);
    }
}