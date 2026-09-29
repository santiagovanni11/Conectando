import UserCard from '../social/UserCard'
import Modal from '../ui/Modal'
import EmptyState from '../ui/EmptyState'
import ErrorState from '../ui/ErrorState'
import Icon from '../ui/Icon/Icon'
import { useUserConnections } from '../../hooks/useUserConnections'

const TITLES = {
  friends: 'Amigos',
  followers: 'Seguidores',
}

const EMPTY_COPY = {
  friends: { title: 'Todavía no tiene amigos', description: 'Cuando acepte una solicitud, aparecerá acá.' },
  followers: { title: 'Todavía no tiene seguidores', description: 'Cuando alguien lo siga, aparecerá acá.' },
}

/** Lista de amigos o seguidores de un usuario. */
export default function UserConnectionsModal({ userId, kind, onClose }) {
  const { items, loading, error, refresh, sendRequest } = useUserConnections(userId, kind)

  const title = TITLES[kind] ?? 'Conexiones'
  const empty = EMPTY_COPY[kind] ?? EMPTY_COPY.friends

  return (
    <Modal title={title} onClose={onClose} className="connections-modal">
      <div className="connections-modal__bar">
        <span className="connections-modal__count">
          {loading ? 'Cargando…' : `${items.length} ${items.length === 1 ? 'persona' : 'personas'}`}
        </span>
        <button
          type="button"
          className="connections-modal__refresh"
          onClick={refresh}
          disabled={loading}
        >
          Actualizar
        </button>
      </div>

      {error && <ErrorState description={error} onRetry={refresh} />}

      {!loading && !error && items.length === 0 && (
        <EmptyState
          icon={<Icon name="users" size="lg" />}
          title={empty.title}
          description={empty.description}
        />
      )}

      {items.length > 0 && (
        <ul className="user-card-list">
          {items.map((user) => (
            <UserCard key={user.id} user={user} onSendRequest={sendRequest} onOpen={onClose} />
          ))}
        </ul>
      )}
    </Modal>
  )
}