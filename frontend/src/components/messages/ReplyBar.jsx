import Icon from '../ui/Icon/Icon'
import { DELETED_PLACEHOLDER } from '../../constants/messages'

/**
 * Barra sobre el composer: a qué mensaje se está respondiendo.
 *
 * Es la mitad de arriba de la respuesta, como en WhatsApp e Instagram. Va
 * sobre el composer y no dentro de la burbuja porque todavía no hay burbuja:
 * el mensaje se está escribiendo.
 */
export default function ReplyBar({ message, onCancel }) {
  if (!message) return null

  return (
    <div className="reply-bar">
      <Icon name="reply" size="sm" className="reply-bar__icon" />

      <span className="reply-bar__body">
        <span className="reply-bar__label">Respondiendo a {message.sender?.displayName}</span>
        <span className="reply-bar__preview">
          {message.isDeleted ? DELETED_PLACEHOLDER : message.content}
        </span>
      </span>

      <button
        type="button"
        className="reply-bar__cancel"
        onClick={onCancel}
        aria-label="Cancelar la respuesta"
      >
        ✕
      </button>
    </div>
  )
}
