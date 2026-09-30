/**
 * Selector de fotos que en móvil ofrece las dos opciones nativas:
 * sacar una foto con la cámara o elegirla de la fototeca.
 *
 * Se apoya en dos <input type="file">: uno con `capture` fuerza la cámara y el
 * otro sin `capture` deja que el sistema ofrezca la galería.
 *
 * <para>
 * Los dos inputs no van con `hidden`. En iOS, un input de archivo oculto hace
 * que el menú de "Cámara / Fototeca /Buscar" se abra en blanco: las opciones
 * están pero no se ven. Se tapan con `.photo-source__input`, que los deja
 * dibujándose en un píxel en vez de sacarlos de pantalla.
 *
 * <para>
 * También llevan `tabIndex={-1}` porque, al estar recortados y no ocultos,
 * el teclado los alcanzaría y dejaría el foco en algo que no se ve. Quien
 * navega con teclado usa los botones del modal, que son los que tienen nombre
 * y descripción.
 */
import { useRef, useState } from 'react'
import Icon from '../ui/Icon/Icon'
import Modal from '../ui/Modal'

import { ACCEPTED_IMAGE_TYPES as ACCEPTED } from '../../utils/images'

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
        className="photo-source__input"
        type="file"
        accept={ACCEPTED}
        capture="environment"
        multiple={multiple}
        tabIndex={-1}
        aria-hidden="true"
        data-testid="camera-input"
        onChange={handleFiles}
      />
      <input
        ref={galleryRef}
        className="photo-source__input"
        type="file"
        accept={ACCEPTED}
        multiple={multiple}
        tabIndex={-1}
        aria-hidden="true"
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