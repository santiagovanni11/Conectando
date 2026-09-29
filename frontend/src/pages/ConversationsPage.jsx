import { useState } from 'react'
import { useAuth } from '../hooks/useAuth'
import { useConversations } from '../hooks/useConversations'
import { useFriendsList } from '../hooks/useFriendsList'
import ConversationListItem from '../components/messages/ConversationListItem'
import StartConversationPanel from '../components/messages/StartConversationPanel'
import ErrorState from '../components/ui/ErrorState'

/** Página de lista: /messages */
export default function ConversationsPage() {
  const { user } = useAuth()
  const { conversations, isLoading, error, setMuted } = useConversations()
  const friends = useFriendsList()
  const [tab, setTab] = useState('chats')

  return (
    <section className="conversations-page">
      <h1 className="conversations-page__title">Mensajes</h1>

      <div className="conversations-page__tabs" role="tablist" aria-label="Mensajes">
        <button
          type="button"
          role="tab"
          aria-selected={tab === 'chats'}
          className={`conversations-page__tab${tab === 'chats' ? ' is-active' : ''}`}
          onClick={() => setTab('chats')}
        >
          Chats
        </button>
        <button
          type="button"
          role="tab"
          aria-selected={tab === 'new'}
          className={`conversations-page__tab${tab === 'new' ? ' is-active' : ''}`}
          onClick={() => setTab('new')}
        >
          Nuevo chat
        </button>
      </div>

      {tab === 'new' ? (
        <StartConversationFriends friends={friends} />
      ) : (
        <ConversationChats
          conversations={conversations}
          currentUserId={user?.id}
          isLoading={isLoading}
          error={error}
          onMute={setMuted}
        />
      )}
    </section>
  )
}

function StartConversationFriends({ friends }) {
  if (friends.status === 'error') {
    return <ErrorState description={friends.error} onRetry={friends.refresh} />
  }

  return <StartConversationPanel friends={friends.friends} status={friends.status} />
}

function ConversationChats({ conversations, currentUserId, isLoading, error, onMute }) {
  if (isLoading) return <p className="conversations-page__status">Cargando…</p>

  if (error) {
    return <ErrorState description="No se pudieron cargar tus conversaciones." />
  }

  if (conversations.length === 0) {
    return (
      <p className="conversations-page__status">
        Todavía no tenés conversaciones. Usá <strong>Nuevo chat</strong> para empezar.
      </p>
    )
  }

  return (
    <ul className="conversations-list">
      {conversations.map((conversation) => (
        <ConversationListItem
          key={conversation.id}
          conversation={conversation}
          currentUserId={currentUserId}
          onMute={(isMuted) => onMute(conversation.id, isMuted)}
        />
      ))}
    </ul>
  )
}