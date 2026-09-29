using Conectando.Api.DTOs;
using Conectando.Api.DTOs.Posts;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Tests.TestInfrastructure;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class PostPatchTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Patch_ContentAndPrivacy_ByAuthor_AppliesOnlyThoseFields()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var patch = PostServices.CreatePatch(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], "original", PostPrivacy.Public);

        var updated = await patch.PatchAsync(post.Id, users[0].Id, new PatchPostRequest
        {
            Content = Optional<string?>.Of("editado"),
            Privacy = Optional<PostPrivacy>.Of(PostPrivacy.Private),
        });

        Assert.Equal("editado", updated.Content);
        Assert.Equal(PostPrivacy.Private, updated.Privacy);
        var stored = await read.GetByIdAsync(post.Id, users[0].Id);
        Assert.Equal("editado", stored.Content);
    }

    [Fact]
    public async Task Patch_OmitContent_KeepsExistingContentAndChangesPrivacy()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var patch = PostServices.CreatePatch(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], "texto original", PostPrivacy.Public);

        await patch.PatchAsync(post.Id, users[0].Id, new PatchPostRequest
        {
            Privacy = Optional<PostPrivacy>.Of(PostPrivacy.Private),
        });

        var stored = await read.GetByIdAsync(post.Id, users[0].Id);
        Assert.Equal("texto original", stored.Content);
        Assert.Equal(PostPrivacy.Private, stored.Privacy);
    }

    [Fact]
    public async Task Patch_ExplicitNullContentWithoutMedia_RejectedAndUnchanged()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var patch = PostServices.CreatePatch(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], "texto", PostPrivacy.Public);

        await Assert.ThrowsAsync<EmptyPostException>(() =>
            patch.PatchAsync(post.Id, users[0].Id, new PatchPostRequest { Content = Optional<string?>.Of(null) }));

        var stored = await read.GetByIdAsync(post.Id, users[0].Id);
        Assert.Equal("texto", stored.Content);
    }

    [Fact]
    public async Task Patch_ExplicitNullContent_WithMedia_ClearsContent()
    {
        await using var db = fixture.CreateContext();
        var (read, _, _, _) = PostServices.Create(db);
        var patch = PostServices.CreatePatch(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var media = await PostTestData.SeedPendingAsync(db, users[0], 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], "texto", PostPrivacy.Public, media: media);

        var updated = await patch.PatchAsync(post.Id, users[0].Id, new PatchPostRequest
        {
            Content = Optional<string?>.Of(null),
        });

        Assert.Null(updated.Content);
        var stored = await read.GetByIdAsync(post.Id, users[0].Id);
        Assert.Null(stored.Content);
    }

    [Fact]
    public async Task Patch_ByOtherUser_Forbidden()
    {
        await using var db = fixture.CreateContext();
        var (_, _, _, _) = PostServices.Create(db);
        var patch = PostServices.CreatePatch(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "texto", PostPrivacy.Public);

        await Assert.ThrowsAsync<PostOwnershipException>(() =>
            patch.PatchAsync(post.Id, users[1].Id, new PatchPostRequest { Content = Optional<string?>.Of("hack") }));
    }

    [Fact]
    public async Task Patch_UnknownPost_NotFound()
    {
        await using var db = fixture.CreateContext();
        var (_, _, _, _) = PostServices.Create(db);
        var patch = PostServices.CreatePatch(db);
        var users = await TestUsers.SeedAsync(db, 1);

        await Assert.ThrowsAsync<PostNotFoundException>(() =>
            patch.PatchAsync(Guid.NewGuid(), users[0].Id, new PatchPostRequest { Content = Optional<string?>.Of("texto") }));
    }
}