using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class PostMediaDeleteTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Delete_Pending_ByOwner_RemovesAndDestroys()
    {
        await using var db = fixture.CreateContext();
        var (_, _, media, storage) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var pending = await PostTestData.SeedPendingAsync(db, users[0], 1);

        await media.DeleteAsync(pending[0].Id, users[0].Id);

        Assert.Empty(await db.PostMedia.AsNoTracking().Where(m => m.UserId == users[0].Id).ToListAsync());
        Assert.Contains(pending[0].PublicId, storage.Destroyed);
    }

    [Fact]
    public async Task Delete_Pending_ByOtherUser_NotFound()
    {
        await using var db = fixture.CreateContext();
        var (_, _, media, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var pending = await PostTestData.SeedPendingAsync(db, users[0], 1);

        await Assert.ThrowsAsync<PendingMediaNotFoundException>(() => media.DeleteAsync(pending[0].Id, users[1].Id));
    }

    [Fact]
    public async Task Delete_Attached_ByPostOwner_Removes()
    {
        await using var db = fixture.CreateContext();
        var (_, _, media, storage) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var mediaRows = await PostTestData.SeedPendingAsync(db, users[0], 1);
        await PostTestData.SeedPostAsync(db, users[0], null, PostPrivacy.Public, media: mediaRows);

        await media.DeleteAsync(mediaRows[0].Id, users[0].Id);

        Assert.Empty(await db.PostMedia.AsNoTracking().Where(m => m.UserId == users[0].Id).ToListAsync());
        Assert.Contains(mediaRows[0].PublicId, storage.Destroyed);
    }

    [Fact]
    public async Task Delete_Attached_ByNonOwner_Forbidden()
    {
        await using var db = fixture.CreateContext();
        var (_, _, media, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var mediaRows = await PostTestData.SeedPendingAsync(db, users[0], 1);
        await PostTestData.SeedPostAsync(db, users[0], null, PostPrivacy.Public, media: mediaRows);

        await Assert.ThrowsAsync<PostOwnershipException>(() => media.DeleteAsync(mediaRows[0].Id, users[1].Id));
    }
}