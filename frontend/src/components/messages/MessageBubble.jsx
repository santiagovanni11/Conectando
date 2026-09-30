import SeenTicks from './SeenTicks'
import MessageMenu from './MessageMenu'
import MessageReplyQuote from './MessageReplyQuote'
import { DELETED_PLACEHOLDER } from '../../constants/messages'

/**
 * Burbuja de un mensaje. Alineada a la derecha si es propia.
 *
 * Es un <div> y no un <li> a propósito: el <li> de la fila lo aporta
 * MessageThreadRow. Dos <li> anidados no son HTML válido y el flex del
 * hilo los dejaba todos en una misma línea.
 *
 * Un mensaje borrado no muestra su texto ni menú de edición: se reemplaza
 * por un aviso, igual que en WhatsApp. Tampoco acepta respuestas ni gesto
 * de deslizar: no hay nada que citar.
 */
export default function MessageBubble({ message, isOwn, onEdit, onDelete, onReply }) {
  const time = new Date(message.createdAt).toLocaleTimeString([], {
    hour: '2-digit',
    minute: '2-digit',
  })

  if (message.isDeleted) {
    return (
      <div className={`message-bubble is-deleted ${isOwn ? 'is-own' : 'is-theirs'}`}>
        <p className="message-bubble__placeholder">{DELETED_PLACEHOLDER}</p>
        <time className="message-bubble__time" dateTime={message.createdAt}>
          {time}
        </time>
      </div>
    )
  }

  const mine = message.isSeenByPeer

  return (
    <div className={`message-bubble ${isOwn ? 'is-own' : 'is-theirs'}`}>
      {!isOwn && <span className="message-bubble__author">{message.sender?.displayName}</span>}

      <MessageReplyQuote replyTo={message.replyTo} />

      <p className="message-bubble__text">{message.content}</p>

      <span className="message-bubble__meta">
        {message.isEdited && <span className="message-bubble__edited">Editado</span>}

        <time className="message-bubble__time" dateTime={message.createdAt}>
          {time}
        </time>

        {/* El visto solo tiene sentido en los mensajes propios. */}
        {isOwn && (
          <span
            className={`message-bubble__seen ${mine ? 'is-seen' : ''}`}
            data-testid="seen-state"
          >
            {mine ? (
              <>
                <SeenTicks />
                <span className="message-bubble__seen-label">Visto</span>
              </>
            ) : (
              <span className="message-bubble__seen-label">Enviado</span>
            )}
          </span>
        )}
      </span>

      {(onEdit || onDelete || onReply) && (
        <MessageMenu
          isOwn={isOwn}
          onEdit={onEdit}
          onDelete={onDelete}
          onReply={onReply}
        />
      )}
    </div>
  )
}