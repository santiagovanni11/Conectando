using Conectando.Api.DTOs.Posts;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class PostListTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task List_ByUnknownUser_NotFound()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            read.ListByUserAsync(Guid.NewGuid(), users[0].Id, new PostListQuery()));
    }

    [Fact]
    public async Task List_ByAuthor_ReturnsAllIncludingPrivate()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        await PostTestData.SeedPostAsync(db, users[0], "pública", PostPrivacy.Public, createdAt: DateTime.UtcNow.AddMinutes(1));
        await PostTestData.SeedPostAsync(db, users[0], "amigos", PostPrivacy.Friends, createdAt: DateTime.UtcNow.AddMinutes(2));
        await PostTestData.SeedPostAsync(db, users[0], "privada", PostPrivacy.Private, createdAt: DateTime.UtcNow.AddMinutes(3));

        var list = await read.ListByUserAsync(users[0].Id, users[0].Id, new PostListQuery());

        Assert.Equal(3, list.Items.Count);
        Assert.False(list.HasMore);
        Assert.Null(list.NextCursor);
    }

    [Fact]
    public async Task List_WithMedia_ReturnsMediaOrderedForEachPost()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var media = await PostTestData.SeedPendingAsync(db, users[0], 3);
        var post = await PostTestData.SeedPostAsync(db, users[0], "con fotos", PostPrivacy.Public, media: media);

        var list = await read.ListByUserAsync(users[0].Id, users[0].Id, new PostListQuery());

        var item = Assert.Single(list.Items);
        Assert.Equal(3, item.Media.Count);
        Assert.Equal([media[0].Id, media[1].Id, media[2].Id], item.Media.Select(m => m.Id).ToArray());
        Assert.Equal([0, 1, 2], item.Media.Select(m => m.DisplayOrder).ToArray());
        Assert.Equal(1, await db.Posts.CountAsync(p => p.Id == post.Id));
    }

    [Fact]
    public async Task List_ByExternalUser_OnlyPublicWhenNotFriend()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await PostTestData.SeedPostAsync(db, users[0], "pública", PostPrivacy.Public);
        await PostTestData.SeedPostAsync(db, users[0], "amigos", PostPrivacy.Friends);
        await PostTestData.SeedPostAsync(db, users[0], "privada", PostPrivacy.Private);

        var list = await read.ListByUserAsync(users[0].Id, users[1].Id, new PostListQuery());

        var item = Assert.Single(list.Items);
        Assert.Equal("pública", item.Content);
    }

    [Fact]
    public async Task List_ByFriend_IncludesFriendsOnlyPosts()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await PostTestData.SeedFriendshipAsync(db, users[0].Id, users[1].Id);
        await PostTestData.SeedPostAsync(db, users[0], "pública", PostPrivacy.Public);
        await PostTestData.SeedPostAsync(db, users[0], "amigos", PostPrivacy.Friends);
        await PostTestData.SeedPostAsync(db, users[0], "privada", PostPrivacy.Private);

        var list = await read.ListByUserAsync(users[0].Id, users[1].Id, new PostListQuery());

        Assert.Equal(2, list.Items.Count);
        Assert.Contains(list.Items, p => p.Content == "pública");
        Assert.Contains(list.Items, p => p.Content == "amigos");
    }

    [Fact]
    public async Task List_Blocks_HidesAllPostsFromViewer()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await PostTestData.SeedPostAsync(db, users[0], "pública", PostPrivacy.Public);

        await new BlockService(db).BlockAsync(users[1].Id, users[0].Id);

        var list = await read.ListByUserAsync(users[0].Id, users[1].Id, new PostListQuery());
        Assert.Empty(list.Items);

        var own = await read.ListByUserAsync(users[0].Id, users[0].Id, new PostListQuery());
        Assert.Single(own.Items);
    }
}