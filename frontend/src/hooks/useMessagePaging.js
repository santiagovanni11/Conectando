import { useCallback, useEffect, useRef, useState } from 'react'
import { fetchMessages } from '../services/conversationService'

const PAGE_SIZE = 30

/**
 * Capa de datos del hilo: carga inicial, paginación hacia atrás y
 * reconsulta para sincronizar el "visto".
 *
 * Vive separado de `useConversationMessages` porque son dos
 * responsabilidades distintas: esto habla con el servidor, aquel maneja
 * el tiempo real y el envío.
 */
export function useMessagePaging(conversationId) {
  const [messages, setMessages] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [isLoadingMore, setIsLoadingMore] = useState(false)
  const [hasMore, setHasMore] = useState(false)
  const [error, setError] = useState(null)
  const cursorRef = useRef(null)

  useEffect(() => {
    if (!conversationId) return

    let cancelled = false

    fetchMessages(conversationId, { limit: PAGE_SIZE })
      .then((page) => {
        if (cancelled) return
        // El backend devuelve de más nuevo a más viejo; la vista al revés.
        setMessages([...page.items].reverse())
        setHasMore(page.hasMore)
        cursorRef.current = page.nextCursor
        setError(null)
      })
      .catch((cause) => {
        if (!cancelled) setError(cause)
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [conversationId])

  /**
   * Reconsulta el hilo y actualiza solo el estado de "visto", sin
   * reemplazar los mensajes locales: así el scroll no salta al sincronizar.
   */
  const reload = useCallback(async () => {
    if (!conversationId) return

    try {
      const page = await fetchMessages(conversationId, { limit: PAGE_SIZE })

      setMessages((current) => {
        if (current.length === 0) return [...page.items].reverse()

        const byId = new Map(page.items.map((item) => [item.id, item]))
        return current.map((message) => {
          const fresh = byId.get(message.id)
          return fresh ? { ...message, isSeenByPeer: fresh.isSeenByPeer } : message
        })
      })

      cursorRef.current = page.nextCursor
      setHasMore(page.hasMore)
    } catch {
      // Se conserva lo ya cargado.
    }
  }, [conversationId])

  const loadMore = useCallback(async () => {
    if (!hasMore || isLoadingMore || !cursorRef.current) return

    setIsLoadingMore(true)
    try {
      const page = await fetchMessages(conversationId, {
        cursor: cursorRef.current,
        limit: PAGE_SIZE,
      })
      setMessages((current) => [...[...page.items].reverse(), ...current])
      setHasMore(page.hasMore)
      cursorRef.current = page.nextCursor
    } catch {
      // Si la página anterior falla se conserva lo ya cargado.
    } finally {
      setIsLoadingMore(false)
    }
  }, [conversationId, hasMore, isLoadingMore])

  const applyIncoming = useCallback((message) => {
    setMessages((current) =>
      current.some((m) => m.id === message.id) ? current : [...current, message],
    )
  }, [])

  const markSeenUntil = useCallback((readAt) => {
    setMessages((current) =>
      current.map((message) =>
        message.createdAt && new Date(message.createdAt).getTime() <= readAt
          ? { ...message, isSeenByPeer: true }
          : message,
      ),
    )
  }, [])

  /** Reemplaza un mensaje por la versión que devolvió el servidor. */
  const replaceOne = useCallback((updated) => {
    setMessages((current) =>
      current.map((message) => (message.id === updated.id ? { ...message, ...updated } : message)),
    )
  }, [])

  return {
    messages,
    isLoading,
    isLoadingMore,
    hasMore,
    error,
    loadMore,
    reload,
    applyIncoming,
    markSeenUntil,
    replaceOne,
  }
}