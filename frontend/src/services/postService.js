import { apiRequest } from './api'

export const postService = {
  createPost(payload) {
    return apiRequest('/api/posts', { method: 'POST', body: payload })
  },

  getPost(id) {
    return apiRequest(`/api/posts/${id}`)
  },

  updatePost(id, payload) {
    return apiRequest(`/api/posts/${id}`, { method: 'PUT', body: payload })
  },

  deletePost(id) {
    return apiRequest(`/api/posts/${id}`, { method: 'DELETE' })
  },

  listUserPosts(userId, { cursor = null, limit = 10 } = {}) {
    const params = new URLSearchParams({ limit: String(limit) })
    if (cursor) params.set('cursor', cursor)
    return apiRequest(`/api/users/${userId}/posts?${params.toString()}`)
  },

  uploadMedia(files) {
    const body = new FormData()
    files.forEach((file) => body.append('files', file))
    return apiRequest('/api/posts/media', { method: 'POST', body })
  },

  deleteMedia(mediaId) {
    return apiRequest(`/api/posts/media/${mediaId}`, { method: 'DELETE' })
  },

  like(postId) {
    return apiRequest(`/api/posts/${postId}/like`, { method: 'POST' })
  },

  unlike(postId) {
    return apiRequest(`/api/posts/${postId}/like`, { method: 'DELETE' })
  },

  getLikeStatus(postId) {
    return apiRequest(`/api/posts/${postId}/like`)
  },

  getLikes(postId, { cursor = null, limit = 20 } = {}) {
    const params = new URLSearchParams({ limit: String(limit) })
    if (cursor) params.set('cursor', cursor)
    return apiRequest(`/api/posts/${postId}/likes?${params.toString()}`)
  },

  /**
   * Guarda o quita la publicación. Un solo endpoint hace las dos cosas: el
   * servidor ya sabe cuál de las dos corresponde y devuelve el estado final.
   */
  toggleSave(postId) {
    return apiRequest(`/api/posts/saved/${postId}`, { method: 'POST' })
  },

  listSaved({ cursor = null, limit = 10 } = {}) {
    const params = new URLSearchParams({ limit: String(limit) })
    if (cursor) params.set('cursor', cursor)
    return apiRequest(`/api/posts/saved?${params.toString()}`)
  },
}