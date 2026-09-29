import { useEffect, useRef } from 'react'
import Icon from '../ui/Icon/Icon'

/**
 * Las dos acciones sobre la foto de perfil, como un menú emergente.
 *
 * Es un `role="menu"` a propósito: con un div común, un lector de pantalla
 * anuncia "imagen" y el usuario no sabe que hay acciones ahí adentro. Con el
 * rol de menú, anuncia "menú, dos elementos".
 *
 * Cierra solo: Escape, un clic afuera, o la tecla Tab que lo saca.
 */
export default function AvatarActionsMenu({ open, onClose, onNewPhoto, onAdjust, canAdjust }) {
  const contenedor = useRef(null)

  useEffect(() => {
    if (!open) return

    const alClicFuera = (evento) => {
      if (!contenedor.current?.contains(evento.target)) onClose()
    }

    // Tab entre elementos: el menú es chico, dos flechas y listo. El foco
    // arranca en el primero para que Escape y Tab funcionen de entrada.
    const alTeclear = (evento) => {
      if (evento.key === 'Escape') onClose()
    }

    document.addEventListener('pointerdown', alClicFuera)
    document.addEventListener('keydown', alTeclear)
    return () => {
      document.removeEventListener('pointerdown', alClicFuera)
      document.removeEventListener('keydown', alTeclear)
    }
  }, [open, onClose])

  if (!open) return null

  const elegir = (accion) => () => {
    onClose()
    accion()
  }

  return (
    <div className="avatar-actions" ref={contenedor} role="menu" aria-label="Foto de perfil">
      <button type="button" role="menuitem" className="avatar-actions__item" onClick={elegir(onNewPhoto)}>
        <Icon name="camera" size="sm" />
        <span>Nueva foto</span>
      </button>

      {canAdjust && (
        <button
          type="button"
          role="menuitem"
          className="avatar-actions__item"
          onClick={elegir(onAdjust)}
        >
          <Icon name="edit" size="sm" />
          <span>Ajustar foto</span>
        </button>
      )}
    </div>
  )
}
