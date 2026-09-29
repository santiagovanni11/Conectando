import { useProfile } from '../hooks/useProfile'
import Loader from '../components/ui/Loader'
import ErrorState from '../components/ui/ErrorState'
import EditProfileForm from '../components/profile/EditProfileForm'

export default function EditProfilePage() {
  const { status, profile, error, reload } = useProfile()

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

  return <EditProfileForm key={profile.id} profile={profile} />
}