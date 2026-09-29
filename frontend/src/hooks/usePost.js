import { useCallback, useEffect, useState } from 'react'
import { postService } from '../services/postService'

const HIDDEN_STATUSES = [403, 404]

export function usePost(postId) {
  const [attempt, setAttempt] = useState(0)
  const [result, setResult] = useState({ id: null, post: null, error: '', notFound: false })

  useEffect(() => {
    if (!postId) return undefined

    let active = true

    postService
      .getPost(postId)
      .then((post) => {
        if (active) setResult({ id: postId, post, error: '', notFound: false })
      })
      .catch((err) => {
        if (!active) return
        setResult({
          id: postId,
          post: null,
          error: err.message,
          notFound: HIDDEN_STATUSES.includes(err.status),
        })
      })

    return () => {
      active = false
    }
  }, [postId, attempt])

  const reload = useCallback(() => setAttempt((current) => current + 1), [])

  // Sin id nunca hay un post que mostrar: se deriva en render, no en un efecto.
  if (!postId) {
    return { post: null, error: '', notFound: true, status: 'not-found', reload }
  }

  // Se ignora el resultado si pertenece a otro post (navegación rápida).
  const isCurrent = result.id === postId
  const post = isCurrent ? result.post : null
  const error = isCurrent ? result.error : ''
  const notFound = isCurrent && result.notFound

  const status = notFound ? 'not-found' : error ? 'error' : post ? 'loaded' : 'loading'

  return { post, error, notFound, status, reload }
}