import { useCallback, useEffect, useState } from 'react'
import { socialService } from '../services/socialService'

/**
 * Carga una lista de usuarios (búsqueda o sugerencias) y permite
 * enviarles una solicitud de amistad actualizando el estado en el acto.
 */
export function useUserDiscovery(loader, deps = []) {
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [busyId, setBusyId] = useState(null)
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    let active = true

    Promise.resolve(loader())
      .then((list) => {
        if (!active) return
        setItems(Array.isArray(list) ? list : [])
        setError('')
      })
      .catch((err) => {
        if (!active) return
        setItems([])
        setError(err.message)
      })
      .finally(() => {
        if (active) setLoading(false)
      })

    return () => {
      active = false
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [attempt, ...deps])

  const refresh = useCallback(() => setAttempt((current) => current + 1), [])

  const sendRequest = useCallback(async (userId) => {
    setBusyId(userId)
    setItems((prev) => prev.map((u) => (u.id === userId ? { ...u, friendship: 'sent' } : u)))
    try {
      await socialService.sendRequest(userId)
    } catch (err) {
      setError(err.message)
      setItems((prev) => prev.map((u) => (u.id === userId ? { ...u, friendship: 'none' } : u)))
    } finally {
      setBusyId(null)
    }
  }, [])

  return { items, loading, error, busyId, sendRequest, refresh }
}