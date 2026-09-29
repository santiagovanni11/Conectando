import { useState } from 'react'
import { MAX_MESSAGE_LENGTH } from '../../constants/messages'

/** Campo para escribir un mensaje. Enter envía, Shift+Enter hace salto de línea. */
export default function MessageComposer({ onSend, onTyping, disabled, error }) {
  const [value, setValue] = useState('')

  const submit = () => {
    const trimmed = value.trim()
    if (!trimmed || disabled) return
    onSend(trimmed)
    setValue('')
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

      <div className="message-composer__row">
        <textarea
          className="message-composer__input"
          value={value}
          onChange={handleChange}
          onKeyDown={handleKeyDown}
          placeholder="Escribí un mensaje"
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