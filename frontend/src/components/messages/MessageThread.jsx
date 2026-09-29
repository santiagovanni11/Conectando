import { useState } from 'react'
import MessageBubble from './MessageBubble'
import { MAX_MESSAGE_LENGTH } from '../../constants/messages'

/**
 * Lista del hilo.
 *
 * La edición es en línea, como en WhatsApp: el mensaje se convierte en un
 * campo de texto y se confirma al guardar o al cancelar.
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
            <ThreadRow
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
            />
          </li>
        ))}
      </ul>
    </>
  )
}

/**
 * Contenido de una fila: la burbuja normal o el formulario de edición.
 * Devuelve un <div> porque el <li> lo aporta el contenedor de arriba:
 * dos <li> anidados no son HTML válido y rompen el flex.
 */
function ThreadRow({
  message,
  isOwn,
  isEditing,
  error,
  draft,
  onDraftChange,
  onStartEdit,
  onCancelEdit,
  onSaveEdit,
  onDelete,
}) {
  if (isEditing) {
    return (
      <form
        className="message-edit"
        onSubmit={(event) => {
          event.preventDefault()
          onSaveEdit()
        }}
      >
        <textarea
          className="message-edit__input"
          value={draft}
          onChange={(event) => onDraftChange(event.target.value)}
          aria-label="Editar mensaje"
          maxLength={MAX_MESSAGE_LENGTH}
          autoFocus
        />
        {/* El error va junto a la edición: si apareciera solo en el
            compositor, abajo, el usuario creería que no se guardó nada. */}
        {error && (
          <p className="message-edit__error" role="alert">
            {error}
          </p>
        )}
        <span className="message-edit__actions">
          <button type="submit" disabled={!draft.trim()}>
            Guardar
          </button>
          <button type="button" onClick={onCancelEdit}>
            Cancelar
          </button>
        </span>
      </form>
    )
  }

  return (
    <MessageBubble
      message={message}
      isOwn={isOwn}
      onEdit={onStartEdit}
      onDelete={onDelete ? () => onDelete(message.id) : undefined}
    />
  )
}