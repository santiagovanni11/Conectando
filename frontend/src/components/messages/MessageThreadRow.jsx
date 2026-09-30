import MessageBubble from './MessageBubble'
import { useSwipeToReply } from '../../hooks/useSwipeToReply'
import { MAX_MESSAGE_LENGTH } from '../../constants/messages'

/**
 * Una fila del hilo: el <li>, con el gesto de responder.
 *
 * El gesto vive acá y no en un envoltorio aparte a propósito. Meter un <div>
 * entre el <li> y la burbuja rompía dos cosas que ya funcionaban: el
 * `max-width: 75%` de la burbuja pasaba a resolverse contra un contenedor
 * angosto y se encogía, y el `row`/`row-reverse` del <li> —que es lo que
 * distingue propios de ajenos— dejaba de mandar. Moviendo el mismo <li> de
 * lugar, el layout es idéntico al que ya estaba.
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
  const startReply = () => onReply?.(message)
  const { shift, swipeProps } = useSwipeToReply(startReply, {
    enabled: !message.isDeleted,
  })

  return (
    <li
      className="message-thread__item"
      style={{ '--swipe': `${shift}px` }}
      {...swipeProps}
    >
      {/* Sale del lado contrario a la burbuja, como en WhatsApp: si el
          mensaje está pegado al borde, al otro lado es donde hay hueco. */}
      <span className="message-row__hint" aria-hidden="true">
        ↩
      </span>

      {isEditing ? (
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
      ) : (
        <MessageBubble
          message={message}
          isOwn={isOwn}
          onEdit={onStartEdit}
          onDelete={onDelete ? () => onDelete(message.id) : undefined}
          // Va envuelto igual que el gesto: el menú lo llama sin argumentos,
          // y la página necesita saber a qué mensaje responde.
          onReply={startReply}
        />
      )}
    </li>
  )
}
