import { apiRequest } from './api'

/**
 * Contrato esperado del backend:
 *   GET  /api/notifications?cursor=&limit=  -> { items, nextCursor, hasMore, unreadCount }
 *   GET  /api/notifications/unread-count   -> { count }
 *   POST /api/notifications/{id}/read      -> 204
 *   POST /api/notifications/read-all       -> 204
 */
export const notificationService = {
  getAll({ cursor = null, limit = 20 } = {}) {
    const params = new URLSearchParams({ limit: String(limit) })
    if (cursor) params.set('cursor', cursor)
    return apiRequest(`/api/notifications?${params.toString()}`)
  },

  getUnreadCount() {
    return apiRequest('/api/notifications/unread-count')
  },

  markAsRead(id) {
    return apiRequest(`/api/notifications/${id}/read`, { method: 'POST' })
  },

  markAllAsRead() {
    return apiRequest('/api/notifications/read-all', { method: 'POST' })
  },
}