import { createContext, useContext } from 'react'
import { useReplyTarget } from '../../hooks/useReplyTarget'

const ReplyContext = createContext(null)

/** Envuelve la pantalla del chat y comparte el mensaje que se responde. */
export function MessageReplyProvider({ children }) {
  return <ReplyContext.Provider value={useReplyTarget()}>{children}</ReplyContext.Provider>
}

/**
 * Lee el mensaje que se está respondiendo.
 * Lanza si se usa fuera del provider: fallar al montar es mejor que
 * devolver undefined y romper en el primer clic.
 */
export function useMessageReply() {
  const value = useContext(ReplyContext)
  if (!value) throw new Error('useMessageReply se usó fuera de MessageReplyProvider')
  return value
}
