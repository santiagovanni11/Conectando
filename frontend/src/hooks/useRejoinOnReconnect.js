import { useEffect } from 'react'
import { onReconnected } from '../services/messagingHub'

/**
 * Vuelve a sumar la conversación al grupo cuando SignalR reconecta.
 *
 * La pertenencia a un grupo se guarda en la conexión, no en el usuario: al
 * reconectarse hay una conexión nueva, sin ningún grupo. Sin esto, quien dejó
 * la pestaña abierta un rato deja de recibir mensajes en vivo sin darse cuenta
 * — la pantalla se ve normal y los mensajes solo aparecen al recargar. En
 * desarrollo casi no pasa porque el servidor no se cae; en la nube, que apaga
 * el servicio por inactividad, pasa seguido.
 */
export function useRejoinOnReconnect(conversationId, joinConversation) {
  useEffect(() => {
    if (!conversationId) return
    return onReconnected(() => {
      joinConversation(conversationId)
    })
  }, [conversationId, joinConversation])
}
