import { useEffect, useState } from 'react'
import { socialService } from '../services/socialService'

export function useBlockedProfiles() {
  const [items, setItems] = useState([])
  const [phase, setPhase] = useState('loading')
  const [error, setError] = useState('')
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    let active = true

    socialService
      .getBlocks()
      .then((list) => {
        if (!active) return
        setItems(list)
        setPhase('loaded')
        setError('')
      })
      .catch((err) => {
        if (!active) return
        setItems([])
        setPhase('error')
        setError(err.message)
      })

    return () => {
      active = false
    }
  }, [attempt])

  /**
   * Desbloquear saca la fila de la lista. Si el POST falla, se recarga: es
   * preferible volver a pedir la lista que mostrar un bloqueo que ya no existe.
   */
  async function unblock(userId) {
    try {
      await socialService.unblock(userId)
      setItems((current) => current.filter((item) => item.user.id !== userId))
      return true
    } catch {
      setAttempt((current) => current + 1)
      return false
    }
  }

  return { items, phase, error, unblock, refresh: () => setAttempt((c) => c + 1) }
}