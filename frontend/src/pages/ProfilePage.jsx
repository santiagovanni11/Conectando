import { useCallback, useState } from 'react'
import { Navigate, useParams } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { useProfile } from '../hooks/useProfile'
import { useProfileCounts } from '../hooks/useProfileCounts'
import { socialService } from '../services/socialService'
import { ROUTES } from '../constants/routes'
import Loader from '../components/ui/Loader'
import ErrorState from '../components/ui/ErrorState'
import EmptyState from '../components/ui/EmptyState'
import Icon from '../components/ui/Icon/Icon'
import Divider from '../components/ui/Divider'
import ProfileHeader from '../components/profile/ProfileHeader'
import ProfileTabs from '../components/profile/ProfileTabs'
import ProfileMeta from '../components/profile/ProfileMeta'
import SocialActions from '../components/social/SocialActions'
import UserPostsSection from '../components/posts/UserPostsSection'
import SavedPostsSection from '../components/posts/SavedPostsSection'
import BlockedProfilesSection from '../components/profile/BlockedProfilesSection'
import AccountSettings from '../components/profile/AccountSettings'
import UserConnectionsModal from '../components/profile/UserConnectionsModal'

export default function ProfilePage() {
  const { user } = useAuth()
  const { userId } = useParams()
  const { status, profile, error, notFound, reload } = useProfile(userId)
  const [connections, setConnections] = useState(null)
  const [areFriends, setAreFriends] = useState(false)
  const [tab, setTab] = useState('posts')

  // /users/{miId} también es mi perfil: se compara con la sesión, no con la URL.
  const isOwn = !userId || userId === user?.id

  // En el perfil propio la URL no trae id: se usa el del perfil ya cargado.
  // Así los conteos se piden siempre y no quedan en cero.
  const targetId = userId ?? profile?.id

  // Los conteos se piden en cualquier perfil: en el propio no hay bloqueos
  // que restar, pero los números tienen que venir igual. Al bloquear hay que
  // recargarlos, porque el número tiene que bajar junto con la lista.
  const { counts, refresh: refreshCounts } = useProfileCounts(targetId, Boolean(targetId))

  // Tras agregar/quitar amigo o seguir, los conteos del perfil quedan viejos.
  const handleChanged = useCallback(() => {
    reload()
    refreshCounts()
    if (isOwn || !userId) return
    socialService.getStatus(userId)
      .then((status) => setAreFriends(status.friendship === 'friends'))
      .catch(() => setAreFriends(false))
  }, [isOwn, reload, refreshCounts, userId])

  // Click en tu propio nombre: se redirige a /profile, como en Facebook.
  if (userId && userId === user?.id) {
    return <Navigate to={ROUTES.profile} replace />
  }

  if (notFound) {
    return (
      <div className="profile-page">
        <EmptyState
          icon={<Icon name="user" size="lg" />}
          title="Perfil no encontrado"
          description="Este perfil no existe o ya no está disponible."
        />
      </div>
    )
  }

  if (status === 'error') {
    return (
      <div className="profile-page">
        <ErrorState description={error} onRetry={reload} />
      </div>
    )
  }

  if (status === 'loading' || !profile) {
    return (
      <div className="profile-page">
        <Loader label="Cargando perfil…" />
      </div>
    )
  }

  return (
    <div className="profile-page">
      <ProfileHeader
        profile={profile}
        counts={counts}
        isOwn={isOwn}
        onOpenConnections={(kind) => setConnections(kind)}
      />
      {!isOwn && <SocialActions user={profile} onChanged={handleChanged} />}
      <ProfileMeta createdAt={profile.createdAt} />
      <Divider />

      {/* Cada pestaña muestra una lista distinta, pero todas son del mismo
          tipo: publicaciones o perfiles. */}
      {isOwn && <ProfileTabs active={tab} onChange={setTab} />}

      {isOwn && tab === 'posts' && <UserPostsSection userId={profile.id} isOwn={isOwn} />}
      {isOwn && tab === 'saved' && <SavedPostsSection />}
      {isOwn && tab === 'blocked' && <BlockedProfilesSection />}
      {isOwn && tab === 'account' && <AccountSettings />}

      {!isOwn && profile.isPrivate && !areFriends && (
        <div className="profile-locked">
          <Icon name="users" size="lg" />
          <h2 className="profile-locked__title">Esta cuenta es privada</h2>
          <p className="profile-locked__text">
            Agregame como amigo para ver sus publicaciones, sus amigos y sus seguidores.
          </p>
        </div>
      )}

      {!isOwn && !profile.isPrivate && <UserPostsSection userId={profile.id} isOwn={isOwn} />}
      <Divider />
      {connections && (
        <UserConnectionsModal
          key={`${profile.id}-${connections}`}
          userId={profile.id}
          kind={connections}
          onClose={() => setConnections(null)}
        />
      )}
    </div>
  )
}