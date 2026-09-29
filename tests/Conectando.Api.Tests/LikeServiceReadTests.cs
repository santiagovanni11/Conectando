using Conectando.Api.Data;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class LikeServiceReadTests(SocialTestFixture fixture)
{
    private LikeService CreateService(ConectandoDbContext db)
    {
        return new LikeService(db, new PostVisibilityService(db), new NullNotificationService());
    }

    [Fact]
    public async Task GetLikes_ListsWhoLiked()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 3);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        await LikeTestData.SeedLikesAsync(db, post.Id, users[1].Id, users[2].Id);

        var result = await service.GetLikesAsync(post.Id, users[0].Id, null, 20);

        Assert.Equal(2, result.Items.Count);
        Assert.False(result.HasMore);
    }

    [Fact]
    public async Task GetLikes_PaginatesWithCursor()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 4);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        await LikeTestData.SeedLikesAsync(db, post.Id, users[1].Id, users[2].Id, users[3].Id);

        var first = await service.GetLikesAsync(post.Id, users[0].Id, null, 2);

        Assert.Equal(2, first.Items.Count);
        Assert.True(first.HasMore);
        Assert.NotNull(first.NextCursor);

        var second = await service.GetLikesAsync(post.Id, users[0].Id, first.NextCursor, 2);

        Assert.Single(second.Items);
        Assert.False(second.HasMore);

        // Sin repetir usuarios entre páginas.
        Assert.Empty(first.Items.Select(i => i.Id).Intersect(second.Items.Select(i => i.Id)));
    }

    [Fact]
    public async Task GetLikes_EmptyWhenNobodyLiked()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);

        var result = await service.GetLikesAsync(post.Id, users[0].Id, null, 20);

        Assert.Empty(result.Items);
    }
}