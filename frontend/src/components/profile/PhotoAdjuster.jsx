import { useCallback, useRef, useState } from 'react'
import Button from '../ui/Button'
import { MAX_ZOOM, avatarUrl } from '../../utils/cloudinary'

/** Tamaño de la imagen que se muestra mientras se ajusta, en píxeles de pantalla. */
const VISTA = 320

/**
 * Recorta la foto moviéndola y acercándola, como el avatar de Instagram.
 *
 * Muestra la imagen con el encuadre actual y deja arrastrar y hacer zoom. No
 * recorta nada en el navegador ni sube nada: devuelve dónde mirar, y es
 * Cloudinary quien aplica eso al servir la imagen. Por eso se puede abrir,
 * mover y guardar tantas veces como haga falta sin gastar una subida.
 *
 * El desplazamiento se guarda en píxeles de la imagen original: la conversión
 * entre lo que se ve en pantalla y eso depende del tamaño mostrado, y hacerlo
 * acá es lo que mantiene la vista previa y el resultado final iguales.
 */
export default function PhotoAdjuster({ src, framing, onSave, onCancel, saving }) {
  const [borrador, setBorrador] = useState(framing)
  const [imagen, setImagen] = useState(null)
  const arrastre = useRef(null)

  // Se necesita el tamaño real de la imagen para saber cuánto se puede mover
  // en cada dirección: si la foto es angosta, no hay a dónde ir para un lado.
  const alCargar = (evento) => {
    setImagen({ ancho: evento.target.naturalWidth, alto: evento.target.naturalHeight })
  }

  const limites = calcularLimites(imagen, borrador.zoom)
  const vista = avatarUrl(src, borrador)

  const onPointerDown = (evento) => {
    evento.currentTarget.setPointerCapture?.(evento.pointerId)
    arrastre.current = { x: evento.clientX, y: evento.clientY, ...borrador }
  }

  const onPointerMove = (evento) => {
    if (!arrastre.current) return
    const escala = escalaAPixelesOriginales(imagen, arrastre.current.zoom)
    setBorrador((actual) => ({
      ...actual,
      offsetX: limitar(arrastre.current.offsetX - (evento.clientX - arrastre.current.x) / escala, limites.x),
      offsetY: limitar(arrastre.current.offsetY - (evento.clientY - arrastre.current.y) / escala, limites.y),
    }))
  }

  const onPointerUp = () => {
    arrastre.current = null
  }

  const acercar = useCallback(() => {
    setBorrador((a) => ({ ...a, zoom: Math.min(MAX_ZOOM, Number((a.zoom + 0.1).toFixed(2))) }))
  }, [])

  const alejar = useCallback(() => {
    setBorrador((a) => ({ ...a, zoom: Math.max(1, Number((a.zoom - 0.1).toFixed(2))) }))
  }, [])

  const alGuardar = () => onSave(borrador)

  return (
    <div className="adjuster" role="dialog" aria-modal="true" aria-label="Ajustar foto de perfil">
      <h2 className="adjuster__title">Ajustar foto</h2>
      <p className="adjuster__hint">Arrastrá la foto para elegir qué se ve.</p>

      <div
        className="adjuster__frame"
        onPointerDown={onPointerDown}
        onPointerMove={onPointerMove}
        onPointerUp={onPointerUp}
        onPointerCancel={onPointerUp}
      >
        {vista ? <img className="adjuster__image" src={vista} alt="" onLoad={alCargar} /> : null}
        <div className="adjuster__grid" aria-hidden="true" />
      </div>

      <div className="adjuster__zoom">
        <Button type="button" onClick={alejar} aria-label="Alejar" disabled={borrador.zoom <= 1}>
          −
        </Button>
        <input
          type="range"
          min="1"
          max={MAX_ZOOM}
          step="0.1"
          value={borrador.zoom}
          onChange={(e) => setBorrador((a) => ({ ...a, zoom: Number(e.target.value) }))}
          aria-label="Acercar"
        />
        <Button type="button" onClick={acercar} aria-label="Acercar" disabled={borrador.zoom >= MAX_ZOOM}>
          +
        </Button>
      </div>

      <div className="adjuster__actions">
        <Button type="button" variant="ghost" onClick={onCancel}>
          Cancelar
        </Button>
        <Button type="button" onClick={alGuardar} loading={saving}>
          Guardar
        </Button>
      </div>
    </div>
  )
}

/** Cuánto se puede desplazar en cada eje sin dejar de mostrarse la imagen. */
function calcularLimites(imagen, zoom) {
  if (!imagen) return { x: { min: -1000, max: 1000 }, y: { min: -1000, max: 1000 } }
  const lado = Math.round(VISTA / zoom)
  return {
    x: { min: -(imagen.ancho - lado), max: 0 },
    y: { min: -(imagen.alto - lado), max: 0 },
  }
}

const limitar = (valor, { min, max }) => Math.round(Math.min(Math.max(valor, min), max))

/** Píxeles de pantalla por píxel de la imagen original, para el zoom actual. */
function escalaAPixelesOriginales(imagen, zoom) {
  if (!imagen?.ancho) return 1
  return (imagen.ancho / (VISTA / zoom)) / zoom
}
