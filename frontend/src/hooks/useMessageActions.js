import { useCallback, useState } from 'react'
import {
  deleteMessage as deleteMessageRequest,
  editMessage as editMessageRequest,
} from '../services/conversationService'

/**
 * Acciones sobre un mensaje ya enviado: editar y eliminar.
 *
 * Viven aparte del orquestador porque son el unico camino capaz de
 * deshacer algo que ya se esta mostrando en pantalla. Cada una devuelve
 * si pudo, porque quien edita necesita saberlo para no cerrar el
 * formulario y perder el texto si el servidor lo rechazo.
 *
 * Se intenta por el hub, que guarda y ademas avisa al otro al instante.
 * Si no hay conexion se cae al REST, que guarda igual aunque sin aviso en
 * vivo. El segundo guardado no se hace nunca: seria guardar dos veces.
 */
export function useMessageActions({
  conversationId,
  editMessageViaHub,
  deleteMessageViaHub,
  replaceOne,
}) {
  const [actionError, setActionError] = useState('')

  const edit = useCallback(
    async (messageId, content) => {
      const trimmed = content?.trim() ?? ''
      if (!trimmed) return false

      try {
        if (await editMessageViaHub(conversationId, messageId, trimmed)) {
          // El evento MessageUpdated ya reemplazo el mensaje aca.
          setActionError('')
          return true
        }

        replaceOne(await editMessageRequest(conversationId, messageId, trimmed))
        setActionError('')
        return true
      } catch (cause) {
        setActionError(cause.message || 'No se pudo editar el mensaje.')
        return false
      }
    },
    [conversationId, editMessageViaHub, replaceOne],
  )

  const remove = useCallback(
    async (messageId) => {
      try {
        if (await deleteMessageViaHub(conversationId, messageId)) {
          setActionError('')
          return true
        }

        replaceOne(await deleteMessageRequest(conversationId, messageId))
        setActionError('')
        return true
      } catch (cause) {
        setActionError(cause.message || 'No se pudo eliminar el mensaje.')
        return false
      }
    },
    [conversationId, deleteMessageViaHub, replaceOne],
  )

  return { edit, remove, actionError }
}
