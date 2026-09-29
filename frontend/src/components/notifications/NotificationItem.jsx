import { Link } from 'react-router-dom'
import Avatar from '../ui/Avatar'
import Icon from '../ui/Icon/Icon'
import { NOTIFICATION_ICONS, notificationText } from '../../constants/notifications'
import { notificationTarget } from '../../constants/routes'
import { formatRelativeTime } from '../../utils/dateFormatter'

export default function NotificationItem({ notification, onRead }) {
  const iconName = NOTIFICATION_ICONS[notification.type] ?? 'bell'

  function handleClick() {
    if (!notification.isRead) onRead?.(notification.id)
  }

  return (
    <li className={`notification${notification.isRead ? '' : ' is-unread'}`}>
      <Link to={notificationTarget(notification)} className="notification__link" onClick={handleClick}>
        <Avatar
          name={notification.actor?.displayName}
          src={notification.actor?.profileImageUrl}
          size="sm"
        />

        <span className="notification__body">
          <span className="notification__text">{notificationText(notification)}</span>
          <span className="notification__time">{formatRelativeTime(notification.createdAt)}</span>
        </span>

        <span className={`notification__kind notification__kind--${iconName}`} aria-hidden="true">
          <Icon name={iconName} size="sm" />
        </span>
      </Link>
    </li>
  )
}