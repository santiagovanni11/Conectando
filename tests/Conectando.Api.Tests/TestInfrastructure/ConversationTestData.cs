using Conectando.Api.Data;
using Conectando.Api.Models;

namespace Conectando.Api.Tests.TestInfrastructure;

public static class ConversationTestData
{
    public static async Task<Conversation> SeedConversationAsync(
        ConectandoDbContext db,
        params Guid[] memberIds)
    {
        var now = DateTime.UtcNow;
        var conversation = new Conversation { Id = Guid.NewGuid(), CreatedAt = now, UpdatedAt = now };

        db.Conversations.Add(conversation);
        db.ConversationMembers.AddRange(memberIds.Select(id => new ConversationMember
        {
            ConversationId = conversation.Id,
            UserId = id,
            JoinedAt = now,
        }));

        await db.SaveChangesAsync();
        return conversation;
    }

    public static async Task<Message> SeedMessageAsync(
        ConectandoDbContext db,
        Guid conversationId,
        Guid senderId,
        string content = "Hola")
    {
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
        };

        db.Messages.Add(message);
        await db.SaveChangesAsync();
        return message;
    }
}