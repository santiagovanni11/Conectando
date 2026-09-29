/**
 * Datos de prueba compartidos por los tests del hilo.
 *
 * Viven aparte para que MessageThread, MessageBubble y MessageMenu puedan
 * usar la misma forma de mensaje sin duplicarla: si el DTO cambia, se toca
 * una vez.
 */

export const CURRENT_USER_ID = 'user-1'
export const PEER_ID = 'user-2'

/**
 * Arma un mensaje con los datos minimos que necesita la burbuja.
 * `extra` agrega los campos opcionales (isSeenByPeer, isEdited, isDeleted).
 */
export function message(id, senderId, content, extra = {}) {
  return {
    id,
    conversationId: 'conv-1',
    content,
    createdAt: '2026-01-01T10:00:00Z',
    sender: {
      id: senderId,
      displayName: senderId === CURRENT_USER_ID ? 'Yo' : 'Ana',
    },
    ...extra,
  }
}
