import { useCallback, useEffect } from 'react'
import { fetchMessages, markConversationAsRead } from '../services/conversationService'

/** Cada cuánto se reconsultan los mensajes del hilo abierto. */
export const SEEN_POLL_MS = 4000

/**
 * Red de seguridad del "visto".
 *
 * El WebSocket es el camino rápido, pero el visto no puede depender solo
 * de él: si el lector abrió el chat sin conexión, su aviso de lectura no
 * llega en vivo. Este poll reconsulta el hilo cada pocos segundos y solo
 * con una conversación abierta, así que es barato.
 */
export function useSeenSync({ conversationId, markRead, onMessages }) {
  const sync = useCallback(async () => {
    if (!conversationId) return

    try {
      const page = await fetchMessages(conversationId, { limit: 30 })
      onMessages(page)
    } catch {
      // Se conserva lo ya cargado.
    }
  }, [conversationId, onMessages])

  const syncAndMark = useCallback(
    () => markRead().then(sync).catch(() => {}),
    [markRead, sync],
  )

  useEffect(() => {
    if (!conversationId) return

    const timer = setInterval(syncAndMark, SEEN_POLL_MS)
    return () => clearInterval(timer)
  }, [conversationId, syncAndMark])

  // Al volver a la pestaña también se sincroniza: si el otro leyó mientras
  // no se miraba, el visto tiene que aparecer.
  useEffect(() => {
    function onVisible() {
      if (document.visibilityState === 'visible') syncAndMark()
    }

    document.addEventListener('visibilitychange', onVisible)
    window.addEventListener('focus', onVisible)

    return () => {
      document.removeEventListener('visibilitychange', onVisible)
      window.removeEventListener('focus', onVisible)
    }
  }, [syncAndMark])
}

export { markConversationAsRead }