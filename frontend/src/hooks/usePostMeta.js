import { useEffect } from 'react'
import { absoluteUrl } from '../utils/share'
import { setPostMeta, setDefaultMeta } from '../utils/meta'
import { ROUTES } from '../constants/routes'

/**
 * Actualiza el título y los tags Open Graph según el post abierto.
 * Si no hay post, vuelve a los valores por defecto del sitio.
 */
export function usePostMeta(post) {
  useEffect(() => {
    if (!post) {
      setDefaultMeta()
      return
    }

    setPostMeta({ post, url: absoluteUrl(ROUTES.post(post.id)) })
  }, [post])
}