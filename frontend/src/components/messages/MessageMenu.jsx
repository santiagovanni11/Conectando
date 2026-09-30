import { useEffect, useRef, useState } from 'react'

/**
 * Menú de una burbuja: responder, editar y eliminar.
 *
 * Se abre con el botón de la burbuja y se cierra al clickear afuera o al
 * presionar Escape, que es lo que espera cualquiera que lo use.
 *
 * Cada ítem se dibuja solo si su acción existe. Antes el "Eliminar" salía
 * siempre; con el reply puede pasar que haya acciones sin las otras, y un
 * botón que no hace nada es peor que un botón que no está.
 */
export default function MessageMenu({ isOwn, onEdit, onDelete, onReply }) {
  const [isOpen, setIsOpen] = useState(false)
  const containerRef = useRef(null)

  useEffect(() => {
    if (!isOpen) return undefined

    function onPointerDown(event) {
      if (!containerRef.current?.contains(event.target)) setIsOpen(false)
    }

    function onKeyDown(event) {
      if (event.key === 'Escape') setIsOpen(false)
    }

    document.addEventListener('pointerdown', onPointerDown)
    document.addEventListener('keydown', onKeyDown)

    return () => {
      document.removeEventListener('pointerdown', onPointerDown)
      document.removeEventListener('keydown', onKeyDown)
    }
  }, [isOpen])

  /** Corre la acción y cierra, para que el menú no quede abierto detrás. */
  const run = (action) => {
    setIsOpen(false)
    action?.()
  }

  return (
    <span className="message-menu" ref={containerRef}>
      <button
        type="button"
        className="message-menu__trigger"
        aria-label="Opciones del mensaje"
        aria-expanded={isOpen}
        onClick={() => setIsOpen((open) => !open)}
      >
        ⋮
      </button>

      {isOpen && (
        <div className="message-menu__popover" role="menu">
          {/* Primero, porque es lo que se usa casi siempre. Vale para los
              mensajes propios y los ajenos. */}
          {onReply && (
            <button
              type="button"
              role="menuitem"
              className="message-menu__item"
              onClick={() => run(onReply)}
            >
              Responder
            </button>
          )}

          {isOwn && onEdit && (
            <button
              type="button"
              role="menuitem"
              className="message-menu__item"
              onClick={() => run(onEdit)}
            >
              Editar
            </button>
          )}

          {onDelete && (
            <button
              type="button"
              role="menuitem"
              className="message-menu__item message-menu__item--danger"
              onClick={() => run(onDelete)}
            >
              Eliminar
            </button>
          )}
        </div>
      )}
    </span>
  )
}
