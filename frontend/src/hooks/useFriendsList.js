import { useCallback, useEffect, useState } from 'react'
import { socialService } from '../services/socialService'

export function useFriendsList() {
  const [attempt, setAttempt] = useState(0)
  const [friends, setFriends] = useState([])
  const [error, setError] = useState('')
  const [busyId, setBusyId] = useState(null)
  const [actionError, setActionError] = useState('')

  useEffect(() => {
    let active = true

    socialService
      .getFriends()
      .then((list) => {
        if (!active) return
        setFriends(Array.isArray(list) ? list : [])
        setError('')
      })
      .catch((err) => {
        if (active) {
          setFriends([])
          setError(err.message)
        }
      })

    return () => {
      active = false
    }
  }, [attempt])

  const refresh = useCallback(() => setAttempt((current) => current + 1), [])

  async function removeFriend(friendId) {
    setBusyId(friendId)
    setActionError('')
    try {
      await socialService.removeFriend(friendId)
      setFriends((prev) => prev.filter((item) => item.user.id !== friendId))
    } catch (err) {
      setActionError(err.message)
    } finally {
      setBusyId(null)
    }
  }

  const status = error ? 'error' : 'loaded'

  return { friends, status, error, actionError, busyId, removeFriend, refresh }
}