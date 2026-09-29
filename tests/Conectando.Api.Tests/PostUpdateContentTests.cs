using Conectando.Api.DTOs.Posts;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Tests.TestInfrastructure;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class PostUpdateContentTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task Update_ByAuthor_ChangesContentAndPrivacy()
    {
        await using var db = fixture.CreateContext();
        var (read, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], "original", PostPrivacy.Public);

        var updated = await write.UpdateAsync(post.Id, users[0].Id, new UpdatePostRequest
        {
            Content = "editado",
            Privacy = PostPrivacy.Private,
        });

        Assert.Equal("editado", updated.Content);
        Assert.Equal(PostPrivacy.Private, updated.Privacy);
    }

    [Fact]
    public async Task Update_ByOtherUser_Forbidden()
    {
        await using var db = fixture.CreateContext();
        var (_, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "texto", PostPrivacy.Public);

        await Assert.ThrowsAsync<PostOwnershipException>(() =>
            write.UpdateAsync(post.Id, users[1].Id, new UpdatePostRequest { Content = "hack" }));
    }

    [Fact]
    public async Task Update_ClearingContentWithoutMedia_RejectedAndUnchanged()
    {
        await using var db = fixture.CreateContext();
        var (read, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], "texto", PostPrivacy.Public);

        await Assert.ThrowsAsync<EmptyPostException>(() =>
            write.UpdateAsync(post.Id, users[0].Id, new UpdatePostRequest { Content = null }));

        var stored = await read.GetByIdAsync(post.Id, users[0].Id);
        Assert.Equal("texto", stored.Content);
    }

    [Fact]
    public async Task Update_PrivacyOmitted_KeepsExistingPrivacy()
    {
        await using var db = fixture.CreateContext();
        var (read, write, _, _) = PostServices.Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], "secreto", PostPrivacy.Private);

        await write.UpdateAsync(post.Id, users[0].Id, new UpdatePostRequest { Content = "sigue siendo privado" });

        var stored = await read.GetByIdAsync(post.Id, users[0].Id);
        Assert.Equal(PostPrivacy.Private, stored.Privacy);
    }
}