using Conectando.Api.Data;
using Conectando.Api.DTOs.Account;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

/// <summary>
/// Que al dar de baja una cuenta no quede nada de ella.
///
/// <para>
/// Va aparte de <see cref="DeleteAccountTests"/> porque ese mira la cuenta en
/// sí —el acceso, los datos personales— y este mira lo que dejó alrededor:
/// publicaciones, relaciones y archivos. Son dos preguntas distintas, y en un
/// solo archivo las pruebas de la segunda se mezclan con las de la primera.
/// </para>
/// </summary>
[Collection("Social")]
public class DeleteAccountPurgeTests(SocialTestFixture fixture)
{
    private static (AccountService Servicio, FakeMediaCleaner Almacen) Crear(ConectandoDbContext db)
    {
        var almacen = new FakeMediaCleaner();
        return (new AccountService(db, almacen), almacen);
    }

    private static DeleteAccountRequest Request() => new()
    {
        CurrentPassword = TestUsers.ValidPassword,
        ConfirmPassword = TestUsers.ValidPassword,
    };

    [Fact]
    public async Task Desaparecen_SusPublicaciones()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Mi publicación", PostPrivacy.Public);

        var (servicio, _) = Crear(db);
        await servicio.DeleteAccountAsync(users[0].Id, Request());

        Assert.False(await db.Posts.AnyAsync(p => p.Id == post.Id));
    }

    [Fact]
    public async Task Desaparecen_SusComentariosEnPostsDeOtros()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[1], "Post del otro", PostPrivacy.Public);
        db.Comments.Add(new Comment
        {
            PostId = post.Id,
            AuthorId = users[0].Id,
            Content = "Mi comentario",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var (servicio, _) = Crear(db);
        await servicio.DeleteAccountAsync(users[0].Id, Request());

        // El post del otro queda: no es suyo para borrarlo.
        Assert.True(await db.Posts.AnyAsync(p => p.Id == post.Id));
        Assert.False(await db.Comments.AnyAsync(c => c.AuthorId == users[0].Id));
    }

    [Fact]
    public async Task Desaparece_DeLaListaDeSeguidores()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        await SocialLinkTestData.SeedFollowAsync(db, users[0].Id, users[1].Id);
        await SocialLinkTestData.SeedFollowAsync(db, users[1].Id, users[0].Id);

        var (servicio, _) = Crear(db);
        await servicio.DeleteAccountAsync(users[0].Id, Request());

        // De las dos puntas: si solo se borrara del lado de quien siguió, la
        // cuenta seguiría apareciendo como seguidora en la lista del otro.
        Assert.False(await db.Follows.AnyAsync(f =>
            f.UserId == users[0].Id || f.TargetUserId == users[0].Id));
    }

    [Fact]
    public async Task Desaparece_DeAmigosYSolicitudes()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        await SocialLinkTestData.SeedFriendshipAsync(db, users[0].Id, users[1].Id);
        db.FriendRequests.Add(new FriendRequest
        {
            RequesterId = users[0].Id,
            AddresseeId = users[1].Id,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var (servicio, _) = Crear(db);
        await servicio.DeleteAccountAsync(users[0].Id, Request());

        Assert.False(await db.Friendships.AnyAsync(f =>
            f.UserLowId == users[0].Id || f.UserHighId == users[0].Id));
        Assert.False(await db.FriendRequests.AnyAsync(r =>
            r.RequesterId == users[0].Id || r.AddresseeId == users[0].Id));
    }

    [Fact]
    public async Task SeBordan_SusArchivos()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 1);
        await PostTestData.SeedPendingAsync(db, users[0], 1);

        var (servicio, almacen) = Crear(db);
        await servicio.DeleteAccountAsync(users[0].Id, Request());

        // Sin esto la fila desaparece y el archivo sigue en Cloudinary
        // pagando por siempre, sin que nada lo diga.
        Assert.NotEmpty(almacen.Borrados);
        Assert.False(await db.PostMedia.AnyAsync(m => m.UserId == users[0].Id));
    }

    [Fact]
    public async Task ElChatDelOtro_SigueExistiendo()
    {
        // La contrapartida de todo lo de arriba: el chat no se toca, porque es
        // también historial de quien se queda.
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(
            db, users[0].Id, users[1].Id);
        await ConversationTestData.SeedMessageAsync(
            db, conversation.Id, users[0].Id, "Antes de irme");

        var (servicio, _) = Crear(db);
        await servicio.DeleteAccountAsync(users[0].Id, Request());

        Assert.True(await db.Conversations.AnyAsync(c => c.Id == conversation.Id));
        Assert.Equal(1, await db.Messages.CountAsync(m => m.ConversationId == conversation.Id));
    }
}