import UserCard from '../social/UserCard'
import EmptyState from '../ui/EmptyState'
import ErrorState from '../ui/ErrorState'
import Icon from '../ui/Icon/Icon'
import Loader from '../ui/Loader'
import { userService } from '../../services/userService'
import { useUserDiscovery } from '../../hooks/useUserDiscovery'

export default function FriendSuggestions({ limit = 10 }) {
  const { items, loading, error, busyId, sendRequest, refresh } = useUserDiscovery(
    () => userService.getSuggestions({ limit }),
  )

  if (loading) return <Loader label="Buscando personas para conectar…" />

  if (error) {
    return <ErrorState title="No pudimos cargar las sugerencias" description={error} onRetry={refresh} />
  }

  if (items.length === 0) {
    return (
      <EmptyState
        icon={<Icon name="users" size="lg" />}
        title="No hay sugerencias por ahora"
        description="Buscá personas por su nombre para empezar a seguir conectando."
      />
    )
  }

  return (
    <ul className="user-card-list">
      {items.map((user) => (
        <UserCard
          key={user.id}
          user={user}
          busy={busyId === user.id}
          onSendRequest={sendRequest}
          showMutual
        />
      ))}
    </ul>
  )
}