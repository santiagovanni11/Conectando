export const ROUTES = {
  home: '/',
  login: '/login',
  register: '/register',
  profile: '/profile',
  profileEdit: '/profile/edit',
  post: (postId) => `/posts/${postId}`,
  user: (userId) => `/users/${userId}`,
  friends: '/friends',
  help: '/help',
  notifications: '/notifications',
  messages: '/messages',
  conversation: (conversationId) => `/messages/${conversationId}`,
}

/** Destino al navegar desde una notificación. */
export function notificationTarget(notification) {
  if (notification.postId) return ROUTES.post(notification.postId)
  if (notification.actor?.id) return ROUTES.user(notification.actor.id)
  return ROUTES.home
}

export const ROUTE_PATTERNS = {
  post: '/posts/:postId',
  user: '/users/:userId',
  conversation: '/messages/:conversationId',
}