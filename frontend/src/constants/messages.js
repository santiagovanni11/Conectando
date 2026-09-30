/** Límite de caracteres, alineado con MessageLimits del backend. */
export const MAX_MESSAGE_LENGTH = 2000

/**
 * Texto que se ve en lugar de un mensaje eliminado.
 *
 * Vive acá y no en el componente porque aparece en dos lugares: dentro del
 * chat y en la previsualización de la lista de conversaciones, igual que en
 * WhatsApp. Si estuviera duplicado, corregir uno dejaría el otro viejo.
 */
export const DELETED_PLACEHOLDER = 'Este mensaje fue eliminado'

export function formatConversationTime(isoString) {
  if (!isoString) return ''

  const date = new Date(isoString)
  const today = new Date()
  const sameDay = date.toDateString() === today.toDateString()

  return sameDay
    ? date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
    : date.toLocaleDateString([], { day: '2-digit', month: 'short' })
}

export function conversationTitle(conversation) {
  if (!conversation?.peers?.length) return 'Conversación'
  return conversation.peers.map((peer) => peer.displayName).join(', ')
}

/**
 * La persona con la que se está chateando, si la hay.
 *
 * <para>
 * Devuelve `null` en un chat de grupo: ahí no hay un perfil al que llevar, y
 * enlazar el nombre de la conversación a una sola persona sería llevar a
 * cualquier grupo al perfil equivocado.
/// </para>
 */
export function conversationPeer(conversation) {
  const peers = conversation?.peers ?? []
  return peers.length === 1 ? peers[0] : null
}

/**
 * Texto de la previsualización en la lista de conversaciones.
 *
 * Un mensaje borrado llega con el contenido vacío —el texto ya no existe—,
 * así que sin esta comprobación la fila se vería vacía y el usuario no
 * sabría que hubo algo ahí. Se muestra el aviso, como en WhatsApp.
 */
export function conversationPreview(conversation) {
  const last = conversation?.lastMessage
  if (!last) return 'Sin mensajes todavía'
  if (last.isDeleted) return DELETED_PLACEHOLDER
  return last.content || 'Sin mensajes todavía'
}