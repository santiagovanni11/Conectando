import { apiRequest } from './api'

export function fetchConversations() {
  return apiRequest('/api/conversations')
}

export function startDirectConversation(recipientId) {
  return apiRequest('/api/conversations', { method: 'POST', body: { recipientId } })
}

export function fetchMessages(conversationId, { cursor, limit = 30 } = {}) {
  const query = new URLSearchParams({ limit: String(limit) })
  if (cursor) query.set('cursor', cursor)

  return apiRequest(`/api/conversations/${conversationId}/messages?${query.toString()}`)
}

export function sendMessage(conversationId, content) {
  return apiRequest(`/api/conversations/${conversationId}/messages`, {
    method: 'POST',
    body: { content },
  })
}

export function markConversationAsRead(conversationId) {
  return apiRequest(`/api/conversations/${conversationId}/read`, { method: 'POST' })
}

/** Borra el chat solo para el usuario actual; el otro lo conserva. */
export function deleteConversation(conversationId) {
  return apiRequest(`/api/conversations/${conversationId}`, { method: 'DELETE' })
}

/** Silencia o reactiva un chat. No borra mensajes. */
export function setConversationMuted(conversationId, muted) {
  return apiRequest(`/api/conversations/${conversationId}/mute`, {
    method: 'POST',
    body: { muted },
  })
}

/** Edita un mensaje propio. El servidor valida la ventana de tiempo. */
export function editMessage(conversationId, messageId, content) {
  return apiRequest(`/api/conversations/${conversationId}/messages/${messageId}`, {
    method: 'PATCH',
    body: { content },
  })
}

/** Borrado lógico: el texto deja de enviarse y queda el placeholder. */
export function deleteMessage(conversationId, messageId) {
  return apiRequest(`/api/conversations/${conversationId}/messages/${messageId}`, {
    method: 'DELETE',
  })
}

/**
 * Contrato con el backend:
 *   GET /api/conversations/nav-counts
 *   -> { unreadMessages, pendingFriendRequests, unreadNotifications }
 */
export function fetchNavCounts() {
  return apiRequest('/api/conversations/nav-counts')
}