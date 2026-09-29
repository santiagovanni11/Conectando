using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class CommentReadTests(SocialTestFixture fixture)
{
    private CommentWriteService Create(ConectandoDbContext db)
    {
        return new CommentWriteService(db, new PostVisibilityService(db), new NullNotificationService());
    }

    [Fact]
    public async Task GetComments_ReturnsOrdered()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 3);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        var request1 = new DTOs.Comments.CreateCommentRequest { Content = "Primero" };
        var request2 = new DTOs.Comments.CreateCommentRequest { Content = "Segundo" };
        await service.CreateCommentAsync(post.Id, users[1].Id, request1);
        await service.CreateCommentAsync(post.Id, users[2].Id, request2);

        var result = await service.GetCommentsAsync(post.Id, users[0].Id, null, 20);

        Assert.Equal(2, result.Items.Count);
        Assert.True(result.Items[0].CreatedAt <= result.Items[1].CreatedAt);
    }

    [Fact]
    public async Task GetComments_ExcludesDeleted()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        var request = new DTOs.Comments.CreateCommentRequest { Content = "A eliminar" };
        var comment = await service.CreateCommentAsync(post.Id, users[1].Id, request);
        await service.DeleteCommentAsync(comment.Id, users[1].Id);

        var result = await service.GetCommentsAsync(post.Id, users[0].Id, null, 20);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetComments_Pagination()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        for (int i = 0; i < 5; i++)
        {
            var request = new DTOs.Comments.CreateCommentRequest { Content = $"Comentario {i}" };
            await service.CreateCommentAsync(post.Id, users[1].Id, request);
        }

        var page1 = await service.GetCommentsAsync(post.Id, users[0].Id, null, 2);
        Assert.Equal(2, page1.Items.Count);
        Assert.True(page1.HasMore);

        var page2 = await service.GetCommentsAsync(post.Id, users[0].Id, page1.NextCursor, 2);
        Assert.Equal(2, page2.Items.Count);
        Assert.True(page2.HasMore);

        var page3 = await service.GetCommentsAsync(post.Id, users[0].Id, page2.NextCursor, 2);
        Assert.Single(page3.Items);
        Assert.False(page3.HasMore);
    }

    [Fact]
    public async Task GetComments_BlockedUser_Fails()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await db.Blocks.AddAsync(new Block { UserId = users[0].Id, BlockedUserId = users[1].Id, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);

        await Assert.ThrowsAsync<PostNotFoundException>(() => service.GetCommentsAsync(post.Id, users[1].Id, null, 20));
    }

    [Fact]
    public async Task GetReplies_OnlyRepliesOfParent()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        var parentRequest = new DTOs.Comments.CreateCommentRequest { Content = "Padre" };
        var comment = await service.CreateCommentAsync(post.Id, users[1].Id, parentRequest);
        var replyRequest1 = new DTOs.Comments.CreateCommentRequest { Content = "Respuesta 1", ParentCommentId = comment.Id };
        var replyRequest2 = new DTOs.Comments.CreateCommentRequest { Content = "Respuesta 2", ParentCommentId = comment.Id };
        await service.CreateCommentAsync(post.Id, users[0].Id, replyRequest1);
        await service.CreateCommentAsync(post.Id, users[0].Id, replyRequest2);

        var replies = await service.GetRepliesAsync(comment.Id, users[0].Id, 10);

        Assert.Equal(2, replies.Count);
        Assert.All(replies, r => Assert.Equal(comment.Id, r.ParentCommentId));
    }

    [Fact]
    public async Task GetComments_NoSensitiveData()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        var request = new DTOs.Comments.CreateCommentRequest { Content = "Hola" };
        await service.CreateCommentAsync(post.Id, users[1].Id, request);

        var result = await service.GetCommentsAsync(post.Id, users[0].Id, null, 20);

        var comment = result.Items[0];
        Assert.Equal(users[1].Id, comment.Author.Id);
        Assert.Equal(users[1].UserName, comment.Author.UserName);
        Assert.Equal(users[1].DisplayName, comment.Author.DisplayName);
    }
}
