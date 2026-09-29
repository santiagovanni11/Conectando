using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class PostReadTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Get_ByAuthor_SeesOwnPrivatePost()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], "secreto", PostPrivacy.Private);

        var dto = await read.GetByIdAsync(post.Id, users[0].Id);

        Assert.Equal("secreto", dto.Content);
    }

    [Fact]
    public async Task Get_ByExternalUser_Public_Visible()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "publica", PostPrivacy.Public);

        var dto = await read.GetByIdAsync(post.Id, users[1].Id);

        Assert.Equal("publica", dto.Content);
        Assert.Equal(users[0].Id, dto.Author.Id);
    }

    [Fact]
    public async Task Get_ByNonFriend_Private_NotFound()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "privada", PostPrivacy.Private);

        await Assert.ThrowsAsync<PostNotFoundException>(() => read.GetByIdAsync(post.Id, users[1].Id));
    }

    [Fact]
    public async Task Get_FriendsOnly_VisibleToFriend()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await PostTestData.SeedFriendshipAsync(db, users[0].Id, users[1].Id);
        var post = await PostTestData.SeedPostAsync(db, users[0], "solo amigos", PostPrivacy.Friends);

        var dto = await read.GetByIdAsync(post.Id, users[1].Id);

        Assert.Equal("solo amigos", dto.Content);
    }

    [Fact]
    public async Task Get_FriendsOnly_HiddenFromNonFriend()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "solo amigos", PostPrivacy.Friends);

        await Assert.ThrowsAsync<PostNotFoundException>(() => read.GetByIdAsync(post.Id, users[1].Id));
    }

    [Fact]
    public async Task Get_WhenBlocked_NotFoundInBothDirections()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "publica", PostPrivacy.Public);

        await new BlockService(db).BlockAsync(users[1].Id, users[0].Id);

        await Assert.ThrowsAsync<PostNotFoundException>(() => read.GetByIdAsync(post.Id, users[1].Id));

        var own = await read.GetByIdAsync(post.Id, users[0].Id);
        Assert.Equal("publica", own.Content);
    }

    [Fact]
    public async Task Get_UnknownPost_NotFound()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<PostNotFoundException>(() => read.GetByIdAsync(Guid.NewGuid(), users[0].Id));
    }
}