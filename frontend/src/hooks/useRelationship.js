import { useEffect, useState } from 'react'
import { socialService } from '../services/socialService'

export function useRelationship(userId, onChanged) {
  const [attempt, setAttempt] = useState(0)
  const [data, setData] = useState(null)
  const [error, setError] = useState('')
  const [actionError, setActionError] = useState('')
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    let active = true

    async function load() {
      try {
        const result = await socialService.getStatus(userId)
        if (active) {
          setData(result)
          setError('')
        }
      } catch (err) {
        if (active) {
          setData(null)
          setError(err.message)
        }
      }
    }

    load()

    return () => {
      active = false
    }
  }, [attempt, userId])

  async function run(action) {
    setBusy(true)
    setActionError('')
    try {
      await action()
      setAttempt((current) => current + 1)
      // Avisa a la página para que refresque publicaciones/amigos/seguidores.
      onChanged?.()
    } catch (err) {
      setActionError(err.message)
    } finally {
      setBusy(false)
    }
  }

  function refresh() {
    setError('')
    setActionError('')
    setAttempt((current) => current + 1)
  }

  const status = error ? 'error' : data ? 'loaded' : 'loading'

  return { status, data, error, actionError, busy, refresh, run }
}