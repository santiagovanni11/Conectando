using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conectando.Api.Data.Configuration;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()");

        builder.HasOne(m => m.Conversation)
            .WithMany()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.NoAction);

        // La respuesta apunta al original; no lo reemplaza. Al borrarse el
        // original la referencia queda en null y la cita muestra "Mensaje
        // eliminado". Si fuera Cascade, borrar un mensaje llevaría por delante
        // todas las respuestas que lo citan.
        builder.HasOne(m => m.ReplyToMessage)
            .WithMany()
            .HasForeignKey(m => m.ReplyToMessageId)
            .OnDelete(DeleteBehavior.SetNull);

        // Historial paginado por conversación, del más nuevo al más viejo.
        builder.HasIndex(m => new { m.ConversationId, m.CreatedAt, m.Id })
            .HasDatabaseName("IX_messages_ConversationId_CreatedAt_Id");

        builder.HasIndex(m => m.ReplyToMessageId)
            .HasDatabaseName("IX_messages_ReplyToMessageId");
    }
}