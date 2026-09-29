import { apiRequest } from './api'

export const feedService = {
  getFeed({ cursor, limit = 10 } = {}) {
    const params = new URLSearchParams()
    if (cursor) params.set('cursor', cursor)
    if (limit) params.set('limit', String(limit))

    const query = params.toString()
    return apiRequest(`/api/feed${query ? `?${query}` : ''}`)
  },
}
