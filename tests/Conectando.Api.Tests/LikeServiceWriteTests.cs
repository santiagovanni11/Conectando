using Conectando.Api.Data;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class LikeServiceWriteTests(SocialTestFixture fixture)
{
    private LikeService CreateService(ConectandoDbContext db)
    {
        return new LikeService(db, new PostVisibilityService(db), new NullNotificationService());
    }

    [Fact]
    public async Task Like_PublicPost_SavesAndReturnsStatus()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);

        var result = await service.LikeAsync(post.Id, users[1].Id);

        Assert.True(result.LikedByMe);
        Assert.Equal(1, result.LikesCount);
        Assert.Single(await db.Likes.Where(l => l.PostId == post.Id).ToListAsync());
    }

    [Fact]
    public async Task Like_Twice_IsIdempotent()
    {
        // Cada llamada usa un contexto limpio: con el mismo tracking de EF
        // la segunda inserción fallaría por la clave, no por la base.
        await using var setup = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(setup, 2);
        var post = await PostTestData.SeedPostAsync(setup, users[0], "Hola", PostPrivacy.Public);
        var postId = post.Id;
        var userId = users[1].Id;

        await using var first = fixture.CreateContext();
        await CreateService(first).LikeAsync(postId, userId, default);

        await using var second = fixture.CreateContext();
        var result = await CreateService(second).LikeAsync(postId, userId, default);

        Assert.Equal(1, result.LikesCount);
    }

    [Fact]
    public async Task Unlike_RemovesTheLike()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        await service.LikeAsync(post.Id, users[1].Id);

        var result = await service.UnlikeAsync(post.Id, users[1].Id);

        Assert.False(result.LikedByMe);
        Assert.Equal(0, result.LikesCount);
        Assert.Empty(await db.Likes.Where(l => l.PostId == post.Id).ToListAsync());
    }

    [Fact]
    public async Task Unlike_WhenNotLiked_DoesNothing()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);

        var result = await service.UnlikeAsync(post.Id, users[1].Id);

        Assert.False(result.LikedByMe);
        Assert.Equal(0, result.LikesCount);
    }

    [Fact]
    public async Task GetStatus_ReportsCountAndWhetherViewed()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 3);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        await LikeTestData.SeedLikesAsync(db, post.Id, users[1].Id, users[2].Id);

        var result = await service.GetStatusAsync(post.Id, users[1].Id);

        Assert.Equal(2, result.LikesCount);
        Assert.True(result.LikedByMe);
    }
}