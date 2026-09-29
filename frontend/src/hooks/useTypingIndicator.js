import { useCallback, useEffect, useRef, useState } from 'react'
import { useConversationHub } from './useConversationHub'

/** Cada cuánto se reenvía el aviso: evita inundar el hub al escribir. */
const SEND_THROTTLE_MS = 2000

/** Cuánto sigue mostrando "escribiendo" sin recibir nada nuevo. */
const EXPIRE_MS = 4000

/**
 * Indicador de "escribiendo".
 *
 * Avisa al otro cada cierto tiempo mientras se escribe, y lo apaga solo
 * si deja de recibir avisos. El destinatario solo lo ve si tiene el chat
 * abierto, porque el evento va al grupo de la conversación.
 */
export function useTypingIndicator(conversationId, currentUserId) {
  const [peer, setPeer] = useState(null)
  const lastSentRef = useRef(0)
  const expireTimerRef = useRef(null)
  const { typing, connection } = useConversationHub()

  const stop = useCallback(() => {
    setPeer(null)
    clearTimeout(expireTimerRef.current)
  }, [])

  const onPeerTyping = useCallback((payload) => {
    if (payload?.conversationId !== conversationId) return
    // El evento no vuelve al que escribe, pero se filtra igual por si acaso.
    if (payload.userId === currentUserId) return

    setPeer({ id: payload.userId, name: payload.displayName })

    clearTimeout(expireTimerRef.current)
    expireTimerRef.current = setTimeout(() => setPeer(null), EXPIRE_MS)
  }, [conversationId, currentUserId])

  // El hub es una conexión compartida: hay que desuscribirse al salir.
  useEffect(() => {
    if (!connection) return undefined

    connection.on('UserTyping', onPeerTyping)
    return () => connection.off('UserTyping', onPeerTyping)
  }, [connection, onPeerTyping])

  useEffect(() => () => clearTimeout(expireTimerRef.current), [])

  /** Se llama en cada tecla. Solo envía si pasó el tiempo del throttle. */
  const notify = useCallback(() => {
    if (!conversationId) return

    const now = Date.now()
    if (now - lastSentRef.current < SEND_THROTTLE_MS) return

    lastSentRef.current = now
    typing(conversationId)
  }, [conversationId, typing])

  return { peerIsTyping: Boolean(peer), peerName: peer?.name, notify, stop }
}