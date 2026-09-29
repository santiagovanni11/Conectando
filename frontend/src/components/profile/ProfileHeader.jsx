import { Link } from 'react-router-dom'
import { useState } from 'react'
import Icon from '../ui/Icon/Icon'
import ProfileAvatar from './ProfileAvatar'
import ProfileStats from './ProfileStats'
import { useAvatarEditor } from '../../hooks/useAvatarEditor'
import { ROUTES } from '../../constants/routes'

export default function ProfileHeader({ profile, counts, isOwn = false, onOpenConnections, onProfileChange }) {
  // El perfil se copia porque userService devuelve el actualizado y hay que
  // refrescar la cabecera sin recargar la página.
  const [perfil, setPerfil] = useState(profile)

  const { uploading, savingFraming, error, replacePhoto, saveFraming } = useAvatarEditor(
    perfil,
    (actualizado) => {
      setPerfil(actualizado)
      onProfileChange?.(actualizado)
    },
  )

  return (
    <header className="profile-card profile-header">
      <ProfileAvatar
        name={perfil.displayName}
        src={perfil.profileImageUrl}
        framing={perfil}
        isOwn={isOwn}
        uploading={uploading}
        savingFraming={savingFraming}
        onSelectPhoto={replacePhoto}
        onSaveFraming={saveFraming}
      />
      <div className="profile-header__info">
        <h1 className="profile-header__name">{perfil.displayName}</h1>
        <p className="profile-header__username">@{perfil.userName}</p>
        {perfil.bio && <p className="profile-header__bio">{perfil.bio}</p>}
        <ProfileStats
          profile={perfil}
          counts={counts}
          onOpenConnections={onOpenConnections}
        />
      </div>
      {isOwn && (
        <Link to={ROUTES.profileEdit} className="profile-header__edit">
          <span className="profile-header__edit-text">
            <Icon name="edit" size="sm" />
            <span>Editar perfil</span>
          </span>
        </Link>
      )}
      {error && (
        <p className="profile-header__avatar-error" role="alert">
          {error}
        </p>
      )}
    </header>
  )
}