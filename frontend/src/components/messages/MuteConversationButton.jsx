import { useState } from 'react'
import Icon from '../ui/Icon/Icon'
import { setConversationMuted } from '../../services/conversationService'

/**
 * Botón de silenciar un chat.
 *
 * Silenciar no borra nada: deja de avisar cuando llega un mensaje. El
 * estado se refleja con una barra sobre el ícono, como en WhatsApp, para
 * que se note sin depender del color.
 */
export default function MuteConversationButton({ conversationId, isMuted, onChanged }) {
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  async function toggle() {
    if (busy) return
    setBusy(true)
    setError('')

    try {
      const result = await setConversationMuted(conversationId, !isMuted)
      onChanged?.(result.isMuted)
    } catch (cause) {
      setError(cause.message ?? 'No se pudo cambiar el chat.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <span className="conversation-mute">
      <button
        type="button"
        className="conversation-mute__button"
        onClick={toggle}
        disabled={busy}
        aria-pressed={isMuted}
        aria-label={isMuted ? 'Reactivar notificaciones' : 'Silenciar conversaciones'}
        title={isMuted ? 'Silenciada: tocá para reactivar' : 'Silenciar'}
      >
        <Icon name="bell" size="sm" />
        {isMuted && <span className="conversation-mute__slash" aria-hidden="true" />}
      </button>

      {error && (
        <span className="conversation-mute__error" role="alert">
          {error}
        </span>
      )}
    </span>
  )
}