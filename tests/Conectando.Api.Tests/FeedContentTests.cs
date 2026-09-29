using Conectando.Api.Data;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;

namespace Conectando.Api.Tests;

/// <summary>
/// Qué datos trae cada publicación del feed: me gusta, medios y autor.
/// Las reglas de a quién se le muestra viven en FeedVisibilityTests.
/// </summary>
[Collection("Social")]
public class FeedContentTests(SocialTestFixture fixture)
{
    private static FeedReadService CreateService(ConectandoDbContext db)
    {
        return new FeedReadService(db, new FeedAuthorResolver(new PostVisibilityService(db)));
    }

    [Fact]
    public async Task Feed_IncludesLikesAndMedia()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 3);
        await PostTestData.SeedFollowAsync(db, users[0].Id, users[1].Id);
        var media = await PostTestData.SeedPendingAsync(db, users[1], 2);
        var post = await PostTestData.SeedPostAsync(db, users[1], "con fotos", PostPrivacy.Public, media: media);
        await LikeTestData.SeedLikeAsync(db, users[2].Id, post.Id);

        var page = await service.GetPageAsync(users[0].Id, null, 10);

        var item = Assert.Single(page.Items);
        Assert.Equal(1, item.LikesCount);
        Assert.False(item.LikedByMe);
        Assert.Equal(2, item.Media.Count);
        Assert.Equal("Usuario de prueba", item.Author.DisplayName);
    }
}