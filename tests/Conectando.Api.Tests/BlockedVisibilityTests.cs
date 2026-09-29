using Conectando.Api.Data;
using Conectando.Api.DTOs.Posts;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class BlockedVisibilityTests(SocialTestFixture fixture)
{
    private UserConnectionsService CreateService(ConectandoDbContext db) =>
        new(db, new PostVisibilityService(db));

    [Fact]
    public async Task GetFriends_HidesBlockedUsers()
    {
        // Bloquear tiene que sacar a la persona de la lista de amigos del
        // propio usuario: si apareciera, el bloqueo sería decorativo.
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 3);
        await SocialLinkTestData.SeedFriendshipAsync(db, users[0].Id, users[1].Id);
        await SocialLinkTestData.SeedFriendshipAsync(db, users[0].Id, users[2].Id);
        await BlockTestData.SeedBlockAsync(db, blockerId: users[0].Id, blockedId: users[1].Id);

        var friends = await service.GetFriendsAsync(users[0].Id, users[0].Id);

        Assert.Equal(users[2].Id, Assert.Single(friends).Id);
    }

    [Fact]
    public async Task GetFollowers_HidesBlockedUsers()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 3);
        await SocialLinkTestData.SeedFollowAsync(db, users[1].Id, users[0].Id);
        await SocialLinkTestData.SeedFollowAsync(db, users[2].Id, users[0].Id);
        await BlockTestData.SeedBlockAsync(db, blockerId: users[0].Id, blockedId: users[1].Id);

        var followers = await service.GetFollowersAsync(users[0].Id, users[0].Id);

        Assert.Equal(users[2].Id, Assert.Single(followers).Id);
    }

    [Fact]
    public async Task GetFriends_IsSymmetricForTheBlockedProfile()
    {
        // El bloqueo es entre dos personas, no un castigo público: la otra
        // persona sigue viendo la amistad en su propia lista.
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await SocialLinkTestData.SeedFriendshipAsync(db, users[0].Id, users[1].Id);
        await BlockTestData.SeedBlockAsync(db, blockerId: users[0].Id, blockedId: users[1].Id);

        var asThem = await service.GetFriendsAsync(users[1].Id, users[1].Id);
        var asMe = await service.GetFriendsAsync(users[0].Id, users[0].Id);

        // El bloqueado conserva la amistad en su lista, pero el bloqueador
        // ya no lo ve: cada uno decide qué mostrar en su propio perfil.
        Assert.NotEmpty(asThem);
        Assert.Empty(asMe);
    }

    [Fact]
    public async Task Search_HidesBlockedUsers()
    {
        // Si el bloqueado apareciera en buscar, el bloqueo sería inútil:
        // alcanza con escribir su nombre.
        await using var db = fixture.CreateContext();
        var service = new UserDiscoveryService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await BlockTestData.SeedBlockAsync(db, blockerId: users[0].Id, blockedId: users[1].Id);

        var results = await service.SearchAsync(users[0].Id, users[1].UserName, 10);

        Assert.Empty(results);
    }

    [Fact]
    public async Task ListSaved_HidesPostsFromBlockedUsers()
    {
        // Guardar no es un atajo para seguir viendo lo de alguien bloqueado.
        await using var db = fixture.CreateContext();
        var service = new PostSaveService(db, new PostVisibilityService(db));
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Guardada", PostPrivacy.Public);

        await service.ToggleAsync(users[1].Id, post.Id);
        await BlockTestData.SeedBlockAsync(db, blockerId: users[1].Id, blockedId: users[0].Id);

        var page = await service.ListSavedAsync(users[1].Id, new PostListQuery());

        Assert.Empty(page.Items);
    }

    [Fact]
    public async Task ListSaved_KeepsPostsWhenSomeoneBlocksMe()
    {
        // El bloqueo va en una sola dirección: si me bloquean a mí, mis
        // publicaciones guardadas siguen ahí. Ocultarlas sería un castigo
        // por una acción que no hice.
        await using var db = fixture.CreateContext();
        var service = new PostSaveService(db, new PostVisibilityService(db));
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[1], "Mía", PostPrivacy.Public);

        await service.ToggleAsync(users[0].Id, post.Id);
        await BlockTestData.SeedBlockAsync(db, blockerId: users[1].Id, blockedId: users[0].Id);

        var page = await service.ListSavedAsync(users[0].Id, new PostListQuery());

        Assert.Single(page.Items);
    }
}
