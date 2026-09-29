using Conectando.Api.Exceptions;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class SocialStatusTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Status_DefaultState_IsNone()
    {
        await using var db = fixture.CreateContext();
        var (_, _, _, _, status) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        var result = await status.GetStatusAsync(users[0].Id, users[1].Id);

        Assert.Equal("none", result.Friendship);
        Assert.False(result.Following);
        Assert.False(result.FollowedBy);
        Assert.False(result.BlockedByMe);
        Assert.False(result.BlockedByThem);
    }

    [Fact]
    public async Task Status_ReportsRequestDirections()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, _, _, status) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[0].Id, users[1].Id);

        Assert.Equal("sent", (await status.GetStatusAsync(users[0].Id, users[1].Id)).Friendship);
        Assert.Equal("received", (await status.GetStatusAsync(users[1].Id, users[0].Id)).Friendship);
    }

    [Fact]
    public async Task Status_ReportsFriendshipAndFollowFlags()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, follows, _, status) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[0].Id, users[1].Id);
        await requests.AcceptAsync(users[1].Id, users[0].Id);
        await follows.FollowAsync(users[0].Id, users[1].Id);

        var fromFollowee = await status.GetStatusAsync(users[0].Id, users[1].Id);
        var fromFollowed = await status.GetStatusAsync(users[1].Id, users[0].Id);

        Assert.Equal("friends", fromFollowee.Friendship);
        Assert.True(fromFollowee.Following);
        Assert.False(fromFollowee.FollowedBy);
        Assert.True(fromFollowed.FollowedBy);
    }

    [Fact]
    public async Task Status_ReportsBlockDirections()
    {
        await using var db = fixture.CreateContext();
        var (_, _, _, blocks, status) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await blocks.BlockAsync(users[0].Id, users[1].Id);

        var blockerView = await status.GetStatusAsync(users[0].Id, users[1].Id);
        var blockedView = await status.GetStatusAsync(users[1].Id, users[0].Id);

        Assert.True(blockerView.BlockedByMe);
        Assert.True(blockedView.BlockedByThem);
    }

    [Fact]
    public async Task Status_ToSelf_ThrowsSelfAction()
    {
        await using var db = fixture.CreateContext();
        var (_, _, _, _, status) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<SelfActionException>(() => status.GetStatusAsync(users[0].Id, users[0].Id));
    }

    [Fact]
    public async Task Status_UnknownTarget_ThrowsNotFound()
    {
        await using var db = fixture.CreateContext();
        var (_, _, _, _, status) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<UserNotFoundException>(() => status.GetStatusAsync(users[0].Id, Guid.NewGuid()));
    }
}