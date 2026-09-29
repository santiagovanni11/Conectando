import { useState, useCallback } from 'react'
import { postService } from '../services/postService'

export function usePostLikes(postId) {
  const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  const [hasMore, setHasMore] = useState(false)
  const [nextCursor, setNextCursor] = useState(null)

  const loadInitial = useCallback(async () => {
    setLoading(true)
    setError(null)
    setUsers([])
    setHasMore(false)
    setNextCursor(null)

    try {
      const result = await postService.getLikes(postId, { limit: 20 })
      setUsers(result.items)
      setHasMore(result.hasMore)
      setNextCursor(result.nextCursor)
    } catch {
      setError('No se pudieron cargar los me gusta')
    } finally {
      setLoading(false)
    }
  }, [postId])

  const loadMore = useCallback(async () => {
    if (loading || !hasMore || !nextCursor) return

    setLoading(true)
    try {
      const result = await postService.getLikes(postId, { cursor: nextCursor, limit: 20 })
      setUsers((prev) => [...prev, ...result.items])
      setHasMore(result.hasMore)
      setNextCursor(result.nextCursor)
    } catch {
      setError('No se pudieron cargar más me gusta')
    } finally {
      setLoading(false)
    }
  }, [loading, hasMore, nextCursor, postId])

  return { users, loading, error, hasMore, loadInitial, loadMore }
}