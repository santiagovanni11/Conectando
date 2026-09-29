import { useCallback, useEffect, useRef, useState } from 'react'
import { feedService } from '../services/feedService'

const PAGE_SIZE = 10

export function useFeed() {
  const [items, setItems] = useState([])
  const [nextCursor, setNextCursor] = useState(null)
  const [hasMore, setHasMore] = useState(false)
  const [phase, setPhase] = useState('loading')
  const [error, setError] = useState('')
  const [attempt, setAttempt] = useState(0)
  const loadingRef = useRef(false)

  useEffect(() => {
    let active = true
    loadingRef.current = false

    feedService
      .getFeed({ limit: PAGE_SIZE })
      .then((page) => {
        if (!active) return
        setItems(page.items)
        setNextCursor(page.nextCursor)
        setHasMore(page.hasMore)
        setError('')
        setPhase('loaded')
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
  }, [attempt])

  const loadMore = useCallback(async () => {
    if (loadingRef.current || !hasMore || !nextCursor) return
    loadingRef.current = true
    setPhase('loading-more')

    try {
      const page = await feedService.getFeed({ cursor: nextCursor, limit: PAGE_SIZE })
      setItems((prev) => [...prev, ...page.items])
      setNextCursor(page.nextCursor)
      setHasMore(page.hasMore)
      setError('')
      setPhase('loaded')
    } catch (err) {
      setError(err.message)
      setPhase('loaded')
    } finally {
      loadingRef.current = false
    }
  }, [hasMore, nextCursor])

  const refresh = useCallback(() => {
    setPhase('loading')
    setAttempt((current) => current + 1)
  }, [])

  return { items, hasMore, phase, error, loadMore, refresh }
}
