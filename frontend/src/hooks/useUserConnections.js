import { useCallback, useEffect, useState } from 'react'
import { socialService } from '../services/socialService'
import { userService } from '../services/userService'

/** Carga la lista de amigos o seguidores de un usuario. */
export function useUserConnections(userId, kind) {
  const [items, setItems] = useState([])
  const [loaded, setLoaded] = useState(false)
  const [error, setError] = useState('')
  const [attempt, setAttempt] = useState(0)

  const isOpen = Boolean(userId) && Boolean(kind)

  // El componente se monta con key en cada apertura, así que la consulta
  // arranca siempre desde cero: acá solo se sincroniza con el servidor.
  useEffect(() => {
    if (!isOpen) return undefined

    let active = true

    const load = kind === 'friends'
      ? userService.getUserFriends(userId)
      : userService.getUserFollowers(userId)

    load
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
        if (active) setLoaded(true)
      })

    return () => {
      active = false
    }
  }, [isOpen, userId, kind, attempt])

  const refresh = useCallback(() => {
    setLoaded(false)
    setAttempt((current) => current + 1)
  }, [])

  async function sendRequest(targetId) {
    setItems((prev) => prev.map((u) => (u.id === targetId ? { ...u, friendship: 'sent' } : u)))
    try {
      await socialService.sendRequest(targetId)
    } catch (err) {
      setError(err.message)
      setItems((prev) => prev.map((u) => (u.id === targetId ? { ...u, friendship: 'none' } : u)))
    }
  }

  return { items, loading: !loaded, error, refresh, sendRequest }
}