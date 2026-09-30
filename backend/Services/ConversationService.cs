using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

/// <summary>
/// Conversaciones y mensajes. Se parte en partial class para que
/// ningún archivo crezca sin límite.
/// </summary>
public partial class ConversationService(
    ConectandoDbContext dbContext,
    IBlockService blockService) : IConversationService
{
    private const int MaxLimit = 100;
    private readonly ConectandoDbContext _dbContext = dbContext;
    private readonly IBlockService _blockService = blockService;

    /// <summary>
    /// Elimina un chat solo para quien lo pide, como en WhatsApp. La otra
    /// persona lo sigue teniendo; si vuelve a escribir, el chat reaparece
    /// para quien lo había borrado.
    /// </summary>
    public async Task DeleteConversationAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var member = await _dbContext.ConversationMembers
            .FirstOrDefaultAsync(
                m => m.ConversationId == conversationId && m.UserId == userId,
                cancellationToken);

        if (member is null) throw new ConversationNotFoundException();

        member.DeletedAt = DatabaseTime.UtcNow();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Silencia o reactiva un chat. No borra nada: el otro sigue escribiendo
    /// y los mensajes se siguen viendo, solo deja de llegar la notificación.
    /// </summary>
    public async Task<bool> SetMutedAsync(
        Guid userId,
        Guid conversationId,
        bool muted,
        CancellationToken cancellationToken = default)
    {
        var member = await _dbContext.ConversationMembers
            .FirstOrDefaultAsync(
                m => m.ConversationId == conversationId && m.UserId == userId,
                cancellationToken);

        if (member is null) throw new ConversationNotFoundException();

        member.MutedAt = muted ? DatabaseTime.UtcNow() : null;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return muted;
    }

    public async Task MarkAsReadAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var member = await _dbContext.ConversationMembers
            .FirstOrDefaultAsync(
                m => m.ConversationId == conversationId && m.UserId == userId,
                cancellationToken);

        if (member is null) throw new ConversationNotFoundException();

        member.LastReadAt = DatabaseTime.UtcNow();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
