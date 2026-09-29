import Button from '../ui/Button'
import EmptyState from '../ui/EmptyState'
import ErrorState from '../ui/ErrorState'
import Icon from '../ui/Icon/Icon'
import Loader from '../ui/Loader'
import Avatar from '../ui/Avatar'
import { useBlockedProfiles } from '../../hooks/useBlockedProfiles'
import { formatConversationTime } from '../../constants/messages'

/**
 * Apartado "Perfiles bloqueados" del perfil propio.
 *
 * Existe para poder volver atrás: bloquear es fácil de hacer por un
 * momento y muy difícil de revertir si no se tiene el listado.
 */
export default function BlockedProfilesSection() {
  const { items, phase, error, unblock, refresh } = useBlockedProfiles()

  if (phase === 'loading') return <Loader label="Cargando bloqueados…" />

  if (phase === 'error') return <ErrorState description={error} onRetry={refresh} />

  if (items.length === 0) {
    return (
      <EmptyState
        icon={<Icon name="users" size="lg" />}
        title="No bloqueaste a nadie"
        description="Cuando bloquees a alguien, va a aparecer acá por si querés desbloquearlo."
      />
    )
  }

  return (
    <section className="blocked-profiles" aria-label="Perfiles bloqueados">
      <ul className="blocked-profiles__list">
        {items.map(({ user, blockedAt }) => (
          <li key={user.id} className="blocked-profile">
            <Avatar name={user.displayName} src={user.profileImageUrl} size="sm" />
            <span className="blocked-profile__body">
              <span className="blocked-profile__name">{user.displayName}</span>
              <span className="blocked-profile__date">
                Bloqueado el {formatConversationTime(blockedAt)}
              </span>
            </span>
            <Button variant="secondary" size="sm" onClick={() => unblock(user.id)}>
              Desbloquear
            </Button>
          </li>
        ))}
      </ul>
    </section>
  )
}