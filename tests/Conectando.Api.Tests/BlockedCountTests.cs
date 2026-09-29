using Conectando.Api.Data;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;

namespace Conectando.Api.Tests;

/// <summary>
/// Conteo de un perfil con bloqueos.
/// </summary>
/// <remarks>
/// Va aparte de <see cref="BlockedVisibilityTests"/> porque mide otra cosa:
/// aquellos preguntan "quien aparece en la lista", estos "cuantos cuenta".
/// Lo que los une es que el numero tiene que bajar junto con la lista: si la
/// lista oculta al bloqueado pero el conteo no, se ve inconsistente y el
/// bloqueo parece no haber ocurrido.
/// </remarks>
[Collection("Social")]
public class BlockedCountTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task GetCounts_SubtractsBlockedFriends()
    {
        // El numero tiene que bajar junto con la lista.
        await using var db = fixture.CreateContext();
        var service = new ProfileCountService(db);
        var users = await TestUsers.SeedAsync(db, 3);
        // users[0] tiene dos amigos: users[1] y users[2].
        await SocialLinkTestData.SeedFriendshipAsync(db, users[0].Id, users[1].Id);
        await SocialLinkTestData.SeedFriendshipAsync(db, users[0].Id, users[2].Id);
        // Quien mira es users[2], que bloqueo a users[1]. Como users[1] es
        // amigo de users[0], deja de contarse en esa lista: 2 -> 1.
        await BlockTestData.SeedBlockAsync(db, blockerId: users[2].Id, blockedId: users[1].Id);

        var counts = await service.GetCountsAsync(users[0].Id, users[2].Id);

        Assert.Equal(1, counts.FriendsCount);
    }

    [Fact]
    public async Task GetCounts_UnblockedViewer_KeepsEveryone()
    {
        // El control del caso anterior: sin bloqueo los dos amigos cuentan.
        // La base de pruebas es compartida, asi que este caso fija la base
        // contra la que comparar el filtrado.
        await using var db = fixture.CreateContext();
        var service = new ProfileCountService(db);
        var users = await TestUsers.SeedAsync(db, 3);
        await SocialLinkTestData.SeedFriendshipAsync(db, users[0].Id, users[1].Id);
        await SocialLinkTestData.SeedFriendshipAsync(db, users[0].Id, users[2].Id);

        var counts = await service.GetCountsAsync(users[0].Id, users[2].Id);

        Assert.Equal(2, counts.FriendsCount);
    }

    [Fact]
    public async Task GetCounts_SubtractsBlockedFollowers()
    {
        await using var db = fixture.CreateContext();
        var service = new ProfileCountService(db);
        var users = await TestUsers.SeedAsync(db, 3);
        await SocialLinkTestData.SeedFollowAsync(db, users[1].Id, users[0].Id);
        await SocialLinkTestData.SeedFollowAsync(db, users[2].Id, users[0].Id);
        await BlockTestData.SeedBlockAsync(db, blockerId: users[1].Id, blockedId: users[2].Id);

        // users[1] mira el perfil de users[0] y bloqueo a uno de sus
        // seguidores: ese seguidor no debe contar.
        var counts = await service.GetCountsAsync(users[0].Id, users[1].Id);

        Assert.Equal(1, counts.FollowersCount);
    }

    [Fact]
    public async Task GetCounts_IgnoresWhoBlockedMe()
    {
        // Si a vos te bloquean, sus numeros no cambian: el filtro es del lado
        // de quien bloqueo, no una represalia.
        await using var db = fixture.CreateContext();
        var service = new ProfileCountService(db);
        var users = await TestUsers.SeedAsync(db, 3);
        await SocialLinkTestData.SeedFollowAsync(db, users[0].Id, users[1].Id);
        // users[0] bloquea a users[2], que tambien lo sigue. Quien mira es
        // users[2]: no lo bloqueo, asi que users[0] sigue contando.
        await SocialLinkTestData.SeedFollowAsync(db, users[2].Id, users[1].Id);
        await BlockTestData.SeedBlockAsync(db, blockerId: users[0].Id, blockedId: users[2].Id);

        var counts = await service.GetCountsAsync(users[1].Id, users[2].Id);

        Assert.Equal(2, counts.FollowersCount);
    }

    [Fact]
    public async Task GetCounts_KeepsPostsCount()
    {
        // Los posts se ocultan por privacidad, no por bloqueo: el autor
        // nunca está en su propia lista de bloqueados.
        await using var db = fixture.CreateContext();
        var service = new ProfileCountService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        await PostTestData.SeedPostAsync(db, users[0], "Uno", PostPrivacy.Public);
        await PostTestData.SeedPostAsync(db, users[0], "Dos", PostPrivacy.Public);
        await BlockTestData.SeedBlockAsync(db, blockerId: users[1].Id, blockedId: users[0].Id);

        var counts = await service.GetCountsAsync(users[0].Id, users[1].Id);

        Assert.Equal(2, counts.PostsCount);
    }
}
