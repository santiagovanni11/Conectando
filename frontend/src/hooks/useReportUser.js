import { useCallback, useState } from 'react'
import { reportService } from '../services/reportService'

/**
 * Envío de una denuncia.
 *
 * Es una sola acción sin estado que sincronizar: un envío, un error o un
 * confirmation. Por eso vive en un hook y no dentro del modal.
 */
export function useReportUser() {
  const [sending, setSending] = useState(false)
  const [error, setError] = useState('')
  const [sent, setSent] = useState(false)

  const send = useCallback(async (targetUserId, reason, details) => {
    setSending(true)
    setError('')
    setSent(false)

    try {
      await reportService.create(targetUserId, reason, details)
      setSent(true)
      return true
    } catch (cause) {
      setError(cause.message ?? 'No se pudo enviar la denuncia.')
      return false
    } finally {
      setSending(false)
    }
  }, [])

  return { send, sending, error, sent, reset: () => { setError(''); setSent(false) } }
}