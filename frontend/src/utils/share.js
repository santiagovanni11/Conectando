/**
 * Comparte una URL: usa el diálogo nativo si existe y cae a copiar al portapapeles.
 * Devuelve 'shared' | 'copied' | 'failed'.
 */
export async function shareUrl({ url, title, text }) {
  try {
    if (navigator.share) {
      await navigator.share({ url, title, text })
      return 'shared'
    }
  } catch (err) {
    // El usuario canceló el diálogo: no es un error a mostrar.
    if (err?.name === 'AbortError') return 'shared'
  }

  try {
    await navigator.clipboard.writeText(url)
    return 'copied'
  } catch {
    return 'failed'
  }
}

export function absoluteUrl(path) {
  return new URL(path, window.location.origin).toString()
}