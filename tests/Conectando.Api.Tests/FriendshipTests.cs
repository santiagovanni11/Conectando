using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class FriendshipTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Accept_CreatesSingleNormalizedRowVisibleToBoth()
    {
        await using var db = fixture.CreateContext();
        var (requests, friends, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[0].Id, users[1].Id);
        await requests.AcceptAsync(users[1].Id, users[0].Id);

        var (l, h) = users[0].Id.CompareTo(users[1].Id) <= 0 ? (users[0].Id, users[1].Id) : (users[1].Id, users[0].Id);
        var row = Assert.Single(await db.Friendships.AsNoTracking()
            .Where(f => f.UserLowId == l && f.UserHighId == h).ToListAsync());
        Assert.True(row.UserLowId < row.UserHighId);

        Assert.Single(await friends.GetFriendsAsync(users[0].Id));
        Assert.Single(await friends.GetFriendsAsync(users[1].Id));
    }

    [Fact]
    public async Task ReversedInsert_FailsAtDatabase()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var (_, friends, _, _, _) = SocialServices.Create(db);

        await friends.RemoveFriendAsync(users[0].Id, users[1].Id);

        var (a, b) = (users[0].Id, users[1].Id);
        var (low, high) = a.CompareTo(b) <= 0 ? (a, b) : (b, a);
        db.Friendships.Add(new Friendship { UserLowId = low, UserHighId = high });

        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            db.Friendships.Add(new Friendship { UserLowId = high, UserHighId = low });
            await db.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task RemoveFriend_DeletesAndIsIdempotent()
    {
        await using var db = fixture.CreateContext();
        var (requests, friends, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[0].Id, users[1].Id);
        await requests.AcceptAsync(users[1].Id, users[0].Id);
        await friends.RemoveFriendAsync(users[0].Id, users[1].Id);

        Assert.Empty(await friends.GetFriendsAsync(users[0].Id));

        await friends.RemoveFriendAsync(users[0].Id, users[1].Id);

        Assert.Empty(await friends.GetFriendsAsync(users[0].Id));
    }

    [Fact]
    public async Task GetFriends_ForFreshUser_ReturnsEmpty()
    {
        await using var db = fixture.CreateContext();
        var (_, friends, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        Assert.Empty(await friends.GetFriendsAsync(users[0].Id));
    }

    [Fact]
    public async Task RemoveFriend_ToSelf_ThrowsSelfAction()
    {
        await using var db = fixture.CreateContext();
        var (_, friends, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<SelfActionException>(() => friends.RemoveFriendAsync(users[0].Id, users[0].Id));
    }
}