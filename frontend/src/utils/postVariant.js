/**
 * Cómo se muestra un post según tenga fotos o no.
 *
 * Es una decisión de diseño, no de datos: la misma publicación se ve distinta
 * en el feed y en la vista de detalle. Por eso vive acá y no se calcula en el
 * medio de un componente: si mañana se cambia el criterio, se cambia en un
 * lugar y no hay que buscarla en tres archivos.
 *
 * - 'media'  la foto manda. Ocupa todo el ancho y es alta, como en Instagram.
 * - 'texto'  el texto manda. Compacto y sin elementos de más, como en X.
 */
export const POST_VARIANT = {
  MEDIA: 'media',
  TEXTO: 'texto',
}

export function postVariant(post) {
  return post?.media?.length > 0 ? POST_VARIANT.MEDIA : POST_VARIANT.TEXTO
}
