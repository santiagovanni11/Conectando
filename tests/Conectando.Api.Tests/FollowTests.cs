using Conectando.Api.Exceptions;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class FollowTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Follow_AppearsInFollowingAndFollowers()
    {
        await using var db = fixture.CreateContext();
        var (_, _, follows, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await follows.FollowAsync(users[0].Id, users[1].Id);

        var following = await follows.GetFollowingAsync(users[0].Id);
        var followers = await follows.GetFollowersAsync(users[1].Id);
        Assert.Single(following);
        Assert.Single(followers);
        Assert.Equal(users[1].Id, following[0].User.Id);
        Assert.Equal(users[0].Id, followers[0].User.Id);
    }

    [Fact]
    public async Task Follow_Twice_IsIdempotent()
    {
        await using var db = fixture.CreateContext();
        var (_, _, follows, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await follows.FollowAsync(users[0].Id, users[1].Id);
        await follows.FollowAsync(users[0].Id, users[1].Id);

        Assert.Equal(1, await db.Follows.AsNoTracking()
            .CountAsync(f => f.UserId == users[0].Id && f.TargetUserId == users[1].Id));
    }

    [Fact]
    public async Task Follow_Concurrently_LeavesSingleRow()
    {
        await using var seed = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(seed, 2);
        await using var dbA = fixture.CreateContext();
        await using var dbB = fixture.CreateContext();

        await Task.WhenAll(
            Task.Run(() => new FollowService(dbA, new BlockService(dbA), new NullNotificationService()).FollowAsync(users[0].Id, users[1].Id)),
            Task.Run(() => new FollowService(dbB, new BlockService(dbB), new NullNotificationService()).FollowAsync(users[0].Id, users[1].Id)));

        await using var check = fixture.CreateContext();
        Assert.Equal(1, await check.Follows.AsNoTracking()
            .CountAsync(f => f.UserId == users[0].Id && f.TargetUserId == users[1].Id));
    }

    [Fact]
    public async Task Follow_ToSelf_ThrowsSelfAction()
    {
        await using var db = fixture.CreateContext();
        var (_, _, follows, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<SelfActionException>(() => follows.FollowAsync(users[0].Id, users[0].Id));
    }

    [Fact]
    public async Task Follow_UnknownTarget_ThrowsNotFound()
    {
        await using var db = fixture.CreateContext();
        var (_, _, follows, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<UserNotFoundException>(() => follows.FollowAsync(users[0].Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task Unfollow_IsIdempotent()
    {
        await using var db = fixture.CreateContext();
        var (_, _, follows, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await follows.FollowAsync(users[0].Id, users[1].Id);
        await follows.UnfollowAsync(users[0].Id, users[1].Id);
        await follows.UnfollowAsync(users[0].Id, users[1].Id);

        Assert.Empty(await follows.GetFollowingAsync(users[0].Id));
    }
}