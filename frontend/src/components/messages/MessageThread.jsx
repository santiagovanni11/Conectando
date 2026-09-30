import { useState } from 'react'
import MessageThreadRow from './MessageThreadRow'

/**
 * Lista del hilo.
 *
 * La edición es en línea, como en WhatsApp: el mensaje se convierte en un
 * campo de texto y se confirma al guardar o al cancelar. El estado de la
 * edición es de la lista y no de la fila, porque solo puede haber una
 * edición abierta a la vez en todo el hilo.
 */
export default function MessageThread({
  messages,
  currentUserId,
  isLoading,
  isLoadingMore,
  hasMore,
  onLoadMore,
  onEdit,
  onDelete,
  onReply,
  actionError,
}) {
  const [editingId, setEditingId] = useState(null)
  const [draft, setDraft] = useState('')

  const startEditing = (message) => {
    setEditingId(message.id)
    setDraft(message.content)
  }

  const saveEdit = async (messageId) => {
    // Si el servidor rechaza —pasada la ventana de 15 minutos, o sin
    // permiso— el formulario sigue abierto: así el texto escrito no se
    // pierde y el error queda a la vista, junto a la edición.
    const saved = await onEdit?.(messageId, draft)
    if (saved === false) return

    setEditingId(null)
    setDraft('')
  }

  if (isLoading) {
    return <p className="message-thread__status">Cargando mensajes…</p>
  }

  if (messages.length === 0) {
    return <p className="message-thread__status">Todavía no hay mensajes. Decí hola.</p>
  }

  return (
    <>
      {hasMore && (
        <button
          type="button"
          className="message-thread__more"
          onClick={onLoadMore}
          disabled={isLoadingMore}
        >
          {isLoadingMore ? 'Cargando…' : 'Cargar mensajes anteriores'}
        </button>
      )}

      {/* El <ul> es el contenedor en columna: por eso cada mensaje debe
          ser un <li> propio y no quedar anidado dentro de otro. */}
      <ul className="message-thread">
        {messages.map((message) => (
          <li key={message.id} className="message-thread__item">
            <MessageThreadRow
              message={message}
              isOwn={message.sender?.id === currentUserId}
              isEditing={editingId === message.id}
              error={editingId === message.id ? actionError : ''}
              draft={draft}
              onDraftChange={setDraft}
              onStartEdit={() => startEditing(message)}
              onCancelEdit={() => setEditingId(null)}
              onSaveEdit={() => saveEdit(message.id)}
              onDelete={onDelete}
              onReply={onReply}
            />
          </li>
        ))}
      </ul>
    </>
  )
}
