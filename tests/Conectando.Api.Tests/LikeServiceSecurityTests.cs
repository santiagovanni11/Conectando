using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class LikeServiceSecurityTests(SocialTestFixture fixture)
{
    private LikeService CreateService(ConectandoDbContext db)
    {
        return new LikeService(db, new PostVisibilityService(db), new NullNotificationService());
    }

    [Fact]
    public async Task Like_PrivatePost_ThrowsPostNotFoundException()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Private);

        await Assert.ThrowsAsync<PostNotFoundException>(() => service.LikeAsync(post.Id, users[1].Id));
        Assert.Empty(db.Likes.Where(l => l.PostId == post.Id));
    }

    [Fact]
    public async Task Like_FriendsPost_ByFriend_Succeeds()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await PostTestData.SeedFriendshipAsync(db, users[0].Id, users[1].Id);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Friends);

        var result = await service.LikeAsync(post.Id, users[1].Id);

        Assert.True(result.LikedByMe);
        Assert.Equal(1, result.LikesCount);
    }

    [Fact]
    public async Task Like_OwnPost_Succeeds()
    {
        // A diferencia de otros sitios, uno sí puede darse "me gusta"
        // a su propio post.
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Mi post", PostPrivacy.Public);

        var result = await service.LikeAsync(post.Id, users[0].Id);

        Assert.True(result.LikedByMe);
    }

    [Fact]
    public async Task Like_MissingPost_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<PostNotFoundException>(
            () => service.LikeAsync(Guid.NewGuid(), users[0].Id));
    }

    [Fact]
    public async Task GetLikes_PrivatePost_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Privado", PostPrivacy.Private);

        await Assert.ThrowsAsync<PostNotFoundException>(
            () => service.GetLikesAsync(post.Id, users[1].Id, null, 20));
    }
}