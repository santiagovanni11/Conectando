using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class CommentCreateTests(SocialTestFixture fixture)
{
    private CommentWriteService Create(ConectandoDbContext db)
    {
        return new CommentWriteService(db, new PostVisibilityService(db), new NullNotificationService());
    }

    [Fact]
    public async Task Create_OnOtherPost_Succeeds()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        var request = new DTOs.Comments.CreateCommentRequest { Content = "Buena foto" };

        var result = await service.CreateCommentAsync(post.Id, users[1].Id, request);

        Assert.Equal("Buena foto", result.Content);
        Assert.Equal(users[1].Id, result.AuthorId);
        Assert.Null(result.ParentCommentId);
    }

    [Fact]
    public async Task Create_OnOwnPost_Succeeds()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Mi post", PostPrivacy.Public);
        var request = new DTOs.Comments.CreateCommentRequest { Content = "Me encanta" };

        var result = await service.CreateCommentAsync(post.Id, users[0].Id, request);

        Assert.Equal("Me encanta", result.Content);
        Assert.Equal(users[0].Id, result.AuthorId);
    }

    [Fact]
    public async Task Create_Reply_Succeeds()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        var parentRequest = new DTOs.Comments.CreateCommentRequest { Content = "Primero" };
        var comment = await service.CreateCommentAsync(post.Id, users[1].Id, parentRequest);
        var replyRequest = new DTOs.Comments.CreateCommentRequest { Content = "Respondiendo", ParentCommentId = comment.Id };

        var reply = await service.CreateCommentAsync(post.Id, users[0].Id, replyRequest);

        Assert.Equal(comment.Id, reply.ParentCommentId);
        Assert.Equal("Respondiendo", reply.Content);
    }

    [Fact]
    public async Task Create_ReplyToReply_Fails()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        var parentRequest = new DTOs.Comments.CreateCommentRequest { Content = "Primero" };
        var comment = await service.CreateCommentAsync(post.Id, users[1].Id, parentRequest);
        var replyRequest = new DTOs.Comments.CreateCommentRequest { Content = "Respuesta", ParentCommentId = comment.Id };
        var reply = await service.CreateCommentAsync(post.Id, users[0].Id, replyRequest);

        var invalidRequest = new DTOs.Comments.CreateCommentRequest { Content = "No debe funcionar", ParentCommentId = reply.Id };
        await Assert.ThrowsAsync<InvalidParentCommentException>(() => service.CreateCommentAsync(post.Id, users[1].Id, invalidRequest));
    }

    [Fact]
    public async Task Create_EmptyContent_Fails()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        var request = new DTOs.Comments.CreateCommentRequest { Content = "   " };

        await Assert.ThrowsAsync<EmptyCommentException>(() => service.CreateCommentAsync(post.Id, users[1].Id, request));
    }

    [Fact]
    public async Task Create_TooLong_Fails()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        var request = new DTOs.Comments.CreateCommentRequest { Content = new string('a', 1001) };

        await Assert.ThrowsAsync<CommentTooLongException>(() => service.CreateCommentAsync(post.Id, users[1].Id, request));
    }

    [Fact]
    public async Task Create_OnPrivatePost_Fails()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Private);
        var request = new DTOs.Comments.CreateCommentRequest { Content = "Hola" };

        await Assert.ThrowsAsync<PostNotFoundException>(() => service.CreateCommentAsync(post.Id, users[1].Id, request));
    }

    [Fact]
    public async Task Create_OnNonExistentPost_Fails()
    {
        await using var db = fixture.CreateContext();
        var service = Create(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var request = new DTOs.Comments.CreateCommentRequest { Content = "Hola" };

        await Assert.ThrowsAsync<PostNotFoundException>(() => service.CreateCommentAsync(Guid.NewGuid(), users[0].Id, request));
    }
}
