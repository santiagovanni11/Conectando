using Conectando.Api.Data;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

/// <summary>
/// Lectura del listado de conversaciones y del historial de un hilo.
/// Las pruebas de escritura viven en ConversationServiceWriteTests.
/// </summary>
[Collection("Social")]
public class ConversationServiceReadTests(SocialTestFixture fixture)
{
    private ConversationService CreateService(ConectandoDbContext db) =>
        new(db, new BlockService(db));

    [Fact]
    public async Task GetConversations_ShowsPeersAndUnreadCount()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);
        await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[1].Id, "Hola");

        var list = await service.GetConversationsAsync(users[0].Id);

        var dto = Assert.Single(list);
        Assert.Equal(1, dto.UnreadCount);
        Assert.Equal(users[1].Id, Assert.Single(dto.Peers).Id);
        Assert.Equal("Hola", dto.LastMessage?.Content);
    }

    [Fact]
    public async Task GetMessages_PaginatesWithCursor()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);
        var conversation = await ConversationTestData.SeedConversationAsync(db, users[0].Id, users[1].Id);

        for (var i = 0; i < 5; i++)
        {
            await ConversationTestData.SeedMessageAsync(db, conversation.Id, users[0].Id, $"Mensaje {i}");
        }

        var firstPage = await service.GetMessagesAsync(users[0].Id, conversation.Id, null, 2);

        Assert.Equal(2, firstPage.Items.Count);
        Assert.True(firstPage.HasMore);
        Assert.NotNull(firstPage.NextCursor);

        var secondPage = await service.GetMessagesAsync(users[0].Id, conversation.Id, firstPage.NextCursor, 2);

        Assert.Equal(2, secondPage.Items.Count);
        // Sin repetir mensajes entre páginas.
        Assert.Empty(firstPage.Items.Select(m => m.Id).Intersect(secondPage.Items.Select(m => m.Id)));
    }
}