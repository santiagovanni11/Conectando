import { useCallback, useEffect, useState } from 'react'
import { startHubConnection } from '../services/messagingHub'

/**
 * Expone los métodos del hub (entrar/salir de una conversación, enviar)
 * y el estado de la conexión.
 */
export function useConversationHub() {
  const [connection, setConnection] = useState(null)

  useEffect(() => {
    let cancelled = false

    startHubConnection()
      .then((hub) => {
        if (!cancelled) setConnection(hub)
      })
      .catch(() => {
        if (!cancelled) setConnection(null)
      })

    return () => {
      cancelled = true
    }
  }, [])

  const invoke = useCallback(
    async (method, ...args) => {
      if (!connection) return false
      try {
        await connection.invoke(method, ...args)
        return true
      } catch {
        // Si el hub falla se cae al REST, así que no se corta la UX.
        return false
      }
    },
    [connection],
  )

  return {
    isReady: connection?.state === 'Connected',
    joinConversation: useCallback(
      (id) => invoke('JoinConversation', id),
      [invoke],
    ),
    leaveConversation: useCallback(
      (id) => invoke('LeaveConversation', id),
      [invoke],
    ),
    sendViaHub: useCallback(
      (id, content, replyToMessageId = null) =>
        invoke('SendMessage', id, content, replyToMessageId),
      [invoke],
    ),
    markReadViaHub: useCallback(
      (id) => invoke('MarkAsRead', id),
      [invoke],
    ),

    /**
     * Edita y borra por el hub para que el otro se entere al instante.
     * Devuelven false si no hay conexión, y ahí la pantalla cae al REST.
     */
    editMessageViaHub: useCallback(
      (conversationId, messageId, content) =>
        invoke('EditMessage', conversationId, messageId, content),
      [invoke],
    ),

    deleteMessageViaHub: useCallback(
      (conversationId, messageId) => invoke('DeleteMessage', conversationId, messageId),
      [invoke],
    ),
    typing: useCallback(
      (id) => invoke('Typing', id),
      [invoke],
    ),
  }
}