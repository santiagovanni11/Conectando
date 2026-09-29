/**
 * Qué fotos acepta la app.
 *
 * Vive en un solo lugar porque la decisión se repite: el filtro del `<input>`
 * para que el sistema no ofrezca lo que no sirve, y el control previo a la
 * subida para avisar rápido sin gastar un upload. Si se desincronizan, el
 * usuario ve fotos selected que después el servidor rechaza.
 */

export const MAX_IMAGE_BYTES = 8 * 1024 * 1024

/** Para el atributo `accept` de los inputs de archivo. */
export const ACCEPTED_IMAGE_TYPES =
  'image/jpeg,image/png,image/webp,image/gif,.jpg,.jpeg,.png,.webp,.gif'

const TIPOS = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'image/gif']

/**
 * ".jpeg" y ".jpe" son JPEG igual que ".jpg". El servidor los acepta; si el
 * navegador no, la foto se rechaza antes de salir de la pantalla y el usuario
 * no entiende por qué: el error dice "solo JPG" y el archivo se llama .jpeg.
 */
const EXTENSIONES = ['.jpg', '.jpeg', '.jpe', '.png', '.webp', '.gif']

const sinPunto = (nombre) => nombre?.toLowerCase()?.split('.').pop() ?? ''

/**
 * Se acepta si el tipo **o** la extensión coinciden. `file.type` viene vacío
 * en algunos selectores de archivos de Android y en algunos arrastres, y con
 * un chequeo estricto eso rechaza fotos que están perfecto.
 */
export function isSupportedImage(file) {
  if (!file) return false
  const porTipo = TIPOS.includes((file.type || '').toLowerCase())
  const porExtension = EXTENSIONES.includes(`.${sinPunto(file.name)}`)
  return porTipo || porExtension
}

/** El mensaje único que ve el usuario, para que sea el mismo en todos lados. */
export const IMAGE_ERROR_MESSAGE =
  'Solo imágenes JPG, PNG, WEBP o GIF de hasta 8 MB.'
