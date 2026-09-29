using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class FriendRequestAcceptTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Accept_CreatesFriendshipAndRemovesRequest()
    {
        await using var db = fixture.CreateContext();
        var (requests, friends, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[0].Id, users[1].Id);
        await requests.AcceptAsync(users[1].Id, users[0].Id);

        Assert.Empty(await requests.GetReceivedAsync(users[1].Id));
        Assert.Empty(await requests.GetSentAsync(users[0].Id));
        Assert.Single(await friends.GetFriendsAsync(users[0].Id));
        Assert.Single(await friends.GetFriendsAsync(users[1].Id));
    }

    [Fact]
    public async Task Accept_WhenAlreadyResolvedByIdempotent()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[0].Id, users[1].Id);
        await requests.AcceptAsync(users[1].Id, users[0].Id);

        await requests.AcceptAsync(users[1].Id, users[0].Id);

        var (l, h) = users[0].Id.CompareTo(users[1].Id) <= 0 ? (users[0].Id, users[1].Id) : (users[1].Id, users[0].Id);
        Assert.Single(await db.Friendships.AsNoTracking()
            .Where(f => f.UserLowId == l && f.UserHighId == h).ToListAsync());
    }

    [Fact]
    public async Task Accept_ByRequester_ThrowsNotOwner()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[0].Id, users[1].Id);

        await Assert.ThrowsAsync<NotRequestOwnerException>(() => requests.AcceptAsync(users[0].Id, users[0].Id));
    }

    [Fact]
    public async Task Accept_ByUnrelatedThirdParty_ThrowsNotFound()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 3);

        await requests.SendAsync(users[0].Id, users[1].Id);

        await Assert.ThrowsAsync<FriendRequestNotFoundException>(() => requests.AcceptAsync(users[2].Id, users[0].Id));
    }

    [Fact]
    public async Task Accept_Concurrently_LeavesSingleFriendship()
    {
        var (alice, bob) = (TestUsers.Create(), TestUsers.Create());
        await using var setup = fixture.CreateContext();
        setup.Users.AddRange(alice, bob);
        setup.FriendRequests.Add(new FriendRequest { RequesterId = alice.Id, AddresseeId = bob.Id });
        await setup.SaveChangesAsync();
        await using var dbA = fixture.CreateContext();
        await using var dbB = fixture.CreateContext();

        await Task.WhenAll(
            Task.Run(() => AcceptTracked(dbA, alice.Id, bob.Id)),
            Task.Run(() => AcceptTracked(dbB, alice.Id, bob.Id)));

        var (l, h) = alice.Id.CompareTo(bob.Id) <= 0 ? (alice.Id, bob.Id) : (bob.Id, alice.Id);
        Assert.Equal(1, await dbA.Friendships.AsNoTracking()
            .CountAsync(f => f.UserLowId == l && f.UserHighId == h));
        Assert.Empty(await dbA.FriendRequests.AsNoTracking()
            .Where(r => r.RequesterId == alice.Id && r.AddresseeId == bob.Id).ToListAsync());
    }

    private static async Task AcceptTracked(ConectandoDbContext db, Guid requesterId, Guid addresseeId)
    {
        var requests = new FriendRequestService(db, new BlockService(db), new NullNotificationService());
        try
        {
            await requests.AcceptAsync(addresseeId, requesterId);
        }
        catch (AlreadyFriendsException)
        {
        }
    }
}