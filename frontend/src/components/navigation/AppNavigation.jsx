import { useNavigate } from 'react-router-dom'
import { useMediaQuery } from '../../hooks/useMediaQuery'
import { useNavCounts } from '../../hooks/useNavCounts'
import { BREAKPOINTS_MEDIA } from '../../constants/breakpoints'
import { ROUTES } from '../../constants/routes'
import { useAuth } from '../../hooks/useAuth'
import Logo from '../ui/Logo'
import Avatar from '../ui/Avatar'
import IconButton from '../ui/IconButton'
import NavItem from './NavItem'

export default function AppNavigation() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const isDesktop = useMediaQuery(BREAKPOINTS_MEDIA.md)
  const displayName = user?.displayName ?? user?.userName
  const { counts } = useNavCounts()

  const items = [
    { to: ROUTES.home, icon: 'home', label: 'Inicio', end: true },
    { to: ROUTES.friends, icon: 'users', label: 'Amigos', badge: counts.pendingFriendRequests },
    { to: ROUTES.messages, icon: 'chat', label: 'Mensajes', badge: counts.unreadMessages },
    {
      to: ROUTES.notifications,
      icon: 'bell',
      label: 'Notificaciones',
      badge: counts.unreadNotifications,
    },
    { to: ROUTES.profile, icon: 'user', label: 'Perfil' },
    { to: ROUTES.help, icon: 'help', label: 'Ayuda' },
  ]

  async function handleLogout() {
    await logout()
    navigate(ROUTES.login)
  }

  if (isDesktop) {
    return (
      <header className="topbar">
        <Logo className="topbar__logo" />
        <nav className="topbar__nav" aria-label="Principal">
          {items.map((item) => (
            <NavItem key={item.label} {...item} />
          ))}
        </nav>
        {user && (
          <div className="topbar__user">
            <Avatar name={displayName} size="sm" />
            <span className="topbar__username">{displayName}</span>
            <IconButton name="logout" label="Cerrar sesión" onClick={handleLogout} size="sm" />
          </div>
        )}
      </header>
    )
  }

  return (
    <nav className="bottom-nav" aria-label="Principal">
      {items.map((item) => (
        <NavItem key={item.label} {...item} />
      ))}
      <IconButton name="logout" label="Cerrar sesión" onClick={handleLogout} size="md" />
    </nav>
  )
}
