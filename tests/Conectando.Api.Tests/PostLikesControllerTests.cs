using System.Security.Claims;
using Conectando.Api.Controllers;
using Conectando.Api.Data;
using Conectando.Api.DTOs.Posts;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class PostLikesControllerTests(SocialTestFixture fixture)
{
    private static PostLikesController CreateController(ConectandoDbContext db, Guid userId)
    {
        var service = new LikeService(db, new PostVisibilityService(db), new NullNotificationService());

        var controller = new PostLikesController(service);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "TestAuth")),
            },
        };

        return controller;
    }

    private static PostLikeStatusDto Status(ActionResult<PostLikeStatusDto> result)
    {
        // El controller devuelve Ok(...), así que el DTO viene envuelto
        // en un OkObjectResult y no en Value.
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        return Assert.IsType<PostLikeStatusDto>(ok.Value);
    }

    [Fact]
    public async Task Like_ReturnsStatusWithOneLike()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        var controller = CreateController(db, users[1].Id);

        var status = Status(await controller.Like(post.Id, default));

        Assert.True(status.LikedByMe);
        Assert.Equal(1, status.LikesCount);
    }

    [Fact]
    public async Task Unlike_ClearsTheLike()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        await LikeTestData.SeedLikeAsync(db, users[1].Id, post.Id);
        var controller = CreateController(db, users[1].Id);

        var status = Status(await controller.Unlike(post.Id, default));

        Assert.False(status.LikedByMe);
        Assert.Equal(0, status.LikesCount);
    }

    [Fact]
    public async Task GetStatus_UsesTheAuthenticatedUserNotTheAuthor()
    {
        // El id del usuario sale del token. Si el que consulta no es quien
        // dio like, likedByMe tiene que venir en false.
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Hola", PostPrivacy.Public);
        await LikeTestData.SeedLikeAsync(db, users[1].Id, post.Id);
        var controller = CreateController(db, users[0].Id);

        var status = Status(await controller.GetStatus(post.Id, default));

        Assert.Equal(1, status.LikesCount);
        Assert.False(status.LikedByMe);
    }
}