export const NOTIFICATION_TYPES = {
  Like: 'Like',
  Comment: 'Comment',
  FriendRequest: 'FriendRequest',
  FriendAccepted: 'FriendAccepted',
  Follow: 'Follow',
}

export const NOTIFICATION_ICONS = {
  [NOTIFICATION_TYPES.Like]: 'heart',
  [NOTIFICATION_TYPES.Comment]: 'comment',
  [NOTIFICATION_TYPES.FriendRequest]: 'users',
  [NOTIFICATION_TYPES.FriendAccepted]: 'check',
  [NOTIFICATION_TYPES.Follow]: 'users',
}

export const NOTIFICATION_LABELS = {
  [NOTIFICATION_TYPES.Like]: 'le dio me gusta a tu publicación',
  [NOTIFICATION_TYPES.Comment]: 'comentó tu publicación',
  [NOTIFICATION_TYPES.FriendRequest]: 'te envió una solicitud de amistad',
  [NOTIFICATION_TYPES.FriendAccepted]: 'aceptó tu solicitud de amistad',
  [NOTIFICATION_TYPES.Follow]: 'comenzó a seguirte',
}

export function notificationText(notification) {
  const name = notification.actor?.displayName ?? notification.actor?.userName ?? 'Alguien'
  const label = NOTIFICATION_LABELS[notification.type] ?? 'interactuó con tu contenido'
  return `${name} ${label}`
}