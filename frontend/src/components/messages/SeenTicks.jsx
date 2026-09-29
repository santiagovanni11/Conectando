import Icon from '../ui/Icon/Icon'

/**
 * Doble tilde verde, como los ticks de WhatsApp. Se comparte entre la
 * burbuja del hilo y la fila de la lista de conversaciones.
 */
export default function SeenTicks({ className = '' }) {
  return (
    <span className={`message-bubble__ticks ${className}`} aria-hidden="true">
      <Icon name="check" size="sm" className="icon--tick" />
      <Icon name="check" size="sm" className="icon--tick" />
    </span>
  )
}