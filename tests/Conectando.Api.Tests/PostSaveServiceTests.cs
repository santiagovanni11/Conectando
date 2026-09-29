using Conectando.Api.Data;
using Conectando.Api.DTOs.Posts;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class PostSaveServiceTests(SocialTestFixture fixture)
{
    private PostSaveService CreateService(ConectandoDbContext db) =>
        new(db, new PostVisibilityService(db));

    [Fact]
    public async Task Toggle_FirstTime_SavesThePost()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);

        var result = await service.ToggleAsync(users[1].Id, post.Id);

        Assert.True(result.SavedByMe);
        Assert.Single(await db.PostSaves.Where(s => s.PostId == post.Id).ToListAsync());
    }

    [Fact]
    public async Task Toggle_SecondTime_RemovesTheSave()
    {
        await using var setup = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(setup, 2);
        var post = await PostTestData.SeedPostAsync(setup, users[0], "Hola", PostPrivacy.Public);
        var postId = post.Id;
        var userId = users[1].Id;

        await using (var first = fixture.CreateContext())
        {
            await CreateService(first).ToggleAsync(userId, postId);
        }

        await using var second = fixture.CreateContext();
        var result = await CreateService(second).ToggleAsync(userId, postId);

        Assert.False(result.SavedByMe);

        // Se filtra por el post: la base de pruebas es compartida entre
        // casos y puede tener guardados de otros usuarios.
        await using var verify = fixture.CreateContext();
        var remaining = await verify.PostSaves.Where(s => s.PostId == postId).ToListAsync();
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task Toggle_PrivatePostOfStranger_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Privado", PostPrivacy.Private);

        // Guardar no esquiva la privacidad: si no se puede ver, no se guarda.
        await Assert.ThrowsAsync<PostNotFoundException>(() => service.ToggleAsync(users[1].Id, post.Id));
    }

    [Fact]
    public async Task ListSaved_ReturnsOnlyWhatTheUserSaved()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post1 = await PostTestData.SeedPostAsync(db, users[0], "Guardada", PostPrivacy.Public);
        var post2 = await PostTestData.SeedPostAsync(db, users[0], "No guardada", PostPrivacy.Public);

        await service.ToggleAsync(users[1].Id, post1.Id);

        var page = await service.ListSavedAsync(users[1].Id, new PostListQuery());

        var item = Assert.Single(page.Items);
        Assert.Equal(post1.Id, item.Id);
        Assert.True(item.SavedByMe);
        Assert.DoesNotContain(page.Items, p => p.Id == post2.Id);
    }

    [Fact]
    public async Task ListSaved_HidesPostsFromBlockedUsers()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Bloqueada", PostPrivacy.Public);

        await service.ToggleAsync(users[1].Id, post.Id);
        await BlockTestData.SeedBlockAsync(db, blockerId: users[1].Id, blockedId: users[0].Id);

        // Guardar antes de bloquear no te deja seguir viendo lo suyo.
        var page = await service.ListSavedAsync(users[1].Id, new PostListQuery());

        Assert.Empty(page.Items);
    }
}