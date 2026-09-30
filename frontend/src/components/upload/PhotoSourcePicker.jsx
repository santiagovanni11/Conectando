/**
 * Selector de fotos que en móvil ofrece las dos opciones nativas:
 * sacar una foto con la cámara o elegirla de la fototeca.
 *
 * <para>
 * Cada opción es una etiqueta con el <input type="file"> puesto encima, del
 * mismo tamaño y en la misma posición, y no un botón que lo dispara con
 * `.click()`. Eso es lo que lo hace funcionar en iOS: el usuario toca el
 * input de verdad, con el dedo, sobre un elemento que está dibujándose.
 *
 * <para>
 * Las dos formas que se hicieron antes dejaban la hoja del sistema en blanco.
 * Con `hidden`, porque iOS no dibuja bien un input que no está en pantalla. Y
 * con un input de un píxel más `.click()` desde JavaScript, porque iOS
 * presenta la hoja como si fuera respuesta a ese toque y encuentra el modal
 * todavía montado encima. Ninguna de las dos se nota en un escritorio.
 *
 * <para>
 * El modal no se cierra al elegir la opción, sino cuando el archivo llega. Si
 * se cerrara antes, el input se desmontaría mientras iOS todavía está armando
 * la hoja. Si el usuario cancela, el modal sigue abierto y puede probar la
 * otra opción, que es lo que esperaría.
 */
import { useState } from 'react'
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
  const [isChoosing, setIsChoosing] = useState(false)

  function handleFiles(event) {
    const files = event.target.files
    if (files && files.length > 0) onSelect?.(files)
    event.target.value = ''
    setIsChoosing(false)
  }

  return (
    <>
      <span className={className} onClick={() => !disabled && setIsChoosing(true)}>
        {trigger}
      </span>

      {isChoosing && (
        <Modal title="Agregar foto" onClose={() => setIsChoosing(false)}>
          <div className="photo-source">
            <label className="photo-source__option">
              <Icon name="camera" size="lg" />
              <span className="photo-source__label">Tomar foto</span>
              <input
                className="photo-source__input"
                type="file"
                accept={ACCEPTED}
                capture="environment"
                multiple={multiple}
                aria-label="Tomar foto"
                data-testid="camera-input"
                onChange={handleFiles}
              />
            </label>

            <label className="photo-source__option">
              <Icon name="image" size="lg" />
              <span className="photo-source__label">Elegir de la fototeca</span>
              <input
                className="photo-source__input"
                type="file"
                accept={ACCEPTED}
                multiple={multiple}
                aria-label="Elegir de la fototeca"
                data-testid="gallery-input"
                onChange={handleFiles}
              />
            </label>
          </div>
        </Modal>
      )}
    </>
  )
}