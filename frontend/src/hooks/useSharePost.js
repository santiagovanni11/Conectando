import { useCallback, useEffect, useRef, useState } from 'react'
import { shareUrl, absoluteUrl } from '../utils/share'
import { ROUTES } from '../constants/routes'

const FEEDBACK_MS = 2400

export function useSharePost() {
  const [message, setMessage] = useState('')
  const timeoutRef = useRef(null)

  useEffect(() => () => clearTimeout(timeoutRef.current), [])

  const share = useCallback(async (postId) => {
    const url = absoluteUrl(ROUTES.post(postId))
    const result = await shareUrl({ url, title: 'Conectando' })

    if (result === 'failed') {
      setMessage('No se pudo compartir')
    } else if (result === 'copied') {
      setMessage('Enlace copiado')
    } else {
      return
    }

    clearTimeout(timeoutRef.current)
    timeoutRef.current = setTimeout(() => setMessage(''), FEEDBACK_MS)
  }, [])

  return { share, message }
}