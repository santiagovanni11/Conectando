import ConversationAvatar from './ConversationAvatar'
import { conversationTitle, conversationPeer } from '../../constants/messages'
import UserHandle from '../users/UserHandle'

/**
 * Identidad de una conversación: foto y nombre, como en WhatsApp.
 *
 * Es el mismo componente en la lista y en el encabezado del chat. Se
 * separa del resto para que cambiar cómo se identifica a alguien —por
 * ejemplo, sumar el estado en línea— sea tocar un archivo, no dos.
 *
 * <para>
 * El nombre lleva al perfil, como en cualquier mensajería. En los chats en
 * grupo no hay a quién ir: se muestra el nombre de la conversación y nada
 * más, sin enlace.
 */
export default function ConversationIdentity({ conversation, size = 'md' }) {
  const peer = conversationPeer(conversation)

  if (!peer) {
    return (
      <span className="conversation-identity">
        <ConversationAvatar conversation={conversation} size={size} />
        <span className="conversation-identity__name">
          {conversationTitle(conversation)}
        </span>
      </span>
    )
  }

  return (
    <span className="conversation-identity">
      <UserHandle user={peer} className="conversation-identity__avatar-link">
        <ConversationAvatar conversation={conversation} size={size} />
      </UserHandle>
      <UserHandle user={peer} className="conversation-identity__name">
        {conversationTitle(conversation)}
      </UserHandle>
    </span>
  )
}
