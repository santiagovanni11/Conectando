import { useEffect } from 'react'
import { createPortal } from 'react-dom'
import IconButton from './IconButton'

/**
 * Modal genérico.
 *
 * <para>
 * El contenido va dentro de `modal__body`, que da el aire y el scroll. Si
 * se mete suelto, el panel queda sin padding y el contenido pegado al borde.
 *
 * <para>
 * Se dibuja en un portal contra el `body` y no donde le toque. No es un
 * detalle: el compositor de publicaciones es un modal y, al abrir las fotos,
 * abre otro modal adentro. Con los dos anidados, el de adentro queda dentro
 * del `position: fixed` del de afuera, y en iOS el panel interior se dibuja
 * mal. Por eso el selector de fotos salía en blanco.
 *
 * <para>
 * En un portal además el orden en el DOM es el orden en que se ven, así que
 * el último modal abierto es el de arriba sin depender del `z-index`.
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

  return createPortal(
    <div className="modal" role="dialog" aria-modal="true" aria-label={title}>
      <div className="modal__backdrop" onClick={onClose} />

      <div className={`modal__panel ${className}`.trim()}>
        <header className="modal__header">
          <h2 className="modal__title">{title}</h2>
          <IconButton name="close" label="Cerrar" onClick={onClose} />
        </header>
        <div className="modal__body">{children}</div>
      </div>
    </div>,
    document.body,
  )
}