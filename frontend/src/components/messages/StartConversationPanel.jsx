import { useMemo, useState } from 'react'
import Avatar from '../ui/Avatar'
import Icon from '../ui/Icon/Icon'
import Loader from '../ui/Loader'
import EmptyState from '../ui/EmptyState'
import MessageButton from './MessageButton'
import UserHandle from '../users/UserHandle'

/**
 * Permite empezar un chat con un amigo. Busca por nombre o usuario.
 */
export default function StartConversationPanel({ friends, status }) {
  const [query, setQuery] = useState('')

  const matches = useMemo(() => {
    const term = query.trim().toLowerCase()
    if (!term) return friends

    return friends.filter((friend) =>
      friend.user.displayName.toLowerCase().includes(term) ||
      friend.user.userName.toLowerCase().includes(term),
    )
  }, [friends, query])

  if (status === 'loading') {
    return <Loader label="Cargando amigos…" />
  }

  if (friends.length === 0) {
    return (
      <EmptyState
        icon={<Icon name="users" size="lg" />}
        title="Todavía no tenés amigos"
        description="Buscá personas y enviales una solicitud para poder chatear."
      />
    )
  }

  return (
    <div className="start-conversation">
      <label className="start-conversation__search">
        <span className="start-conversation__label">Buscar amigo</span>
        <input
          type="search"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          placeholder="Escribí un nombre"
        />
      </label>

      <ul className="start-conversation__list">
        {matches.map((friend) => (
          <li key={friend.user.id} className="start-conversation__row">
            <div className="start-conversation__identity">
              <Avatar
                name={friend.user.displayName}
                src={friend.user.profileImageUrl}
                size="sm"
              />
              <span className="start-conversation__text">
                <UserHandle user={friend.user} className="start-conversation__name">
                  {friend.user.displayName}
                </UserHandle>
                <UserHandle user={friend.user} className="start-conversation__meta" />
              </span>
            </div>

            <MessageButton userId={friend.user.id} size="sm" />
          </li>
        ))}
      </ul>

      {matches.length === 0 && (
        <p className="start-conversation__empty">Ningún amigo coincide con esa búsqueda.</p>
      )}
    </div>
  )
}