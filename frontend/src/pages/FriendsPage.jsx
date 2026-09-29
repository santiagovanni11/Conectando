import { useState } from 'react'
import FriendsList from '../components/friends/FriendsList'
import FriendRequestsPanel from '../components/friends/FriendRequestsPanel'
import FriendSuggestions from '../components/friends/FriendSuggestions'
import ErrorState from '../components/ui/ErrorState'
import { useFriendsList } from '../hooks/useFriendsList'
import { useFriendRequests } from '../hooks/useFriendRequests'

const TABS = [
  { id: 'requests', label: 'Solicitudes' },
  { id: 'suggestions', label: 'Sugerencias' },
  { id: 'friends', label: 'Amigos' },
]

export default function FriendsPage() {
  const [tab, setTab] = useState('requests')
  const requests = useFriendRequests()
  const friends = useFriendsList()

  if (requests.status === 'error') {
    return (
      <div className="friends-page">
        <ErrorState title="No pudimos cargar tus conexiones" description={requests.error} onRetry={requests.refresh} />
      </div>
    )
  }

  return (
    <div className="friends-page">
      <header className="friends-page__header">
        <h1 className="friends-page__title">Amigos</h1>
        {requests.actionError && <p className="friends-page__action-error" role="alert">{requests.actionError}</p>}
      </header>

      <div className="friends-page__tabs" role="tablist" aria-label="Conexiones">
        {TABS.map((item) => (
          <button
            key={item.id}
            type="button"
            role="tab"
            aria-selected={tab === item.id}
            className={`friends-page__tab${tab === item.id ? ' is-active' : ''}`}
            onClick={() => setTab(item.id)}
          >
            {item.label}
            {item.id === 'requests' && requests.received.length > 0 && (
              <span className="friends-page__badge">{requests.received.length}</span>
            )}
          </button>
        ))}
      </div>

      {tab === 'requests' && (
        <FriendRequestsPanel
          received={requests.received}
          sent={requests.sent}
          busyId={requests.busyId}
          onAccept={requests.accept}
          onReject={requests.reject}
          onCancel={requests.cancel}
        />
      )}

      {tab === 'suggestions' && <FriendSuggestions />}

      {tab === 'friends' && (
        <FriendsList
          friends={friends.friends}
          status={friends.status}
          error={friends.error}
          busyId={friends.busyId}
          onRemove={friends.removeFriend}
          onRetry={friends.refresh}
        />
      )}
    </div>
  )
}