import Badge from '../ui/Badge'
import Button from '../ui/Button'
import ErrorState from '../ui/ErrorState'
import Icon from '../ui/Icon/Icon'
import Loader from '../ui/Loader'
import MessageButton from '../messages/MessageButton'
import SafetyActions from './SafetyActions'
import { useRelationship } from '../../hooks/useRelationship'
import { socialService } from '../../services/socialService'

export default function SocialActions({ user, onChanged }) {
  const userId = user.id
  const { status, data, error, actionError, busy, refresh, run } = useRelationship(userId, onChanged)

  if (status === 'loading') {
    return <Loader label="Cargando estado…" className="social-actions__loader" />
  }

  if (status === 'error') {
    return <ErrorState description={error} onRetry={refresh} className="social-actions__error" />
  }

  if (!data) return null

  const { friendship, following, blockedByMe, blockedByThem } = data

  const followLabel = following ? 'Dejar de seguir' : 'Seguir'
  const cancelRequest = () => run(() => socialService.cancelRequest(userId))
  const acceptRequest = () => run(() => socialService.acceptRequest(userId))
  const rejectRequest = () => run(() => socialService.rejectRequest(userId))
  const sendRequest = () => run(() => socialService.sendRequest(userId))
  const removeFriend = () => run(() => socialService.removeFriend(userId))
  const toggleFollow = () => run(() => (following ? socialService.unfollow(userId) : socialService.follow(userId)))
  const unblockUser = () => run(() => socialService.unblock(userId))

  return (
    <section className="social-actions" aria-label="Acciones sociales">
      {blockedByThem ? (
        <p className="social-actions__note" role="alert">
          No podés interactuar con esta cuenta.
        </p>
      ) : blockedByMe ? (
        <div className="social-actions__row">
          <p className="social-actions__note">Bloqueaste a esta persona.</p>
          <Button variant="destructive" size="sm" loading={busy} onClick={unblockUser}>
            Desbloquear
          </Button>
        </div>
      ) : (
        <div className="social-actions__row">
          {friendship === 'none' && (
            <Button variant="primary" size="sm" loading={busy} icon={<Icon name="user" size="sm" />} onClick={sendRequest}>
              Agregar amigo
            </Button>
          )}

          {friendship === 'sent' && (
            <>
              <Badge variant="neutral">Solicitud enviada</Badge>
              <Button variant="ghost" size="sm" loading={busy} onClick={cancelRequest}>
                Cancelar
              </Button>
            </>
          )}

          {friendship === 'received' && (
            <>
              <Button variant="primary" size="sm" loading={busy} onClick={acceptRequest}>
                Aceptar
              </Button>
              <Button variant="ghost" size="sm" loading={busy} onClick={rejectRequest}>
                Rechazar
              </Button>
            </>
          )}

          {friendship === 'friends' && (
            <>
              <Badge variant="success" dot>
                Amigos
              </Badge>
              <Button variant="ghost" size="sm" loading={busy} onClick={removeFriend}>
                Eliminar amistad
              </Button>
            </>
          )}

          <Button variant="secondary" size="sm" loading={busy} onClick={toggleFollow}>
            {followLabel}
          </Button>

          {/* El mensaje se puede enviar en cualquier estado de amistad. */}
          <MessageButton userId={userId} size="sm" variant="primary" />
        </div>
      )}

      {actionError && (
        <p className="social-actions__alert" role="alert">
          {actionError}
        </p>
      )}

      {/* Bloquear y denunciar van aparte: el modal de confirmación pesa y
          este componente ya tiene suficiente responsabilidad. */}
      {!blockedByThem && !blockedByMe && (
        <SafetyActions user={user} blockedByMe={blockedByMe} busy={busy} onChanged={run} />
      )}
    </section>
  )
}