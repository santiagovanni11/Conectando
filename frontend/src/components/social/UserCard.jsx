import { Link } from 'react-router-dom'
import Avatar from '../ui/Avatar'
import Button from '../ui/Button'
import { ROUTES } from '../../constants/routes'

const FRIENDSHIP_LABELS = {
  none: 'Agregar',
  sent: 'Solicitud enviada',
  received: 'Responder',
  friends: 'Son amigos',
}

/**
 * Tarjeta de persona con su acción de relación.
 * La usan la búsqueda, las sugerencias y la lista de amigos.
 */
export default function UserCard({ user, busy = false, onSendRequest, onOpen, showMutual = false }) {
  const label = FRIENDSHIP_LABELS[user.friendship] ?? FRIENDSHIP_LABELS.none
  const canSend = user.friendship === 'none'

  return (
    <li className="user-card">
      <Link to={ROUTES.user(user.id)} className="user-card__identity" onClick={onOpen}>
        <Avatar name={user.displayName} src={user.profileImageUrl} size="md" />
        <span className="user-card__text">
          <span className="user-card__name">{user.displayName}</span>
          <span className="user-card__meta">@{user.userName}</span>
          {showMutual && user.mutualFriendsCount > 0 && (
            <span className="user-card__mutual">
              {user.mutualFriendsCount} {user.mutualFriendsCount === 1 ? 'amigo en común' : 'amigos en común'}
            </span>
          )}
        </span>
      </Link>

      {canSend && onSendRequest ? (
        <Button
          size="sm"
          loading={busy}
          onClick={() => onSendRequest(user.id)}
          aria-label={`Agregar a ${user.displayName}`}
        >
          {label}
        </Button>
      ) : (
        <span className={`user-card__tag user-card__tag--${user.friendship}`}>{label}</span>
      )}
    </li>
  )
}