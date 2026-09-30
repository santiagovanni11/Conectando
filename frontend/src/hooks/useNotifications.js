import { useCallback, useEffect, useState } from 'react'
import { notificationService } from '../services/notificationService'

const PAGE_SIZE = 20

export function useNotifications() {
  const [attempt, setAttempt] = useState(0)
  const [items, setItems] = useState([])
  const [unreadCount, setUnreadCount] = useState(0)
  const [nextCursor, setNextCursor] = useState(null)
  const [hasMore, setHasMore] = useState(false)
  const [loading, setLoading] = useState(true)
  const [loadingMore, setLoadingMore] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true

    notificationService
      .getAll({ limit: PAGE_SIZE })
      .then((page) => {
        if (!active) return
        const list = page?.items ?? []
        setItems(Array.isArray(list) ? list : [])
        setNextCursor(page?.nextCursor ?? null)
        setHasMore(Boolean(page?.hasMore))
        setUnreadCount(page?.unreadCount ?? 0)
        setError('')
      })
      .catch((err) => {
        if (active) setError(err.message)
      })
      .finally(() => {
        if (active) setLoading(false)
      })

    return () => {
      active = false
    }
  }, [attempt])

  const refresh = useCallback(() => setAttempt((current) => current + 1), [])

  const loadMore = useCallback(async () => {
    if (loadingMore || !hasMore || !nextCursor) return
    setLoadingMore(true)
    try {
      const page = await notificationService.getAll({ cursor: nextCursor, limit: PAGE_SIZE })
      setItems((prev) => [...prev, ...(page?.items ?? [])])
      setNextCursor(page?.nextCursor ?? null)
      setHasMore(Boolean(page?.hasMore))
    } catch (err) {
      setError(err.message)
    } finally {
      setLoadingMore(false)
    }
  }, [hasMore, loadingMore, nextCursor])

  const markAsRead = useCallback(async (id) => {
    setItems((prev) => prev.map((n) => (n.id === id ? { ...n, isRead: true } : n)))
    setUnreadCount((count) => Math.max(0, count - 1))
    try {
      await notificationService.markAsRead(id)
    } catch {
      refresh()
    }
  }, [refresh])

  const markAllAsRead = useCallback(async () => {
    setItems((prev) => prev.map((n) => ({ ...n, isRead: true })))
    setUnreadCount(0)
    try {
      await notificationService.markAllAsRead()
    } catch {
      refresh()
    }
  }, [refresh])

  /**
   * Abrir la pantalla marca todo como leído.
   *
   * Es lo que hacen las demás apps y lo que espera el usuario: si la pantalla
   * está abierta, los avisos ya se leyeron. Sin esto el número de arriba se
   * queda ahí hasta que uno cliquea cada aviso uno por uno, o hasta que pasa
   * el refresco de respaldo.
   *
   * Va DESPUÉS de `markAllAsRead` y no antes, a propósito. El array de
   * dependencias se evalúa en la línea del `useEffect`, así que nombrar ahí
   * una `const` que todavía no se declaró la deja en zona muerta y revienta
   * con "Cannot access ... before initialization". La pantalla entera
   * desapareció con ese error, y el minificador lo reportaba como una
   * variable llamada `y`, sin rastro de qué lo causaba.
   */
  useEffect(() => {
    if (loading || unreadCount === 0) return

    markAllAsRead()
  }, [loading, unreadCount, markAllAsRead])

  return {
    items, unreadCount, hasMore, loading, loadingMore, error,
    refresh, loadMore, markAsRead, markAllAsRead,
  }
}