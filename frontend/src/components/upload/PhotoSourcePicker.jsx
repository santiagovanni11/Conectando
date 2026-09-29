/**
 * Selector de fotos que en móvil ofrece las dos opciones nativas:
 * sacar una foto con la cámara o elegirla de la fototeca.
 *
 * Se apoya en dos <input type="file"> ocultos: uno con `capture` fuerza
 * la cámara y el otro sin `capture` deja que el sistema ofrezca la galería.
 */
import { useRef, useState } from 'react'
import Icon from '../ui/Icon/Icon'
import Modal from '../ui/Modal'

const ACCEPTED = 'image/jpeg,image/png,image/webp,image/gif'

export default function PhotoSourcePicker({
  trigger,
  multiple = false,
  onSelect,
  disabled = false,
  className = '',
}) {
  const cameraRef = useRef(null)
  const galleryRef = useRef(null)
  const [isChoosing, setIsChoosing] = useState(false)

  function handleFiles(event) {
    const files = event.target.files
    if (files && files.length > 0) onSelect?.(files)
    event.target.value = ''
  }

  return (
    <>
      <input
        ref={cameraRef}
        type="file"
        accept={ACCEPTED}
        capture="environment"
        multiple={multiple}
        hidden
        data-testid="camera-input"
        onChange={handleFiles}
      />
      <input
        ref={galleryRef}
        type="file"
        accept={ACCEPTED}
        multiple={multiple}
        hidden
        data-testid="gallery-input"
        onChange={handleFiles}
      />

      <span className={className} onClick={() => !disabled && setIsChoosing(true)}>
        {trigger}
      </span>

      {isChoosing && (
        <Modal title="Agregar foto" onClose={() => setIsChoosing(false)}>
          <div className="photo-source">
            <button
              type="button"
              className="photo-source__option"
              onClick={() => {
                setIsChoosing(false)
                cameraRef.current?.click()
              }}
            >
              <Icon name="camera" size="lg" />
              <span className="photo-source__label">Tomar foto</span>
            </button>
            <button
              type="button"
              className="photo-source__option"
              onClick={() => {
                setIsChoosing(false)
                galleryRef.current?.click()
              }}
            >
              <Icon name="image" size="lg" />
              <span className="photo-source__label">Elegir de la fototeca</span>
            </button>
          </div>
        </Modal>
      )}
    </>
  )
}