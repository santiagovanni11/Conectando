import { useEffect, useRef, useState } from 'react'

/**
 * Menú de una burbuja: editar y eliminar.
 *
 * Se abre con el botón de la burbuja y se cierra al clickear afuera o al
 * presionar Escape, que es lo que espera cualquiera que lo use.
 */
export default function MessageMenu({ isOwn, onEdit, onDelete }) {
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
          {isOwn && (
            <button
              type="button"
              role="menuitem"
              className="message-menu__item"
              onClick={() => {
                setIsOpen(false)
                onEdit()
              }}
            >
              Editar
            </button>
          )}

          <button
            type="button"
            role="menuitem"
            className="message-menu__item message-menu__item--danger"
            onClick={() => {
              setIsOpen(false)
              onDelete()
            }}
          >
            Eliminar
          </button>
        </div>
      )}
    </span>
  )
}