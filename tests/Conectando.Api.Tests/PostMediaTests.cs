using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class PostMediaTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Upload_AddsPendingDrafts()
    {
        await using var db = fixture.CreateContext();
        var (_, _, media, storage) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        var drafts = await media.UploadAsync(users[0].Id, [PostTestData.JpegFile("a.jpg"), PostTestData.JpegFile("b.jpg")]);

        Assert.Equal(2, drafts.Count);
        Assert.Equal(2, storage.Uploaded.Count);
        Assert.Equal(2, await db.PostMedia.AsNoTracking().CountAsync(m => m.UserId == users[0].Id && m.State == PostMediaState.Pending));
    }

    [Fact]
    public async Task Upload_TooManyFiles_Rejected()
    {
        await using var db = fixture.CreateContext();
        var (_, _, media, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<MediaLimitException>(() =>
            media.UploadAsync(users[0].Id, Enumerable.Range(0, 9).Select(_ => PostTestData.JpegFile()).ToArray()));
    }

    [Fact]
    public async Task Upload_InvalidType_Rejected()
    {
        await using var db = fixture.CreateContext();
        var (_, _, media, storage) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var stream = new MemoryStream("solo texto"u8.ToArray());
        var file = new FormFile(stream, 0, stream.Length, "files", "nota.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain",
        };

        await Assert.ThrowsAsync<InvalidFileException>(() => media.UploadAsync(users[0].Id, [file]));

        Assert.Empty(storage.Uploaded);
        Assert.Empty(await db.PostMedia.AsNoTracking().Where(m => m.UserId == users[0].Id).ToListAsync());
    }

    [Fact]
    public async Task Upload_SpoofedSignature_Rejected()
    {
        await using var db = fixture.CreateContext();
        var (_, _, media, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var stream = new MemoryStream(PostTestData.ValidJpegBytes);
        var file = new FormFile(stream, 0, stream.Length, "files", "foto.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png",
        };

        await Assert.ThrowsAsync<InvalidFileException>(() => media.UploadAsync(users[0].Id, [file]));
    }

    [Fact]
    public async Task Upload_DraftLimitExceeded_Rejected()
    {
        await using var db = fixture.CreateContext();
        var (_, _, media, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        await PostTestData.SeedPendingAsync(db, users[0], 16);

        await Assert.ThrowsAsync<MediaLimitException>(() => media.UploadAsync(users[0].Id, [PostTestData.JpegFile()]));
    }

    [Fact]
    public async Task Upload_MidBatchFailure_DestroysUploadedAssets()
    {
        await using var db = fixture.CreateContext();
        var (_, _, media, storage) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        storage.FailOnUploadAfter = 1;

        await Assert.ThrowsAsync<MediaStorageException>(() =>
            media.UploadAsync(users[0].Id, [PostTestData.JpegFile("1.jpg"), PostTestData.JpegFile("2.jpg")]));

        Assert.Equal(2, storage.Uploaded.Count);
        Assert.Single(storage.Destroyed);
        Assert.Empty(await db.PostMedia.AsNoTracking().Where(m => m.UserId == users[0].Id).ToListAsync());
    }

    [Fact]
    public async Task Upload_CleansStalePendingBeforeCounting()
    {
        await using var db = fixture.CreateContext();
        var (_, _, media, storage) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var now = DateTime.UtcNow;
        db.PostMedia.AddRange(Enumerable.Range(0, 16).Select(i => new PostMedia
        {
            Id = Guid.NewGuid(),
            UserId = users[0].Id,
            Url = $"https://media.test/{i}.jpg",
            PublicId = $"posts/stale-{i}",
            DisplayOrder = i,
            State = PostMediaState.Pending,
            CreatedAt = now.AddHours(-25),
        }));
        await db.SaveChangesAsync();

        var drafts = await media.UploadAsync(users[0].Id, [PostTestData.JpegFile()]);

        Assert.Single(drafts);
        Assert.Equal(16, storage.Destroyed.Count);
        Assert.Equal(1, await db.PostMedia.AsNoTracking().CountAsync(m => m.UserId == users[0].Id && m.State == PostMediaState.Pending));
    }
}