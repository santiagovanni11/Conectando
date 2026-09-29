import { Link } from 'react-router-dom'
import Avatar from '../ui/Avatar'
import Icon from '../ui/Icon/Icon'
import ProfileStats from './ProfileStats'
import { ROUTES } from '../../constants/routes'

export default function ProfileHeader({ profile, counts, isOwn = false, onOpenConnections }) {
  return (
    <header className="profile-card profile-header">
      <Avatar
        name={profile.displayName}
        src={profile.profileImageUrl}
        alt={profile.displayName}
        size="xl"
      />
      <div className="profile-header__info">
        <h1 className="profile-header__name">{profile.displayName}</h1>
        <p className="profile-header__username">@{profile.userName}</p>
        {profile.bio && <p className="profile-header__bio">{profile.bio}</p>}
        <ProfileStats
          profile={profile}
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
    </header>
  )
}