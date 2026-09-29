using Conectando.Api.Data;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class PagePreviewServiceTests(SocialTestFixture fixture)
{
    private PagePreviewService CreateService(ConectandoDbContext db) => new(db);

    [Fact]
    public async Task GetPost_PublicPost_ReturnsTitleAndContent()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Una foto linda", PostPrivacy.Public);

        var preview = await service.GetPostAsync(post.Id, users[1].Id);

        Assert.NotNull(preview);
        Assert.Equal("article", preview.Type);
        Assert.Contains("Una foto linda", preview.Description);
        Assert.Contains(users[0].DisplayName, preview.Title);
        Assert.Equal($"/posts/{post.Id}", preview.CanonicalUrl);
    }

    [Fact]
    public async Task GetPost_PrivatePost_ReturnsNull()
    {
        // Un preview se comparte por WhatsApp: no puede revelar un post privado.
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Secreto", PostPrivacy.Private);

        var preview = await service.GetPostAsync(post.Id, users[1].Id);

        Assert.Null(preview);
    }

    [Fact]
    public async Task GetPost_LongContent_IsTruncated()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 1);
        var post = await PostTestData.SeedPostAsync(db, users[0], new string('a', 500), PostPrivacy.Public);

        var preview = await service.GetPostAsync(post.Id, users[0].Id);

        Assert.NotNull(preview);
        Assert.True(preview.Description.Length < 210);
        Assert.EndsWith("…", preview.Description);
    }

    [Fact]
    public async Task GetProfile_ReturnsNameAndUsername()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 1);

        var preview = await service.GetProfileAsync(users[0].Id);

        Assert.NotNull(preview);
        Assert.Contains(users[0].UserName, preview.Title);
        Assert.Equal($"/users/{users[0].Id}", preview.CanonicalUrl);
    }

    [Fact]
    public async Task GetProfile_PrivateAccount_ReturnsNull()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 1);

        users[0].IsPrivate = true;
        await db.SaveChangesAsync();

        var preview = await service.GetProfileAsync(users[0].Id);

        Assert.Null(preview);
    }

    [Fact]
    public async Task GetProfile_Missing_ReturnsNull()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);

        Assert.Null(await service.GetProfileAsync(Guid.NewGuid()));
    }
}