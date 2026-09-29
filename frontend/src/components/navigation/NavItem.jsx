import { NavLink } from 'react-router-dom'
import Icon from '../ui/Icon/Icon'

export default function NavItem({ to, icon, label, end = false, badge = 0, className = '' }) {
  return (
    <NavLink
      to={to}
      end={end}
      className={({ isActive }) => `nav-item ${isActive ? 'nav-item--active' : ''} ${className}`}
      aria-label={badge > 0 ? `${label}, ${badge} sin leer` : label}
    >
      <Icon name={icon} size="md" />
      <span className="nav-item__label">{label}</span>
      {badge > 0 && (
        <span className="nav-item__badge" data-testid={`badge-${label}`}>
          {badge > 99 ? '99+' : badge}
        </span>
      )}
    </NavLink>
  )
}