using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class FeedPaginationTests(SocialTestFixture fixture)
{
    private static FeedReadService CreateService(ConectandoDbContext db)
    {
        return new FeedReadService(db, new FeedAuthorResolver(new PostVisibilityService(db)));
    }

    private static async Task<List<Post>> SeedManyAsync(ConectandoDbContext db, AppUser author, int count)
    {
        var posts = new List<Post>();
        for (var i = 0; i < count; i++)
        {
            posts.Add(await PostTestData.SeedPostAsync(db, author, $"post {i}", PostPrivacy.Public,
                createdAt: DateTime.UtcNow.AddMinutes(-i)));
        }

        return posts;
    }

    [Fact]
    public async Task Feed_HasMoreAndNextCursor()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 1);
        await SeedManyAsync(db, users[0], 5);

        var page = await service.GetPageAsync(users[0].Id, null, 2);

        Assert.Equal(2, page.Items.Count);
        Assert.True(page.HasMore);
        Assert.NotNull(page.NextCursor);
    }

    [Fact]
    public async Task Feed_CursorNavigatesWithoutDuplicatesOrGaps()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 1);
        await SeedManyAsync(db, users[0], 5);

        var first = await service.GetPageAsync(users[0].Id, null, 2);
        var second = await service.GetPageAsync(users[0].Id, first.NextCursor, 2);
        var third = await service.GetPageAsync(users[0].Id, second.NextCursor, 2);

        var ids = first.Items.Concat(second.Items).Concat(third.Items).Select(i => i.Id).ToHashSet();
        Assert.Equal(5, ids.Count);
        Assert.False(third.HasMore);
    }

    [Fact]
    public async Task Feed_InvalidCursor_ThrowsInvalidCursorException()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<InvalidCursorException>(() => service.GetPageAsync(users[0].Id, "no-es-un-cursor", 10));
    }

    [Fact]
    public async Task Feed_LimitIsClamped()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 1);
        await SeedManyAsync(db, users[0], 25);

        var overMax = await service.GetPageAsync(users[0].Id, null, 100);
        Assert.Equal(20, overMax.Items.Count);

        var underMin = await service.GetPageAsync(users[0].Id, null, 0);
        Assert.Single(underMin.Items);
    }

    [Fact]
    public async Task Feed_OrderIsCreatedAtDescThenIdDesc()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await PostTestData.SeedFollowAsync(db, users[0].Id, users[1].Id);
        await PostTestData.SeedPostAsync(db, users[0], "A", PostPrivacy.Public, createdAt: DateTime.UtcNow);
        await PostTestData.SeedPostAsync(db, users[1], "B", PostPrivacy.Public, createdAt: DateTime.UtcNow);

        var page = await service.GetPageAsync(users[0].Id, null, 10);

        Assert.Equal(2, page.Items.Count);
        for (var i = 1; i < page.Items.Count; i++)
        {
            var previous = page.Items[i - 1];
            var current = page.Items[i];
            Assert.True(previous.CreatedAt > current.CreatedAt
                || (previous.CreatedAt == current.CreatedAt && previous.Id > current.Id));
        }
    }
}