using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class CommentUpdateTests(SocialTestFixture fixture)
{
    private CommentWriteService Create(ConectandoDbContext db)
    {
        return new CommentWriteService(db, new PostVisibilityService(db), new NullNotificationService());
    }

    [Fact]
    public async Task Update_OwnComment_Succeeds()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        var request = new DTOs.Comments.CreateCommentRequest { Content = "Original" };
        var comment = await service.CreateCommentAsync(post.Id, users[1].Id, request);
        var updateRequest = new DTOs.Comments.UpdateCommentRequest { Content = "Editado" };

        var updated = await service.UpdateCommentAsync(comment.Id, users[1].Id, updateRequest);

        Assert.Equal("Editado", updated.Content);
        Assert.True(updated.IsEdited);
    }

    [Fact]
    public async Task Update_CommentOfOther_Fails()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        var request = new DTOs.Comments.CreateCommentRequest { Content = "Original" };
        var comment = await service.CreateCommentAsync(post.Id, users[1].Id, request);
        var updateRequest = new DTOs.Comments.UpdateCommentRequest { Content = "Intento" };

        await Assert.ThrowsAsync<CommentOwnershipException>(() => service.UpdateCommentAsync(comment.Id, users[0].Id, updateRequest));
    }

    [Fact]
    public async Task Delete_OwnComment_Succeeds()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        var request = new DTOs.Comments.CreateCommentRequest { Content = "A eliminar" };
        var comment = await service.CreateCommentAsync(post.Id, users[1].Id, request);

        await service.DeleteCommentAsync(comment.Id, users[1].Id);

        var stored = await db.Comments.AsNoTracking().FirstAsync(c => c.Id == comment.Id);
        Assert.NotNull(stored.DeletedAt);
    }

    [Fact]
    public async Task Delete_CommentOfOther_Fails()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        var request = new DTOs.Comments.CreateCommentRequest { Content = "Ajeno" };
        var comment = await service.CreateCommentAsync(post.Id, users[1].Id, request);

        await Assert.ThrowsAsync<CommentOwnershipException>(() => service.DeleteCommentAsync(comment.Id, users[0].Id));
    }
}
