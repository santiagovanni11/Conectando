import { useEffect } from 'react'
import IconButton from './IconButton'

/**
 * Modal genérico.
 *
 * El contenido va dentro de `modal__body`, que da el aire y el scroll. Si
 * se mete suelto, el panel queda sin padding y el contenido pegado al borde.
 */
export default function Modal({ title, children, onClose, className = '' }) {
  useEffect(() => {
    function handleKeyDown(event) {
      if (event.key === 'Escape') onClose()
    }

    // Sin esto el fondo sigue desplazándose por detrás del modal en mobile.
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'

    document.addEventListener('keydown', handleKeyDown)
    return () => {
      document.removeEventListener('keydown', handleKeyDown)
      document.body.style.overflow = previousOverflow
    }
  }, [onClose])

  return (
    <div className="modal" role="dialog" aria-modal="true" aria-label={title}>
      <div className="modal__backdrop" onClick={onClose} />

      <div className={`modal__panel ${className}`.trim()}>
        <header className="modal__header">
          <h2 className="modal__title">{title}</h2>
          <IconButton name="close" label="Cerrar" onClick={onClose} />
        </header>
        <div className="modal__body">{children}</div>
      </div>
    </div>
  )
}