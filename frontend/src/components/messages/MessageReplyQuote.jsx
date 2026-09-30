import { DELETED_PLACEHOLDER } from '../../constants/messages'

/**
 * Cita del mensaje original, arriba del texto de la respuesta.
 *
 * El texto llega recortado desde el servidor, así que acá no se toca nada:
 * mostrarlo entero duplicaría el peso de cada respuesta sin ganar nada, porque
 * la cita es una línea de referencia y no un segundo mensaje.
 */
export default function MessageReplyQuote({ replyTo }) {
  if (!replyTo) return null

  return (
    <blockquote className="message-quote">
      <span className="message-quote__name">{replyTo.senderDisplayName}</span>
      <span className="message-quote__preview">
        {replyTo.isDeleted ? DELETED_PLACEHOLDER : replyTo.preview}
      </span>
    </blockquote>
  )
}
