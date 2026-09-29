using Conectando.Api.Exceptions;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class BlockTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Block_RemovesFriendshipAndFollows()
    {
        await using var db = fixture.CreateContext();
        var (requests, friends, follows, blocks, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[0].Id, users[1].Id);
        await requests.AcceptAsync(users[1].Id, users[0].Id);
        await follows.FollowAsync(users[1].Id, users[0].Id);
        await follows.FollowAsync(users[0].Id, users[1].Id);

        await blocks.BlockAsync(users[0].Id, users[1].Id);

        var (a, b) = (users[0].Id, users[1].Id);
        var (low, high) = a.CompareTo(b) <= 0 ? (a, b) : (b, a);
        Assert.Single(await db.Blocks.AsNoTracking()
            .Where(x => x.UserId == a && x.BlockedUserId == b).ToListAsync());
        Assert.Empty(await db.Friendships.AsNoTracking()
            .Where(f => f.UserLowId == low && f.UserHighId == high).ToListAsync());
        Assert.Empty(await db.Follows.AsNoTracking()
            .Where(f => f.UserId == a || f.UserId == b).ToListAsync());
        Assert.Empty(await friends.GetFriendsAsync(users[0].Id));
    }

    [Fact]
    public async Task Block_RemovesPendingRequestsInBothDirections()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, _, blocks, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[0].Id, users[1].Id);

        await blocks.BlockAsync(users[1].Id, users[0].Id);

        Assert.Empty(await requests.GetSentAsync(users[0].Id));
        Assert.Empty(await requests.GetReceivedAsync(users[1].Id));
    }

    [Fact]
    public async Task Block_PreventsFollowsInBothDirections()
    {
        await using var db = fixture.CreateContext();
        var (_, _, follows, blocks, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await blocks.BlockAsync(users[0].Id, users[1].Id);

        await Assert.ThrowsAsync<BlockedActionException>(() => follows.FollowAsync(users[0].Id, users[1].Id));
        await Assert.ThrowsAsync<BlockedActionException>(() => follows.FollowAsync(users[1].Id, users[0].Id));
    }

    [Fact]
    public async Task Block_PreventsFriendRequestsInBothDirections()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, _, blocks, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await blocks.BlockAsync(users[0].Id, users[1].Id);

        await Assert.ThrowsAsync<BlockedActionException>(() => requests.SendAsync(users[0].Id, users[1].Id));
        await Assert.ThrowsAsync<BlockedActionException>(() => requests.SendAsync(users[1].Id, users[0].Id));
    }

    [Fact]
    public async Task Unblock_RestoresInteractionWithoutRelationships()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, follows, blocks, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await blocks.BlockAsync(users[0].Id, users[1].Id);
        await blocks.UnblockAsync(users[0].Id, users[1].Id);

        Assert.Empty(await blocks.GetBlocksAsync(users[0].Id));
        await requests.SendAsync(users[0].Id, users[1].Id);
        await follows.FollowAsync(users[0].Id, users[1].Id);
    }

    [Fact]
    public async Task Block_ToSelf_ThrowsSelfAction()
    {
        await using var db = fixture.CreateContext();
        var (_, _, _, blocks, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<SelfActionException>(() => blocks.BlockAsync(users[0].Id, users[0].Id));
    }

    [Fact]
    public async Task Block_Twice_IsIdempotentAndSymmetric()
    {
        await using var db = fixture.CreateContext();
        var (_, _, _, blocks, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await blocks.BlockAsync(users[0].Id, users[1].Id);
        await blocks.BlockAsync(users[0].Id, users[1].Id);

        Assert.Equal(1, await db.Blocks.AsNoTracking()
            .CountAsync(x => x.UserId == users[0].Id && x.BlockedUserId == users[1].Id));
        Assert.True(await blocks.IsBlockedAsync(users[0].Id, users[1].Id));
        Assert.True(await blocks.IsBlockedAsync(users[1].Id, users[0].Id));
    }

    [Fact]
    public async Task Block_Concurrently_LeavesSingleRow()
    {
        await using var seed = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(seed, 2);
        await using var dbA = fixture.CreateContext();
        await using var dbB = fixture.CreateContext();

        await Task.WhenAll(
            Task.Run(() => new BlockService(dbA).BlockAsync(users[0].Id, users[1].Id)),
            Task.Run(() => new BlockService(dbB).BlockAsync(users[0].Id, users[1].Id)));

        await using var check = fixture.CreateContext();
        Assert.Equal(1, await check.Blocks.AsNoTracking()
            .CountAsync(x => x.UserId == users[0].Id && x.BlockedUserId == users[1].Id));
    }
}