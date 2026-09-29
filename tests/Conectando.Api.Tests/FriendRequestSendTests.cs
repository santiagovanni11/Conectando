using Conectando.Api.Exceptions;
using Conectando.Api.Tests.TestInfrastructure;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class FriendRequestSendTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Send_CreatesPendingRequestVisibleOnBothSides()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[0].Id, users[1].Id);

        var sent = await requests.GetSentAsync(users[0].Id);
        var received = await requests.GetReceivedAsync(users[1].Id);
        Assert.Single(sent);
        Assert.Single(received);
        Assert.Equal(users[1].Id, sent[0].User.Id);
        Assert.Equal(users[0].Id, received[0].User.Id);
    }

    [Fact]
    public async Task Send_ToSelf_ThrowsSelfAction()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<SelfActionException>(() => requests.SendAsync(users[0].Id, users[0].Id));
    }

    [Fact]
    public async Task Send_ToUnknownUser_ThrowsNotFound()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<UserNotFoundException>(() => requests.SendAsync(users[0].Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task Send_Duplicate_ThrowsPendingExists()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[0].Id, users[1].Id);

        await Assert.ThrowsAsync<FriendRequestExistsException>(() => requests.SendAsync(users[0].Id, users[1].Id));
    }

    [Fact]
    public async Task Send_WhenReversePending_ThrowsPendingExists()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[1].Id, users[0].Id);

        await Assert.ThrowsAsync<FriendRequestExistsException>(() => requests.SendAsync(users[0].Id, users[1].Id));
    }

    [Fact]
    public async Task Send_WhenAlreadyFriends_ThrowsAlreadyFriends()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[0].Id, users[1].Id);
        await requests.AcceptAsync(users[1].Id, users[0].Id);

        await Assert.ThrowsAsync<AlreadyFriendsException>(() => requests.SendAsync(users[0].Id, users[1].Id));
    }

    [Fact]
    public async Task Reject_RemovesRequest()
    {
        await using var db = fixture.CreateContext();
        var (requests, friends, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[0].Id, users[1].Id);
        await requests.RejectAsync(users[1].Id, users[0].Id);

        Assert.Empty(await requests.GetReceivedAsync(users[1].Id));
        Assert.Empty(await friends.GetFriendsAsync(users[0].Id));
    }

    [Fact]
    public async Task Cancel_RemovesRequest()
    {
        await using var db = fixture.CreateContext();
        var (requests, _, _, _, _) = SocialServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await requests.SendAsync(users[0].Id, users[1].Id);
        await requests.CancelAsync(users[0].Id, users[1].Id);

        Assert.Empty(await requests.GetSentAsync(users[0].Id));
        Assert.Empty(await requests.GetReceivedAsync(users[1].Id));
    }
}