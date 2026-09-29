using Conectando.Api.DTOs.Posts;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class PostUpdateMediaTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Update_AddingPendingMedia_KeepsGivenOrder()
    {
        await using var db = fixture.CreateContext();
        var (read, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var first = await PostTestData.SeedPendingAsync(db, users[0], 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], null, PostPrivacy.Public, media: first);
        var added = await PostTestData.SeedPendingAsync(db, users[0], 2);

        var updated = await write.UpdateAsync(post.Id, users[0].Id,
            new UpdatePostRequest { MediaIds = [first[0].Id, added[0].Id, added[1].Id] });

        Assert.Equal(3, updated.Media.Count);
        Assert.Equal([0, 1, 2], updated.Media.Select(m => m.DisplayOrder).ToArray());
        Assert.Equal([added[0].Id, added[1].Id], updated.Media.Skip(1).Select(m => m.Id).ToArray());
        Assert.Equal(3, (await read.GetByIdAsync(post.Id, users[0].Id)).Media.Count);
    }

    [Fact]
    public async Task Update_RemovingMedia_DestroysCloudinaryAsset()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, storage) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var media = await PostTestData.SeedPendingAsync(db, users[0], 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], null, PostPrivacy.Public, media: media);

        var updated = await write.UpdateAsync(post.Id, users[0].Id, new UpdatePostRequest { MediaIds = [media[1].Id] });

        Assert.Single(updated.Media);
        Assert.Contains(media[0].PublicId, storage.Destroyed);
    }

    [Fact]
    public async Task Update_RemovedMediaRow_DeletedFromDatabase()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, storage) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var media = await PostTestData.SeedPendingAsync(db, users[0], 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], null, PostPrivacy.Public, media: media);

        await write.UpdateAsync(post.Id, users[0].Id, new UpdatePostRequest { MediaIds = [media[1].Id] });

        Assert.Null(await db.PostMedia.AsNoTracking().SingleOrDefaultAsync(m => m.Id == media[0].Id));
        Assert.NotNull(await db.PostMedia.AsNoTracking().SingleOrDefaultAsync(m => m.Id == media[1].Id));
        Assert.Contains(media[0].PublicId, storage.Destroyed);
    }

    [Fact]
    public async Task Update_ReordersExistingMedia()
    {
        await using var db = fixture.CreateContext();
        var (read, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var media = await PostTestData.SeedPendingAsync(db, users[0], 3);
        var post = await PostTestData.SeedPostAsync(db, users[0], "texto", PostPrivacy.Public, media: media);

        var updated = await write.UpdateAsync(post.Id, users[0].Id,
            new UpdatePostRequest { Content = "texto", MediaIds = [media[2].Id, media[0].Id, media[1].Id] });

        Assert.Equal([media[2].Id, media[0].Id, media[1].Id], updated.Media.Select(m => m.Id).ToArray());
        Assert.Equal([0, 1, 2], updated.Media.Select(m => m.DisplayOrder).ToArray());
        Assert.Equal(3, (await read.GetByIdAsync(post.Id, users[0].Id)).Media.Count);
    }

    [Fact]
    public async Task Update_RemoveAndReorderAndAdd_InOneCall()
    {
        await using var db = fixture.CreateContext();
        var (read, write, _, storage) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var existing = await PostTestData.SeedPendingAsync(db, users[0], 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "texto", PostPrivacy.Public, media: existing);
        var brandNew = await PostTestData.SeedPendingAsync(db, users[0], 1);

        var updated = await write.UpdateAsync(post.Id, users[0].Id,
            new UpdatePostRequest { MediaIds = [brandNew[0].Id, existing[1].Id] });

        Assert.Equal([brandNew[0].Id, existing[1].Id], updated.Media.Select(m => m.Id).ToArray());
        Assert.Equal([0, 1], updated.Media.Select(m => m.DisplayOrder).ToArray());
        Assert.Equal(2, updated.Media.Count);
        Assert.Contains(existing[0].PublicId, storage.Destroyed);
        Assert.Equal(2, (await read.GetByIdAsync(post.Id, users[0].Id)).Media.Count);
    }

    [Fact]
    public async Task Update_RemovingOnlyMediaWithoutContent_RejectedAndPreservesMedia()
    {
        await using var db = fixture.CreateContext();
        var (read, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var media = await PostTestData.SeedPendingAsync(db, users[0], 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], null, PostPrivacy.Public, media: media);

        await Assert.ThrowsAsync<EmptyPostException>(() =>
            write.UpdateAsync(post.Id, users[0].Id, new UpdatePostRequest { MediaIds = [] }));

        var stored = await read.GetByIdAsync(post.Id, users[0].Id);
        Assert.Single(stored.Media);
    }

    [Fact]
    public async Task Update_AddingAnotherUsersPending_Rejected()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "texto", PostPrivacy.Public);
        var foreign = await PostTestData.SeedPendingAsync(db, users[1], 1);

        await Assert.ThrowsAsync<PendingMediaNotFoundException>(() =>
            write.UpdateAsync(post.Id, users[0].Id, new UpdatePostRequest { MediaIds = [foreign[0].Id] }));
    }
}