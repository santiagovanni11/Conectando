import { useCallback, useEffect, useRef, useState } from 'react'
import { postService } from '../services/postService'

const PAGE_SIZE = 10

/**
 * Lista de publicaciones guardadas.
 *
 * Reutiliza la paginación por cursor del resto de las listas. El `useRef`
 * guarda el callback de recarga: el perfil lo dispara cuando cambia el post
 * guardado, sin tener que re-montar el componente.
 */
export function useSavedPosts() {
  const [items, setItems] = useState([])
  const [hasMore, setHasMore] = useState(false)
  const [phase, setPhase] = useState('loading')
  const [error, setError] = useState('')
  const [attempt, setAttempt] = useState(0)
  const cursorRef = useRef(null)

  useEffect(() => {
    let active = true

    postService
      .listSaved({ limit: PAGE_SIZE })
      .then((page) => {
        if (!active) return
        setItems(page.items)
        setHasMore(page.hasMore)
        setPhase('loaded')
        setError('')
        cursorRef.current = page.nextCursor
      })
      .catch((err) => {
        if (!active) return
        setItems([])
        setHasMore(false)
        setPhase('error')
        setError(err.message)
      })

    return () => {
      active = false
    }
  }, [attempt])

  const loadMore = useCallback(async () => {
    if (!hasMore || !cursorRef.current || phase === 'loading-more') return
    setPhase('loading-more')

    try {
      const page = await postService.listSaved({ limit: PAGE_SIZE, cursor: cursorRef.current })
      setItems((prev) => [...prev, ...page.items])
      setHasMore(page.hasMore)
      setPhase('loaded')
      cursorRef.current = page.nextCursor
    } catch (err) {
      setError(err.message)
      setPhase('loaded')
    }
  }, [hasMore, phase])

  return {
    items,
    hasMore,
    phase,
    error,
    loadMore,
    refresh: () => setAttempt((current) => current + 1),
  }
}