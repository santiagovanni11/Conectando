using Conectando.Api.DTOs.Posts;
using Conectando.Api.Models;
using Conectando.Api.Tests.TestInfrastructure;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class PostPaginationTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task List_Pagination_KeysetCursorReturnsAllPages()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var baseTime = DateTime.UtcNow.AddDays(-1);
        for (var i = 0; i < 25; i++)
        {
            await PostTestData.SeedPostAsync(db, users[0], $"p{i}", PostPrivacy.Public, createdAt: baseTime.AddMinutes(i));
        }

        var page1 = await read.ListByUserAsync(users[0].Id, users[0].Id, new PostListQuery { Limit = 10 });
        var page2 = await read.ListByUserAsync(users[0].Id, users[0].Id, new PostListQuery { Limit = 10, Cursor = page1.NextCursor });
        var page3 = await read.ListByUserAsync(users[0].Id, users[0].Id, new PostListQuery { Limit = 10, Cursor = page2.NextCursor });

        Assert.Equal(10, page1.Items.Count);
        Assert.True(page1.HasMore);
        Assert.Equal(10, page2.Items.Count);
        Assert.True(page2.HasMore);
        Assert.Equal(5, page3.Items.Count);
        Assert.False(page3.HasMore);
        Assert.Null(page3.NextCursor);

        var ids = page1.Items.Concat(page2.Items).Concat(page3.Items).Select(p => p.Id).ToHashSet();
        Assert.Equal(25, ids.Count);
        Assert.Equal("p24", page1.Items[0].Content);
        Assert.Equal("p0", page3.Items[^1].Content);
    }

    [Fact]
    public async Task List_SameCreatedAt_OrderedByNewestThenId()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var sameTime = DateTime.UtcNow;
        await PostTestData.SeedPostAsync(db, users[0], "a", PostPrivacy.Public, createdAt: sameTime);
        await PostTestData.SeedPostAsync(db, users[0], "b", PostPrivacy.Public, createdAt: sameTime);

        var page1 = await read.ListByUserAsync(users[0].Id, users[0].Id, new PostListQuery { Limit = 10 });

        Assert.Equal(2, page1.Items.Count);
        Assert.Equal(page1.Items.Select(p => p.Id).OrderByDescending(id => id).ToArray(), page1.Items.Select(p => p.Id).ToArray());
    }

    [Fact]
    public async Task List_TieBreak_ContinuityAcrossPages()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var sameTime = DateTime.UtcNow;
        var newer = sameTime.AddMinutes(10);
        await PostTestData.SeedPostAsync(db, users[0], "n1", PostPrivacy.Public, createdAt: newer);
        await PostTestData.SeedPostAsync(db, users[0], "n2", PostPrivacy.Public, createdAt: newer);
        for (var i = 0; i < 9; i++)
        {
            await PostTestData.SeedPostAsync(db, users[0], $"o{i}", PostPrivacy.Public, createdAt: sameTime.AddMinutes(i));
        }

        var page1 = await read.ListByUserAsync(users[0].Id, users[0].Id, new PostListQuery { Limit = 10 });
        var page2 = await read.ListByUserAsync(users[0].Id, users[0].Id, new PostListQuery { Limit = 10, Cursor = page1.NextCursor });

        Assert.Equal(10, page1.Items.Count);
        Assert.True(page1.HasMore);
        Assert.Single(page2.Items);
        Assert.False(page2.HasMore);

        var ids = page1.Items.Concat(page2.Items).Select(p => p.Id).ToHashSet();
        Assert.Equal(11, ids.Count);
    }

    [Fact]
    public async Task List_InvalidCursor_IsIgnoredAndReturnsFirstPage()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        await PostTestData.SeedPostAsync(db, users[0], "único", PostPrivacy.Public);

        var list = await read.ListByUserAsync(users[0].Id, users[0].Id, new PostListQuery { Cursor = "no-va" });

        Assert.Single(list.Items);
    }
}