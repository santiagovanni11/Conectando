import { apiRequest } from './api'

export const commentService = {
  getComments(postId, params = {}) {
    const query = new URLSearchParams()
    if (params.cursor) query.set('cursor', params.cursor)
    query.set('limit', params.limit ?? 20)
    return apiRequest(`/api/posts/${postId}/comments?${query}`)
  },

  getReplies(commentId, params = {}) {
    const query = new URLSearchParams()
    query.set('limit', params.limit ?? 10)
    return apiRequest(`/api/comments/${commentId}/replies?${query}`)
  },

  create(postId, data) {
    return apiRequest(`/api/posts/${postId}/comments`, {
      method: 'POST',
      body: data,
    })
  },

  update(commentId, data) {
    return apiRequest(`/api/comments/${commentId}`, {
      method: 'PUT',
      body: data,
    })
  },

  delete(commentId) {
    return apiRequest(`/api/comments/${commentId}`, {
      method: 'DELETE',
    })
  },
}