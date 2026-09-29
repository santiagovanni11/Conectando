import { Link } from 'react-router-dom'
import Avatar from '../ui/Avatar'
import Button from '../ui/Button'
import Icon from '../ui/Icon/Icon'
import IconButton from '../ui/IconButton'
import EmptyState from '../ui/EmptyState'
import { ROUTES } from '../../constants/routes'
import { formatRelativeTime } from '../../utils/dateFormatter'

function UserIdentity({ user, meta }) {
  return (
    <Link to={ROUTES.user(user.id)} className="friends-row__identity">
      <Avatar name={user.displayName} src={user.profileImageUrl} size="sm" />
      <span className="friends-row__text">
        <span className="friends-row__name">{user.displayName}</span>
        <span className="friends-row__meta">{meta ?? `@${user.userName}`}</span>
      </span>
    </Link>
  )
}

function ReceivedItem({ item, busyId, onAccept, onReject }) {
  const busy = busyId === item.user.id
  return (
    <li className="friends-row">
      <UserIdentity user={item.user} />
      <div className="friends-row__actions">
        <Button size="sm" loading={busy} onClick={() => onAccept(item.user.id)}>Aceptar</Button>
        <Button variant="secondary" size="sm" disabled={busy} onClick={() => onReject(item.user.id)}>
          Rechazar
        </Button>
      </div>
    </li>
  )
}

function SentItem({ item, busyId, onCancel }) {
  return (
    <li className="friends-row">
      <UserIdentity user={item.user} meta={formatRelativeTime(item.createdAt)} />
      <IconButton
        name="close"
        label="Cancelar solicitud"
        disabled={busyId === item.user.id}
        onClick={() => onCancel(item.user.id)}
      />
    </li>
  )
}

export default function FriendRequestsPanel({ received, sent, busyId, onAccept, onReject, onCancel }) {
  if (received.length === 0 && sent.length === 0) {
    return (
      <EmptyState
        icon={<Icon name="users" size="lg" />}
        title="No tenés solicitudes pendientes"
        description="Cuando alguien te agregue, la solicitud aparece acá."
      />
    )
  }

  return (
    <div className="friends-panel">
      {received.length > 0 && (
        <section aria-label="Solicitudes recibidas">
          <h2 className="friends-panel__title">Recibidas</h2>
          <ul className="friends-panel__list">
            {received.map((item) => (
              <ReceivedItem key={item.user.id} item={item} busyId={busyId} onAccept={onAccept} onReject={onReject} />
            ))}
          </ul>
        </section>
      )}

      {sent.length > 0 && (
        <section aria-label="Solicitudes enviadas">
          <h2 className="friends-panel__title">Enviadas</h2>
          <ul className="friends-panel__list">
            {sent.map((item) => (
              <SentItem key={item.user.id} item={item} busyId={busyId} onCancel={onCancel} />
            ))}
          </ul>
        </section>
      )}
    </div>
  )
}