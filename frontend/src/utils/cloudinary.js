/**
 * Arma la URL de un avatar con el encuadre que eligió el usuario.
 *
 * El recorte no se guarda como archivo recortado: se guarda dónde mirar y se
 * le pide a Cloudinary que lo haga al servir. Así "ajustar" es instantáneo, se
 * repite las veces que haga falta sin volver a subir nada, y la foto original
 * queda intacta por si algún día se quiere mostrar entera.
 *
 * Cloudinary describe el recorte como una región: cuánto se recorta y desde
 * dónde arranca. El zoom achica la región (acercar = ver menos), y el
 * desplazamiento la mueve.
 */

const CLOUDINARY_HOST = 'res.cloudinary.com'
const AVATAR_SIZE = 400
const DEFAULT_ZOOM = 1

/** Zoomes que tienen sentido. Ir más allá de 4 solo produce bordes borrosos. */
export const MIN_ZOOM = 1
export const MAX_ZOOM = 4

const esCloudinary = (url) => typeof url === 'string' && url.includes(CLOUDINARY_HOST)

/**
 * @param {string|null} url URL original de Cloudinary.
 * @param {{zoom?: number, offsetX?: number, offsetY?: number}} framing
 * @returns {string|null} La misma URL con la transformación, o la original
 *   si el encuadre es el de siempre.
 */
export function avatarUrl(url, framing) {
  if (!url) return null
  if (!esCloudinary(url)) return url

  const zoom = clamp(framing?.zoom ?? DEFAULT_ZOOM, MIN_ZOOM, MAX_ZOOM)
  const offsetX = Math.round(framing?.offsetX ?? 0)
  const offsetY = Math.round(framing?.offsetY ?? 0)

  const sinAjuste = zoom === DEFAULT_ZOOM && offsetX === 0 && offsetY === 0
  if (sinAjuste) return url

  const lado = Math.round(AVATAR_SIZE / zoom)
  const recorte = `c-crop,w_${lado},h_${lado},x_${offsetX},y_${offsetY}`
  const salida = `w_${AVATAR_SIZE},h_${AVATAR_SIZE},c_fill,g_auto,q_auto,f_auto`

  return url.replace('/upload/', `/upload/${recorte}/${salida}/`)
}

const clamp = (valor, min, max) => Math.min(Math.max(valor, min), max)

/** ¿El usuario ajustó el avatar, o está en el encuadre de siempre? */
export function isCustomFraming(framing) {
  return (
    (framing?.zoom ?? DEFAULT_ZOOM) !== DEFAULT_ZOOM ||
    (framing?.offsetX ?? 0) !== 0 ||
    (framing?.offsetY ?? 0) !== 0
  )
}
