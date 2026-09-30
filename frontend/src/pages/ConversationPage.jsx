import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { useConversationMessages } from '../hooks/useConversationMessages'
import { useTypingIndicator } from '../hooks/useTypingIndicator'
import { deleteConversation, fetchConversations } from '../services/conversationService'
import { ROUTES } from '../constants/routes'
import MessageThread from '../components/messages/MessageThread'
import MessageComposer from '../components/messages/MessageComposer'
import ConversationIdentity from '../components/messages/ConversationIdentity'
import {
  MessageReplyProvider,
  useMessageReply,
} from '../components/messages/MessageReplyContext'

/**
 * Página del hilo: /messages/:conversationId
 *
 * El provider va por fuera a propósito: `useMessageReply` se lee en
 * ConversationView, que es hija suya. Si el hook corriera en este mismo
 * componente, todavía no existiría el contexto que tiene que leer.
 */
export default function ConversationPage() {
  return (
    <MessageReplyProvider>
      <ConversationView />
    </MessageReplyProvider>
  )
}

function ConversationView() {
  const { conversationId } = useParams()
  const navigate = useNavigate()
  const { user } = useAuth()
  const thread = useConversationMessages(conversationId, user?.id)
  const typing = useTypingIndicator(conversationId, user?.id)
  const { replyTo, startReply, cancelReply } = useMessageReply()
  const [conversation, setConversation] = useState(null)
  const [deleteError, setDeleteError] = useState('')

  /**
   * Elimina el chat solo para este usuario y vuelve a la lista. Se pide
   * confirmación porque el gesto no tiene vuelta atrás en la pantalla.
   */
  const handleDeleteChat = async () => {
    const confirmed = window.confirm(
      '¿Eliminar esta conversación? Solo se borra para vos; la otra persona la conserva.',
    )
    if (!confirmed) return

    try {
      await deleteConversation(conversationId)
      navigate(ROUTES.messages)
    } catch (cause) {
      setDeleteError(cause.message ?? 'No se pudo eliminar la conversación.')
    }
  }

  useEffect(() => {
    fetchConversations()
      .then((list) => {
        const found = list.find((c) => c.id === conversationId)
        // Si el usuario no pertenece, la API lo rechaza: se vuelve atrás.
        if (!found) {
          navigate(ROUTES.messages, { replace: true })
          return
        }
        setConversation(found)
      })
      .catch(() => {})
  }, [conversationId, navigate])

  return (
    <section className="conversation-page">
      <header className="conversation-page__header">
        <button
          type="button"
          className="conversation-page__back"
          onClick={() => navigate(ROUTES.messages)}
        >
          Volver
        </button>
        {/* La identidad va dentro del <h1> para que el encabezado siga
            siendo el título de la página, y a la vez muestre la foto como
            en WhatsApp. El avatar es decorativo: el nombre ya está en el
            texto del encabezado. */}
        <h1 className="conversation-page__title">
          <ConversationIdentity
            conversation={conversation ?? { peers: [] }}
            size="sm"
          />
        </h1>

        <button
          type="button"
          className="conversation-page__delete"
          onClick={handleDeleteChat}
          aria-label="Eliminar conversación"
        >
          Eliminar
        </button>

        {/* Indicador en vivo, como el "escribiendo…" de WhatsApp. */}
        <p
          className={`conversation-page__typing${typing.peerIsTyping ? ' is-active' : ''}`}
          role="status"
          aria-live="polite"
        >
          {typing.peerIsTyping ? `${typing.peerName ?? 'Alguien'} está escribiendo…` : ''}
        </p>
      </header>

      <MessageThread
        messages={thread.messages}
        currentUserId={user?.id}
        isLoading={thread.isLoading}
        isLoadingMore={thread.isLoadingMore}
        hasMore={thread.hasMore}
        onLoadMore={thread.loadMore}
        onEdit={thread.edit}
        onDelete={thread.remove}
        onReply={startReply}
        actionError={thread.actionError}
        error={thread.error}
      />

      <MessageComposer
        onSend={thread.send}
        onTyping={typing.notify}
        replyTo={replyTo}
        onCancelReply={cancelReply}
        error={thread.sendError || thread.actionError || deleteError}
      />
    </section>
  )
}
