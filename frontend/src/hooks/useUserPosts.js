import { useEffect, useState } from 'react'
import { postService } from '../services/postService'

export function useUserPosts(userId) {
  const [items, setItems] = useState([])
  const [nextCursor, setNextCursor] = useState(null)
  const [hasMore, setHasMore] = useState(false)
  const [phase, setPhase] = useState('loading')
  const [error, setError] = useState('')
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    let active = true

    postService
      .listUserPosts(userId, { limit: 10 })
      .then((page) => {
        if (!active) return
        setItems(page.items)
        setNextCursor(page.nextCursor)
        setHasMore(page.hasMore)
        setPhase('loaded')
        setError('')
      })
      .catch((err) => {
        if (!active) return
        setItems([])
        setNextCursor(null)
        setHasMore(false)
        setError(err.message)
        setPhase('error')
      })

    return () => {
      active = false
    }
  }, [attempt, userId])

  async function loadMore() {
    if (!hasMore || !nextCursor || phase === 'loading-more') return
    setPhase('loading-more')
    try {
      const page = await postService.listUserPosts(userId, { limit: 10, cursor: nextCursor })
      setItems((prev) => [...prev, ...page.items])
      setNextCursor(page.nextCursor)
      setHasMore(page.hasMore)
      setPhase('loaded')
    } catch (err) {
      setError(err.message)
      setPhase('loaded')
    }
  }

  function prepend(post) {
    setItems((prev) => [post, ...prev])
  }

  function updatePost(post) {
    setItems((prev) => prev.map((item) => (item.id === post.id ? post : item)))
  }

  function removePost(id) {
    setItems((prev) => prev.filter((item) => item.id !== id))
  }

  function refresh() {
    setAttempt((current) => current + 1)
  }

  return { items, hasMore, phase, error, loadMore, prepend, updatePost, removePost, refresh }
}