import { useCallback, useEffect, useState } from 'react'
import { markConversationAsRead, sendMessage } from '../services/conversationService'
import { useConversationHub } from './useConversationHub'
import { useMessageActions } from './useMessageActions'
import { useMessageHub } from './useMessageHub'
import { useMessagePaging } from './useMessagePaging'
import { useSeenSync } from './useSeenSync'

/**
 * Orquestador de un hilo de conversacion.
 *
 * Su responsabilidad es el tiempo real (entrar y salir del grupo, recibir
 * mensajes y avisos de lectura) y el envio. La carga y la paginacion viven
 * en `useMessagePaging`; la red de seguridad del "visto", en `useSeenSync`;
 * editar y eliminar, en `useMessageActions`. Asi cada modulo tiene una sola
 * razon para cambiar.
 */
export function useConversationMessages(conversationId, currentUserId = null) {
  const {
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
  } = useMessagePaging(conversationId)

  const [sendError, setSendError] = useState('')
  const {
    joinConversation,
    leaveConversation,
    sendViaHub,
    markReadViaHub,
    editMessageViaHub,
    deleteMessageViaHub,
  } = useConversationHub()

  const { edit, remove, actionError } = useMessageActions({
    conversationId,
    editMessageViaHub,
    deleteMessageViaHub,
    replaceOne,
  })

  // El hub avisa al otro al instante. Si no esta disponible se marca por
  // REST igual, para que el "visto" quede guardado aunque tarde en verse.
  const markRead = useCallback(
    () =>
      markReadViaHub(conversationId).then((ok) =>
        ok ? true : markConversationAsRead(conversationId),
      ),
    [conversationId, markReadViaHub],
  )

  useSeenSync({ conversationId, markRead, onMessages: reload })

  useEffect(() => {
    if (!conversationId) return

    joinConversation(conversationId)
    markRead().catch(() => {})

    return () => leaveConversation(conversationId)
  }, [conversationId, joinConversation, leaveConversation, markRead])

  useMessageHub({
    onMessage: (message) => {
      if (message.conversationId !== conversationId) return
      // El emisor ya lo tiene en pantalla: si no se descarta, al cambiar
      // la difusion a todo el grupo aparecia duplicado.
      if (message.sender?.id === currentUserId) return
      applyIncoming(message)
    },
    onSeen: (payload) => {
      if (payload?.conversationId !== conversationId) return
      if (payload.readerId === currentUserId) return
      markSeenUntil(new Date(payload.readAt).getTime())
    },
    // Quien edita o borra ya actualizo su propia vista con la respuesta;
    // el evento llega para el otro, que no tiene que recargar.
    onUpdated: (message) => {
      if (message.conversationId !== conversationId) return
      replaceOne(message)
    },
    onDeleted: (message) => {
      if (message.conversationId !== conversationId) return
      replaceOne(message)
    },
  })

  return {
    messages,
    isLoading,
    isLoadingMore,
    hasMore,
    error,
    sendError,
    actionError,
    loadMore,
    reload,
    send: (content) => sendThrough(conversationId, content, sendViaHub, setSendError),
    edit,
    remove,
  }
}

/**
 * Envia por el hub y, si no se pudo, por el REST.
 *
 * Los dos caminos terminan en el mismo servicio, asi que avisar por
 * cualquiera deja el mensaje guardado igual; lo unico que cambia es el
 * tiempo. El error se toma aca y no de un evento del hub: el hub ya no
 * avisa los fallos, sube la excepcion, y por eso `sendViaHub` devuelve
 * false y el REST es quien tira con el mensaje de verdad.
 */
async function sendThrough(conversationId, content, sendViaHub, onError) {
  const trimmed = content?.trim() ?? ''
  if (!trimmed) return

  try {
    if (!(await sendViaHub(conversationId, trimmed))) {
      await sendMessage(conversationId, trimmed)
    }
    onError('')
  } catch (cause) {
    // Con `||` y no con `??`: un error sin mensaje llega como cadena vacía,
    // que `??` deja pasar y dejaria la pantalla sin aviso.
    onError(cause.message || 'No se pudo enviar el mensaje.')
  }
}
