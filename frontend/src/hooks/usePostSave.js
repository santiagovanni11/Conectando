import { useOptimisticToggle } from './useOptimisticToggle'
import { postService } from '../services/postService'

/**
 * Guardar una publicación: el marcador se apaga al instante y, si el
 * servidor lo rechaza, vuelve a como estaba.
 */
export function usePostSave(post) {
  const { state, pending, toggle } = useOptimisticToggle({
    initial: { saved: post?.savedByMe ?? false },
    preview: (s) => ({ saved: !s.saved }),
    perform: async () => ({ saved: (await postService.toggleSave(post.id)).savedByMe }),
    enabled: Boolean(post),
  })

  return { ...state, pending, toggle }
}