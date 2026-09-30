import MessageBubble from './MessageBubble'
import MessageSwipeRow from './MessageSwipeRow'
import { MAX_MESSAGE_LENGTH } from '../../constants/messages'

/**
 * Contenido de una fila: el gesto de responder, la burbuja normal o el
 * formulario de edición.
 *
 * Devuelve un <div> porque el <li> lo aporta el contenedor de arriba: dos
 * <li> anidados no son HTML válido y rompen el flex.
 *
 * Mientras se edita no hay gesto: el dedo está en el textarea, no en la
 * burbuja, y activar los dos gestos a la vez se pelea.
 */
export default function MessageThreadRow({
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
  onReply,
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
    <MessageSwipeRow onReply={() => onReply?.(message)} disabled={message.isDeleted}>
      <MessageBubble
        message={message}
        isOwn={isOwn}
        onEdit={onStartEdit}
        onDelete={onDelete ? () => onDelete(message.id) : undefined}
        onReply={onReply}
      />
    </MessageSwipeRow>
  )
}
