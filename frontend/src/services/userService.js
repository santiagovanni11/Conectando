import { apiRequest } from './api'

export const userService = {
  getOwnProfile() {
    return apiRequest('/api/users/me/profile')
  },

  getPublicProfile(userId) {
    return apiRequest(`/api/users/${userId}`)
  },

  /**
   * Conteos del perfil de otro usuario. Van aparte porque dependen de quién
   * mira: si bloqueaste a alguien de su lista, el número baja.
   */
  getPublicCounts(userId) {
    return apiRequest(`/api/users/${userId}/counts`)
  },

  /**
   * Busca por nombre o usuario. Devuelve tarjetas con el estado de
   * relación ya resuelto (friendship: none | sent | received | friends).
   */
  searchUsers(query, { limit = 20 } = {}) {
    const params = new URLSearchParams({ query, limit: String(limit) })
    return apiRequest(`/api/users/search?${params.toString()}`)
  },

  /** Personas sugeridas para amistad, ordenadas por amigos en común. */
  getSuggestions({ limit = 10 } = {}) {
    return apiRequest(`/api/users/suggestions?limit=${limit}`)
  },

  getUserFriends(userId) {
    return apiRequest(`/api/users/${userId}/friends`)
  },

  getUserFollowers(userId) {
    return apiRequest(`/api/users/${userId}/followers`)
  },

  updateProfile(body) {
    return apiRequest('/api/users/me/profile', { method: 'PUT', body })
  },
}