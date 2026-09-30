import ConversationAvatar from './ConversationAvatar'
import { conversationTitle } from '../../constants/messages'

/**
 * Identidad de una conversación: foto y nombre, como en WhatsApp.
 *
 * Es el mismo componente en la lista y en el encabezado del chat. Se
 * separa del resto para que cambiar cómo se identifica a alguien —por
 * ejemplo, sumar el estado en línea— sea tocar un archivo, no dos.
 */
export default function ConversationIdentity({ conversation, size = 'md' }) {
  return (
    <span className="conversation-identity">
      <ConversationAvatar conversation={conversation} size={size} />
      <span className="conversation-identity__name">
        {conversationTitle(conversation)}
      </span>
    </span>
  )
}
