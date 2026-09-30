import { useCallback, useState } from 'react'

/**
 * Qué mensaje se está respondiendo.
 *
 * Vive en la página del chat y se reparte por contexto. La razón es que el
 * gesto para responder se dispara en la burbuja —dentro del hilo— y se
 * muestra sobre el composer, que es otro subtree: pasarlo por props
 * obligaría a atravesar MessageThread entero para llegar a MessageComposer,
 * y a sumar un prop más a cada fila del medio.
 */
export function useReplyTarget() {
  const [replyTo, setReplyTo] = useState(null)

  const startReply = useCallback((message) => {
    // Un mensaje borrado no se puede citar: no hay texto al que responder y
    // la cita mostraria el placeholder sin sentido.
    if (!message || message.isDeleted) return
    setReplyTo(message)
  }, [])

  const cancelReply = useCallback(() => setReplyTo(null), [])

  return { replyTo, startReply, cancelReply }
}
