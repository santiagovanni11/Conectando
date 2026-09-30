import { useState } from 'react'
import ReplyBar from './ReplyBar'
import { MAX_MESSAGE_LENGTH } from '../../constants/messages'

/**
 * Campo para escribir un mensaje. Enter envía, Shift+Enter hace salto de línea.
 *
 * La respuesta se resuelve acá adentro y no en la página: es estado del
 * formulario —se borra al enviar o al cancelar— y mezclarlo con el hilo
 * haría que la página anduviera pendiente de un borrador de texto.
 */
export default function MessageComposer({ onSend, onTyping, onCancelReply, replyTo, disabled, error }) {
  const [value, setValue] = useState('')

  const submit = () => {
    const trimmed = value.trim()
    if (!trimmed || disabled) return
    onSend(trimmed, replyTo?.id ?? null)
    setValue('')
    // La barra se baja en el mismo gesto que el texto. Si el envío falla, el
    // error aparece arriba y el mensaje se conserva, como ya pasaba; la cita
    // sigue siendo la que el usuario eligió.
    onCancelReply?.()
  }

  const handleChange = (event) => {
    setValue(event.target.value)
    // Solo tiene sentido avisar si queda texto: si borra todo, no escribe.
    if (event.target.value.trim()) onTyping?.()
  }

  const handleKeyDown = (event) => {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault()
      submit()
    }
  }

  return (
    <form
      className="message-composer"
      onSubmit={(event) => {
        event.preventDefault()
        submit()
      }}
    >
      {error && <p className="message-composer__error">{error}</p>}

      <ReplyBar message={replyTo} onCancel={onCancelReply} />

      <div className="message-composer__row">
        <textarea
          className="message-composer__input"
          value={value}
          onChange={handleChange}
          onKeyDown={handleKeyDown}
          placeholder={replyTo ? 'Escribí tu respuesta' : 'Escribí un mensaje'}
          rows={1}
          maxLength={MAX_MESSAGE_LENGTH}
          disabled={disabled}
          aria-label="Mensaje"
        />
        <button
          type="submit"
          className="message-composer__send"
          disabled={disabled || value.trim().length === 0}
        >
          Enviar
        </button>
      </div>
    </form>
  )
}