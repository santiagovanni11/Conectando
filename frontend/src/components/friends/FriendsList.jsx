import { Link } from 'react-router-dom'
import Avatar from '../ui/Avatar'
import Icon from '../ui/Icon/Icon'
import IconButton from '../ui/IconButton'
import EmptyState from '../ui/EmptyState'
import ErrorState from '../ui/ErrorState'
import UserHandle from '../users/UserHandle'
import { ROUTES } from '../../constants/routes'

export default function FriendsList({ friends, status, error, busyId, onRemove, onRetry }) {
  if (status === 'error') {
    return <ErrorState description={error} onRetry={onRetry} />
  }

  if (friends.length === 0) {
    return (
      <EmptyState
        icon={<Icon name="users" size="lg" />}
        title="Todavía no tenés amigos"
        description="Buscá personas y enviales una solicitud para empezar."
      />
    )
  }

  return (
    <ul className="friends-panel__list">
      {friends.map((item) => (
        <li key={item.user.id} className="friends-row">
          <Link to={ROUTES.user(item.user.id)} className="friends-row__identity">
            <Avatar name={item.user.displayName} src={item.user.profileImageUrl} size="sm" />
            <span className="friends-row__text">
              <span className="friends-row__name">{item.user.displayName}</span>
              {/* La fila ya es un enlace al perfil: este solo pone el arroba y
                  mantiene la etiqueta de cuenta dada de baja. */}
              <UserHandle user={item.user} className="friends-row__meta" linked={false} />
            </span>
          </Link>
          <IconButton
            name="close"
            label={`Dejar de ser amigos con ${item.user.displayName}`}
            disabled={busyId === item.user.id}
            onClick={() => onRemove(item.user.id)}
          />
        </li>
      ))}
    </ul>
  )
}