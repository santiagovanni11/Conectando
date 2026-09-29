import { useCallback, useEffect, useState } from 'react'
import { fetchNavCounts } from '../services/conversationService'
import { useMessageHub } from './useMessageHub'

/** Segundos entre refrescos cuando no llega nada por el hub. */
const POLL_MS = 30000

/**
 * Contadores de aviso de la navegación: mensajes sin leer, solicitudes
 * pendientes y notificaciones. Se refrescan al entrar, cuando llega algo
 * por el hub y como respaldo con un poll.
 */
export function useNavCounts() {
  const [counts, setCounts] = useState({
    unreadMessages: 0,
    pendingFriendRequests: 0,
    unreadNotifications: 0,
  })

  const refresh = useCallback(async () => {
    try {
      setCounts(await fetchNavCounts())
    } catch {
      // Si falla se conserva lo anterior: es un dato informativo.
    }
  }, [])

  useEffect(() => {
    let active = true

    const load = async () => {
      try {
        const data = await fetchNavCounts()
        if (active) setCounts(data)
      } catch {
        // Sin conexión: se reintenta en el siguiente ciclo.
      }
    }

    load()
    const timer = setInterval(load, POLL_MS)

    return () => {
      active = false
      clearInterval(timer)
    }
  }, [])

  // Un mensaje nuevo o una notificación cambian los contadores al instante.
  useMessageHub({
    onMessage: () => refresh(),
    onSeen: () => refresh(),
  })

  // Al volver a la pestaña: puede haber cambios mientras no se miraba.
  useEffect(() => {
    function onVisible() {
      if (document.visibilityState === 'visible') refresh()
    }

    document.addEventListener('visibilitychange', onVisible)
    return () => document.removeEventListener('visibilitychange', onVisible)
  }, [refresh])

  return { counts, refresh }
}