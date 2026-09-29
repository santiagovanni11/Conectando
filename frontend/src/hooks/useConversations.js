import { useCallback, useEffect, useState } from 'react'
import { fetchConversations } from '../services/conversationService'
import { useMessageHub } from './useMessageHub'

/** Carga la lista de conversaciones y la mantiene al día con el hub. */
export function useConversations() {
  const [conversations, setConversations] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(null)

  const load = useCallback(async () => {
    try {
      const result = await fetchConversations()
      setConversations(result)
      setError(null)
    } catch (cause) {
      setError(cause)
    } finally {
      setIsLoading(false)
    }
  }, [])

  // El setState ocurre tras un await, no de forma síncrona: por eso no
  // dispara el aviso de render en cascada.
  useEffect(() => {
    let active = true

    fetchConversations()
      .then((result) => {
        if (active) setConversations(result)
      })
      .catch((cause) => {
        if (active) setError(cause)
      })
      .finally(() => {
        if (active) setIsLoading(false)
      })

    return () => {
      active = false
    }
  }, [])

  // Un mensaje nuevo reordena la lista y suma el no leído; un aviso de
  // lectura enciende el tick. Así el "Visto" se ve sin abrir el chat.
  useMessageHub({
    onMessage: (message) => {
      setConversations((current) => applyIncoming(current, message))
    },
    // Editar o borrar cambia el texto que se ve en la lista: sin esto, el
    // otro vería la previsualización vieja hasta recargar.
    onUpdated: (message) => {
      setConversations((current) => applyChange(current, message))
    },
    onDeleted: (message) => {
      setConversations((current) => applyChange(current, message))
    },
    onSeen: (payload) => {
      setConversations((current) => applySeen(current, payload))
    },
  })

  const markAsRead = useCallback((conversationId) => {
    setConversations((current) =>
      current.map((c) => (c.id === conversationId ? { ...c, unreadCount: 0 } : c)),
    )
  }, [])

  /**
   * Silenciar no recarga la lista: el endpoint ya devuelve el estado, así
   * que alcanza con cambiar esa fila y la pantalla no parpadea.
   */
  const setMuted = useCallback((conversationId, isMuted) => {
    setConversations((current) =>
      current.map((c) => (c.id === conversationId ? { ...c, isMuted } : c)),
    )
  }, [])

  return { conversations, isLoading, error, reload: load, markAsRead, setMuted }
}

function applyIncoming(current, message) {
  const without = current.filter((c) => c.id !== message.conversationId)
  const previous = current.find((c) => c.id === message.conversationId)
  if (!previous) return current

  return [
    { ...previous, lastMessage: message, updatedAt: message.createdAt },
    ...without,
  ]
}

/**
 * Reemplaza el último mensaje cuando fue editado o borrado.
 *
 * No reordena la lista: el cambio no altera la hora, así que la
 * conversación tiene que quedar donde estaba.
 */
function applyChange(current, message) {
  return current.map((conversation) =>
    conversation.id === message.conversationId
      ? { ...conversation, lastMessage: message }
      : conversation,
  )
}

/**
 * Marca el último mensaje como visto si llegó después de que el otro
 * abriera el chat. Solo se enciende en mensajes propios.
 */
function applySeen(current, payload) {
  if (!payload?.conversationId || !payload.readAt) return current

  const readAt = new Date(payload.readAt).getTime()

  return current.map((conversation) => {
    if (conversation.id !== payload.conversationId) return conversation

    const last = conversation.lastMessage
    if (!last || new Date(last.createdAt).getTime() > readAt) return conversation

    return {
      ...conversation,
      lastMessage: { ...last, isSeenByPeer: true },
    }
  })
}