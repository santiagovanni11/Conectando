using Conectando.Api.Data;
using Conectando.Api.DTOs.Posts;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

/// <summary>
/// Que una cuenta dada de baja no aparezca por ningún lado.
///
/// <para>
/// Estas pruebas no pasan por la limpieza de <c>DeleteAccountPurgeTests</c> a
/// propósito: marcan la cuenta como dada de baja y dejan todo lo que tenía
/// en la base, como quedó en las cuentas borradas antes de que la limpieza
/// existiera. Si el filtro global no aguanta, la cuenta sigue apareciendo en
/// el feed y en las listas de otros.
/// </para>
///
/// <para>
/// Va en archivo propio porque la garantía no depende de la limpieza: es del
/// modelo. Limpiar y filtrar son dos cosas distintas y hacen falta las dos.
/// Limpiar sola no alcanza para lo que ya quedó, y filtrar solo dejaría
/// archivos huérfanos pagando en el almacenamiento.
/// </para>
/// </summary>
[Collection("Social")]
public class CuentasEliminadasFiltroTests(SocialTestFixture fixture)
{
    [Fact]
    public async Task SusPublicacionesNoAparecen()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(
            db, users[0], "Publicado antes de irme", PostPrivacy.Public);

        await BajaSinLimpiar.AplicarAsync(db, users[0]);

        // La fila sigue físicamente en la base —nadie la borró— pero la app no
        // la ve. Por eso la primera comprobación busca sin filtro y la
        // segunda se asegura de que la fila está, que es lo que hace que esto
        // sea un filtro y no un borrado encubierto.
        var id = users[0].Id;
        Assert.False(await db.Posts.AnyAsync(p => p.AuthorId == id));
        Assert.True(await db.Posts.IgnoreQueryFilters().AnyAsync(p => p.Id == post.Id));
    }

    [Fact]
    public async Task NoCuentaComoSeguidoraNiComoAmiga()
    {
        // Los números tienen que bajar solos, sin que nadie venga a
        // recalcularlos: el filtro los saca de los dos conteos.
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        await SocialLinkTestData.SeedFollowAsync(db, users[0].Id, users[1].Id);
        await SocialLinkTestData.SeedFriendshipAsync(db, users[0].Id, users[1].Id);

        await BajaSinLimpiar.AplicarAsync(db, users[0]);

        var id = users[0].Id;
        Assert.False(await db.Follows.AnyAsync(f => f.UserId == id || f.TargetUserId == id));
        Assert.False(await db.Friendships.AnyAsync(f => f.UserLowId == id || f.UserHighId == id));
    }

    [Fact]
    public async Task NoCuentaComoSolicitanteNiComoDestinatario()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        db.FriendRequests.Add(new FriendRequest
        {
            RequesterId = users[0].Id,
            AddresseeId = users[1].Id,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        await BajaSinLimpiar.AplicarAsync(db, users[0]);

        var id = users[0].Id;
        Assert.False(await db.FriendRequests
            .AnyAsync(r => r.RequesterId == id || r.AddresseeId == id));
    }

    [Fact]
    public async Task SusMeGustaNoAparecen()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(
            db, users[1], "Post del otro", PostPrivacy.Public);
        await LikeTestData.SeedLikeAsync(db, users[0].Id, post.Id);

        await BajaSinLimpiar.AplicarAsync(db, users[0]);

        var id = users[0].Id;
        Assert.False(await db.Likes.AnyAsync(l => l.UserId == id));
    }

    [Fact]
    public async Task LaCuentaNoSeBuscaNiSeLista()
    {
        // Tampoco por id: un perfil dado de baja tiene que dar 404, igual que
        // si la fila no existiera.
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 1);

        await BajaSinLimpiar.AplicarAsync(db, users[0]);

        var id = users[0].Id;
        Assert.False(await db.Users.AnyAsync(u => u.Id == id));
    }

    [Fact]
    public async Task ElFeedNoLasDevuelve()
    {
        // La prueba que importa de verdad: no que la consulta directa las
        // esconda, sino que el servicio que usa la app no las devuelva. Con
        // el filtro puesto a mano en cada consulta esto también pasaría, y un
        // endpoint nuevo las mostraría sin que nadie se entere.
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var suyo = await PostTestData.SeedPostAsync(
            db, users[0], "Publicado antes de irme", PostPrivacy.Public);
        var ajeno = await PostTestData.SeedPostAsync(
            db, users[1], "Post de quien sigue aquí", PostPrivacy.Public);

        await BajaSinLimpiar.AplicarAsync(db, users[0]);

        var read = PostServices.Create(db).Read;

        // Ni siquiera se puede pedir la lista: da 404, como si la cuenta no
        // hubiera existido. Es más fuerte que devolverla vacía.
        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            read.ListByUserAsync(users[0].Id, users[1].Id, new PostListQuery()));

        var delActivo = await read.ListByUserAsync(users[1].Id, users[1].Id, new PostListQuery());

        Assert.Contains(delActivo.Items, p => p.Id == ajeno.Id);
        Assert.DoesNotContain(delActivo.Items, p => p.Id == suyo.Id);
    }
}