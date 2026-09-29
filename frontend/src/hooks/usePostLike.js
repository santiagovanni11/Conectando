import { useOptimisticToggle } from './useOptimisticToggle'
import { postService } from '../services/postService'

/**
 * Me gusta de una publicación: el ícono se enciende al instante y el
 * contador lo acompaña. Si el servidor lo rechaza, ambos vuelven atrás.
 *
 * `perform` recibe el estado previo, que es lo que decide si toca dar like
 * o quitarlo: sobre un post ya marcado lo que corresponde es Unlike.
 */
export function usePostLike(post) {
  const { state, pending, toggle } = useOptimisticToggle({
    initial: {
      liked: post?.likedByMe ?? false,
      count: post?.likesCount ?? 0,
    },
    preview: (s) => ({
      liked: !s.liked,
      count: s.liked ? s.count - 1 : s.count + 1,
    }),
    perform: async (s) => {
      const result = s.liked
        ? await postService.unlike(post.id)
        : await postService.like(post.id)
      return { liked: result.likedByMe, count: result.likesCount }
    },
    enabled: Boolean(post),
  })

  return { ...state, pending, toggle }
}