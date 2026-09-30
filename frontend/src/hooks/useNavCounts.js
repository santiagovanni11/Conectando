import { useCallback, useEffect, useState } from 'react'
import { fetchNavCounts } from '../services/conversationService'
import { useMessageHub } from './useMessageHub'

/** Segundos entre refrescos cuando no llega nada por el hub. */
const POLL_MS = 30000

/** La forma que el resto de la app da por cierta: tres números, siempre. */
const SIN_CUENTOS = Object.freeze({
  unreadMessages: 0,
  pendingFriendRequests: 0,
  unreadNotifications: 0,
})

/**
 * Normaliza lo que llega del servidor o del hub a la forma exacta.
 *
 * No es desconfianza gratuita: `AppNavigation` vive arriba de todas las
 * páginas y lee estos tres números sin protegerse, porque este hook se
 * compromete a entregarlos siempre. Si llegara un `null` —una respuesta
 * vacía del proxy, un evento del hub con otra forma— el error boundary se
 * comería la app entera y el usuario vería "Algo salió mal" por un dato
 * que solo es informativo.
 *
 * Va acá y no repartido con `?.` en cada consumidor: la garantía se
 * sostiene en un solo lugar.
 */
export function toCounts(data) {
  if (!data || typeof data !== 'object') return SIN_CUENTOS

  const entero = (valor) => {
    const numero = Number(valor)
    return Number.isFinite(numero) && numero > 0 ? Math.floor(numero) : 0
  }

  return {
    unreadMessages: entero(data.unreadMessages),
    pendingFriendRequests: entero(data.pendingFriendRequests),
    unreadNotifications: entero(data.unreadNotifications),
  }
}

/**
 * Contadores de aviso de la navegación: mensajes sin leer, solicitudes
 * pendientes y notificaciones. Se refrescan al entrar, cuando llega algo
 * por el hub y como respaldo con un poll.
 */
export function useNavCounts() {
  const [counts, setCounts] = useState(SIN_CUENTOS)

  const refresh = useCallback(async () => {
    try {
      setCounts(toCounts(await fetchNavCounts()))
    } catch {
      // Si falla se conserva lo anterior: es un dato informativo.
    }
  }, [])

  useEffect(() => {
    let active = true

    const load = async () => {
      try {
        const data = await fetchNavCounts()
        if (active) setCounts(toCounts(data))
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

  // El camino normal: el servidor avisa con los números ya contados y solo
  // hay que pintarlos. El poll de arriba queda como red de seguridad para lo
  // que no pasa por el hub, y por si la conexión se perdió sin que se note.
  useMessageHub({
    onNavCounts: (data) => setCounts(toCounts(data)),
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
