using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

/// <summary>
/// Me gusta de un comentario.
///
/// <para>
/// Lo que más importa acá es que no se pueda dar me gusta a un comentario de
/// algo que no se ve. Sin esa comprobación, alguien podría dar me gusta a un
/// post privado y enterarse de que existe solo por el número que cambia.
/// </para>
/// </summary>
[Collection("Social")]
public class CommentLikeTests(SocialTestFixture fixture)
{
    private static CommentLikeService Create(ConectandoDbContext db) =>
        new(db, new PostVisibilityService(db));

    private static async Task<Comment> SembrarComentario(
        ConectandoDbContext db, AppUser autor, Post post, string contenido)
    {
        var comment = new Comment
        {
            PostId = post.Id,
            AuthorId = autor.Id,
            Content = contenido,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        db.Comments.Add(comment);
        await db.SaveChangesAsync();
        return comment;
    }

    [Fact]
    public async Task Poner_YSacar_QuedaConsistente()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Post", PostPrivacy.Public);
        var comment = await SembrarComentario(db, users[0], post, "Un comentario");
        var service = Create(db);

        var puesto = await service.LikeAsync(comment.Id, users[1].Id);
        Assert.True(puesto.LikedByMe);
        Assert.Equal(1, puesto.LikesCount);

        var sacado = await service.UnlikeAsync(comment.Id, users[1].Id);
        Assert.False(sacado.LikedByMe);
        Assert.Equal(0, sacado.LikesCount);
    }

    [Fact]
    public async Task DosVeces_NoCuenta_Doble()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Post", PostPrivacy.Public);
        var comment = await SembrarComentario(db, users[0], post, "Un comentario");
        var service = Create(db);

        await service.LikeAsync(comment.Id, users[1].Id);
        var segundo = await service.LikeAsync(comment.Id, users[1].Id);

        // Dos toques seguidos no pueden dejar el número en dos.
        Assert.Equal(1, segundo.LikesCount);
    }

    [Fact]
    public async Task CadaUno_Cuenta_Una_Vez()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 3);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Post", PostPrivacy.Public);
        var comment = await SembrarComentario(db, users[0], post, "Un comentario");
        var service = Create(db);

        await service.LikeAsync(comment.Id, users[1].Id);
        var estado = await service.LikeAsync(comment.Id, users[2].Id);

        Assert.Equal(2, estado.LikesCount);
    }

    [Fact]
    public async Task EnUnPostPrivado_NoDeja_Dar_Like()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 3);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Privado", PostPrivacy.Private);
        var comment = await SembrarComentario(db, users[0], post, "Secreto");
        var service = Create(db);

        // Un cualquiera no puede ni ver el post. Si el me gusta no validara
        // visibilidad, el número le diría que el comentario existe.
        //
        // Dice "comentario no existe" y no "la publicación no existe": con un
        // mensaje distinto, alguien podría deducir que hay un post privado
        // solo por ver que los errores no son iguales.
        await Assert.ThrowsAsync<CommentNotFoundException>(() =>
            service.LikeAsync(comment.Id, users[2].Id));
    }

    [Fact]
    public async Task EnUnComentarioEliminado_NoDeja()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 2);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Post", PostPrivacy.Public);
        var comment = await SembrarComentario(db, users[0], post, "Se va");
        comment.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var service = Create(db);

        await Assert.ThrowsAsync<CommentNotFoundException>(() =>
            service.LikeAsync(comment.Id, users[1].Id));
    }

    [Fact]
    public async Task Una_Cuenta_Dada_De_Baja_No_Deja_El_Numero_Por_Arriba()
    {
        await using var db = fixture.CreateContext();
        var users = await TestUsers.SeedAsync(db, 3);
        var post = await PostTestData.SeedPostAsync(db, users[0], "Post", PostPrivacy.Public);
        var comment = await SembrarComentario(db, users[0], post, "Un comentario");
        var service = Create(db);

        await service.LikeAsync(comment.Id, users[1].Id);
        await BajaSinLimpiar.AplicarAsync(db, users[1]);

        // El filtro global saca el me gusta sin que haya que limpiar nada: el
        // número baja solo.
        var estado = await service.LikeAsync(comment.Id, users[2].Id);
        Assert.Equal(1, estado.LikesCount);
    }
}