using Conectando.Api.DTOs.Posts;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class PostCreateTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Create_TextOnly_Succeeds()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        var post = await write.CreateAsync(users[0].Id, new CreatePostRequest { Content = "Hola mundo", Privacy = PostPrivacy.Public });

        Assert.Equal(users[0].Id, post.Author.Id);
        Assert.Equal("Hola mundo", post.Content);
        Assert.Empty(post.Media);
        Assert.Equal(PostPrivacy.Public, post.Privacy);
        Assert.Equal(1, await db.Posts.CountAsync(p => p.AuthorId == users[0].Id));
    }

    [Fact]
    public async Task Create_EmptyContent_Rejected()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<EmptyPostException>(() => write.CreateAsync(users[0].Id, new CreatePostRequest { Content = "" }));
    }

    [Fact]
    public async Task Create_WhitespaceOnly_Rejected()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<EmptyPostException>(() => write.CreateAsync(users[0].Id, new CreatePostRequest { Content = "   " }));
    }

    [Fact]
    public async Task Create_PhotoOnly_SucceedsWithAttachedMedia()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var pending = await PostTestData.SeedPendingAsync(db, users[0], 1);

        var post = await write.CreateAsync(users[0].Id, new CreatePostRequest { MediaIds = [pending[0].Id] });

        var media = Assert.Single(post.Media);
        Assert.Equal(pending[0].Id, media.Id);
        Assert.Equal(0, media.DisplayOrder);

        var stored = await db.PostMedia.AsNoTracking().SingleAsync(m => m.Id == pending[0].Id);
        Assert.Equal(PostMediaState.Attached, stored.State);
        Assert.NotNull(stored.PostId);
    }

    [Fact]
    public async Task Create_PendingMediaOfAnotherUser_Rejected()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var pending = await PostTestData.SeedPendingAsync(db, users[1], 1);

        await Assert.ThrowsAsync<PendingMediaNotFoundException>(() =>
            write.CreateAsync(users[0].Id, new CreatePostRequest { MediaIds = [pending[0].Id] }));
    }

    [Fact]
    public async Task Create_OrdersMediaByProvidedSequence()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var ids = (await PostTestData.SeedPendingAsync(db, users[0], 3)).Select(m => m.Id).ToArray();

        var post = await write.CreateAsync(users[0].Id, new CreatePostRequest { MediaIds = [ids[2], ids[0], ids[1]] });

        Assert.Equal([ids[2], ids[0], ids[1]], post.Media.Select(m => m.Id).ToArray());
        Assert.Equal([0, 1, 2], post.Media.Select(m => m.DisplayOrder).ToArray());
    }

    [Fact]
    public async Task Create_MoreThanMaxPhotos_Rejected()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var pending = await PostTestData.SeedPendingAsync(db, users[0], 9);

        await Assert.ThrowsAsync<MediaLimitException>(() =>
            write.CreateAsync(users[0].Id, new CreatePostRequest { MediaIds = pending.Select(m => m.Id).ToList() }));
    }

    [Fact]
    public async Task Create_ContentOverMaxLength_Rejected()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<MediaLimitException>(() =>
            write.CreateAsync(users[0].Id, new CreatePostRequest { Content = new string('a', 2001) }));
    }

    [Fact]
    public async Task Create_DuplicateMediaIds_UsesDistinct()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var pending = await PostTestData.SeedPendingAsync(db, users[0], 2);

        var post = await write.CreateAsync(users[0].Id,
            new CreatePostRequest { MediaIds = [pending[0].Id, pending[1].Id, pending[0].Id] });

        Assert.Equal(2, post.Media.Count);
    }
}