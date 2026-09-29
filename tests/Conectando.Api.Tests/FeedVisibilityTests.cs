using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class FeedVisibilityTests(SocialTestFixture fixture)
{
    private static FeedReadService CreateService(ConectandoDbContext db)
    {
        return new FeedReadService(db, new FeedAuthorResolver(new PostVisibilityService(db)));
    }

    [Fact]
    public async Task Feed_IncludesOwnPost()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 1);
        await PostTestData.SeedPostAsync(db, users[0], "mio", PostPrivacy.Private);

        var page = await service.GetPageAsync(users[0].Id, null, 10);

        var item = Assert.Single(page.Items);
        Assert.Equal("mio", item.Content);
        Assert.Equal(PostPrivacy.Private, item.Privacy);
    }

    [Fact]
    public async Task Feed_IncludesFriendPublicAndFriendsPosts()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await PostTestData.SeedFriendshipAsync(db, users[0].Id, users[1].Id);
        await PostTestData.SeedPostAsync(db, users[1], "publica", PostPrivacy.Public);
        await PostTestData.SeedPostAsync(db, users[1], "amigos", PostPrivacy.Friends);

        var page = await service.GetPageAsync(users[0].Id, null, 10);

        Assert.Equal(2, page.Items.Count);
        Assert.All(page.Items, i => Assert.NotEqual(PostPrivacy.Private, i.Privacy));
    }

    [Fact]
    public async Task Feed_FollowedUser_PublicOnly()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await PostTestData.SeedFollowAsync(db, users[0].Id, users[1].Id);
        await PostTestData.SeedPostAsync(db, users[1], "publica", PostPrivacy.Public);
        await PostTestData.SeedPostAsync(db, users[1], "amigos", PostPrivacy.Friends);
        await PostTestData.SeedPostAsync(db, users[1], "privada", PostPrivacy.Private);

        var page = await service.GetPageAsync(users[0].Id, null, 10);

        var item = Assert.Single(page.Items);
        Assert.Equal("publica", item.Content);
    }

    [Fact]
    public async Task Feed_FriendAndFollowed_NoDuplicates()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await PostTestData.SeedFriendshipAsync(db, users[0].Id, users[1].Id);
        await PostTestData.SeedFollowAsync(db, users[0].Id, users[1].Id);
        await PostTestData.SeedPostAsync(db, users[1], "doble", PostPrivacy.Public);

        var page = await service.GetPageAsync(users[0].Id, null, 10);

        Assert.Single(page.Items);
    }

    [Fact]
    public async Task Feed_BlockedAuthor_Excluded()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await PostTestData.SeedFriendshipAsync(db, users[0].Id, users[1].Id);
        await PostTestData.SeedFollowAsync(db, users[0].Id, users[1].Id);
        await PostTestData.SeedPostAsync(db, users[1], "publica", PostPrivacy.Public);
        await db.Blocks.AddAsync(new Block { UserId = users[1].Id, BlockedUserId = users[0].Id, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var page = await service.GetPageAsync(users[0].Id, null, 10);

        Assert.Empty(page.Items);
    }

    [Fact]
    public async Task Feed_Unfollow_RemovesPostsOnNextRequest()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await PostTestData.SeedFollowAsync(db, users[0].Id, users[1].Id);
        await PostTestData.SeedPostAsync(db, users[1], "publica", PostPrivacy.Public);

        var before = await service.GetPageAsync(users[0].Id, null, 10);
        Assert.Single(before.Items);

        var follow = await db.Follows.SingleAsync(f => f.UserId == users[0].Id);
        db.Follows.Remove(follow);
        await db.SaveChangesAsync();

        var after = await service.GetPageAsync(users[0].Id, null, 10);
        Assert.Empty(after.Items);
    }

    [Fact]
    public async Task Feed_Empty_ReturnsEmptyPage()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 1);

        var page = await service.GetPageAsync(users[0].Id, null, 10);

        Assert.Empty(page.Items);
        Assert.False(page.HasMore);
        Assert.Null(page.NextCursor);
    }
}