using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class PostMutationTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Delete_ByAuthor_RemovesPostMediaAndDestroysAssets()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, storage) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var media = await PostTestData.SeedPendingAsync(db, users[0], 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], null, PostPrivacy.Public, media: media);

        await write.DeleteAsync(post.Id, users[0].Id);

        Assert.Empty(await db.Posts.AsNoTracking().Where(p => p.AuthorId == users[0].Id).ToListAsync());
        Assert.Empty(await db.PostMedia.AsNoTracking().Where(m => m.UserId == users[0].Id).ToListAsync());
        Assert.Equal(2, storage.Destroyed.Count);
        Assert.Contains(media[0].PublicId, storage.Destroyed);
        Assert.Contains(media[1].PublicId, storage.Destroyed);
    }

    [Fact]
    public async Task Delete_ByOtherUser_Forbidden()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "texto", PostPrivacy.Public);

        await Assert.ThrowsAsync<PostOwnershipException>(() => write.DeleteAsync(post.Id, users[1].Id));
    }

    [Fact]
    public async Task Delete_UnknownPost_NotFound()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<PostNotFoundException>(() => write.DeleteAsync(Guid.NewGuid(), users[0].Id));
    }
}