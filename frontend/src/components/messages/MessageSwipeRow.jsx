import Icon from '../ui/Icon/Icon'
import { useSwipeToReply } from '../../hooks/useSwipeToReply'

/**
 * Fila del hilo con el gesto de deslizar para responder.
 *
 * La flecha vive detrás de la burbuja y se revela a medida que esta se
 * corre. Es la única señal de que el gesto sirve para algo: sin ella el
 * usuario no tiene por qué sospechar que un mensaje se puede responder, y
 * en computador el gesto no existe.
 *
 * `touch-action: pan-y` (en el CSS) deja el scroll vertical en manos del
 * navegador; el JS solo corre cuando ya se decidió que el gesto es
 * horizontal.
 */
export default function MessageSwipeRow({ onReply, disabled = false, children }) {
  const { shift, swipeProps } = useSwipeToReply(onReply, { enabled: !disabled })

  return (
    <div className="message-row" style={{ '--swipe': `${shift}px` }} {...swipeProps}>
      <span className="message-row__hint" aria-hidden="true">
        <Icon name="reply" size="md" />
      </span>

      <div className="message-row__content">{children}</div>
    </div>
  )
}
