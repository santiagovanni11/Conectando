import { useState, useCallback } from 'react'
import { commentService } from '../services/commentService'

const REPLIES_LIMIT = 50

export function usePostComments(postId) {
  const [comments, setComments] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  const [hasMore, setHasMore] = useState(false)
  const [nextCursor, setNextCursor] = useState(null)
  const [replyState, setReplyState] = useState({})

  const loadInitial = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const result = await commentService.getComments(postId, { limit: 20 })
      setComments(result.items)
      setHasMore(result.hasMore)
      setNextCursor(result.nextCursor)
    } catch {
      setError('No se pudieron cargar los comentarios')
    } finally {
      setLoading(false)
    }
  }, [postId])

  const loadMore = useCallback(async () => {
    if (loading || !hasMore || !nextCursor) return
    setLoading(true)
    try {
      const result = await commentService.getComments(postId, { cursor: nextCursor, limit: 20 })
      setComments((prev) => [...prev, ...result.items])
      setHasMore(result.hasMore)
      setNextCursor(result.nextCursor)
    } catch {
      setError('No se pudieron cargar más comentarios')
    } finally {
      setLoading(false)
    }
  }, [loading, hasMore, nextCursor, postId])

  const toggleReplies = useCallback(async (comment) => {
    const current = replyState[comment.id]
    if (current?.loading) return
    if (current && (current.open || current.items.length > 0)) {
      setReplyState((prev) => ({ ...prev, [comment.id]: { ...current, open: !current.open } }))
      return
    }
    setReplyState((prev) => ({ ...prev, [comment.id]: { open: true, loading: true, items: [], error: false } }))
    try {
      const items = await commentService.getReplies(comment.id, { limit: REPLIES_LIMIT })
      setReplyState((prev) => ({ ...prev, [comment.id]: { open: true, loading: false, items, error: false } }))
    } catch {
      setReplyState((prev) => ({ ...prev, [comment.id]: { open: true, loading: false, items: [], error: true } }))
    }
  }, [replyState])

  const addComment = useCallback((comment) => {
    setComments((prev) => [...prev, comment])
  }, [])

  const addReply = useCallback((parentId, reply) => {
    setComments((prev) => prev.map((c) => (
      c.id === parentId ? { ...c, repliesCount: (c.repliesCount ?? 0) + 1 } : c
    )))
    setReplyState((prev) => {
      const current = prev[parentId]
      if (!current?.open) return prev
      return { ...prev, [parentId]: { ...current, items: [...current.items, reply] } }
    })
  }, [])

  const updateComment = useCallback((updated) => {
    setComments((prev) => prev.map((c) => (c.id === updated.id ? updated : c)))
    setReplyState((prev) => Object.fromEntries(Object.entries(prev).map(([id, state]) => [
      id,
      { ...state, items: state.items.map((r) => (r.id === updated.id ? updated : r)) },
    ])))
  }, [])

  const removeComment = useCallback((commentId) => {
    setComments((prev) => prev.filter((c) => c.id !== commentId))
    setReplyState((prev) => Object.fromEntries(Object.entries(prev).map(([id, state]) => [
      id,
      { ...state, items: state.items.filter((r) => r.id !== commentId) },
    ])))
  }, [])

  return {
    comments,
    loading,
    error,
    hasMore,
    replyState,
    loadInitial,
    loadMore,
    toggleReplies,
    addComment,
    addReply,
    updateComment,
    removeComment,
  }
}