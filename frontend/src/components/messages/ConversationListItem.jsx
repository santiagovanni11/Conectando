import { Link } from 'react-router-dom'
import { ROUTES } from '../../constants/routes'
import {
  conversationPreview,
  conversationTitle,
  formatConversationTime,
} from '../../constants/messages'
import SeenTicks from './SeenTicks'
import MuteConversationButton from './MuteConversationButton'
import ConversationAvatar from './ConversationAvatar'

/**
 * Fila de la lista de conversaciones.
 *
 * Va foto + nombre + hora arriba y previsualización + contadores abajo, como
 * en WhatsApp e Instagram. La foto va sola y no junto al nombre porque acá el
 * nombre comparte fila con la hora; en el encabezado del chat, en cambio, van
 * juntos. Por eso son dos componentes y no uno con dos variantes.
 */
export default function ConversationListItem({ conversation, currentUserId, onMute }) {
  const preview = conversationPreview(conversation)
  const sender = conversation.lastMessage?.sender?.displayName

  // El tick solo va en el último mensaje si es mío y el otro ya lo leyó.
  const showSeen =
    conversation.lastMessage?.sender?.id === currentUserId &&
    conversation.lastMessage?.isSeenByPeer

  return (
    <li className="conversation-item">
      <Link
        to={ROUTES.conversation(conversation.id)}
        className="conversation-item__link"
        aria-label={`Abrir conversación con ${conversationTitle(conversation)}`}
      >
        <ConversationAvatar conversation={conversation} />

        <span className="conversation-item__body">
          <span className="conversation-item__header">
            <span className="conversation-item__title">
              {conversationTitle(conversation)}
            </span>
            <time
              className="conversation-item__time"
              dateTime={conversation.updatedAt}
            >
              {formatConversationTime(conversation.updatedAt)}
            </time>
          </span>

          <span className="conversation-item__footer">
            <span className="conversation-item__preview">
              {sender ? `${sender.split(' ')[0]}: ` : ''}
              {preview}
            </span>

            <span className="conversation-item__trailing">
              {showSeen && (
                <SeenTicks />
              )}

              {onMute && (
                <MuteConversationButton
                  conversationId={conversation.id}
                  isMuted={conversation.isMuted}
                  onChanged={onMute}
                />
              )}

              {conversation.unreadCount > 0 && (
                <span className="conversation-item__badge">
                  {conversation.unreadCount}
                </span>
              )}
            </span>
          </span>
        </span>
      </Link>
    </li>
  )
}