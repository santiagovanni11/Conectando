import { useCallback, useEffect, useState } from 'react'
import { socialService } from '../services/socialService'

export function useFriendRequests() {
  const [attempt, setAttempt] = useState(0)
  const [received, setReceived] = useState([])
  const [sent, setSent] = useState([])
  const [error, setError] = useState('')
  const [busyId, setBusyId] = useState(null)
  const [actionError, setActionError] = useState('')

  useEffect(() => {
    let active = true

    Promise.all([socialService.getReceivedRequests(), socialService.getSentRequests()])
      .then(([receivedList, sentList]) => {
        if (!active) return
        setReceived(Array.isArray(receivedList) ? receivedList : [])
        setSent(Array.isArray(sentList) ? sentList : [])
        setError('')
      })
      .catch((err) => {
        if (!active) return
        setReceived([])
        setSent([])
        setError(err.message)
      })

    return () => {
      active = false
    }
  }, [attempt])

  const refresh = useCallback(() => setAttempt((current) => current + 1), [])

  async function accept(userId) {
    setBusyId(userId)
    setActionError('')
    try {
      await socialService.acceptRequest(userId)
      setReceived((prev) => prev.filter((item) => item.user.id !== userId))
    } catch (err) {
      setActionError(err.message)
    } finally {
      setBusyId(null)
    }
  }

  async function reject(userId) {
    setBusyId(userId)
    setActionError('')
    try {
      await socialService.rejectRequest(userId)
      setReceived((prev) => prev.filter((item) => item.user.id !== userId))
    } catch (err) {
      setActionError(err.message)
    } finally {
      setBusyId(null)
    }
  }

  async function cancel(userId) {
    setBusyId(userId)
    setActionError('')
    try {
      await socialService.cancelRequest(userId)
      setSent((prev) => prev.filter((item) => item.user.id !== userId))
    } catch (err) {
      setActionError(err.message)
    } finally {
      setBusyId(null)
    }
  }

  const status = error ? 'error' : 'loaded'

  return { received, sent, status, error, actionError, busyId, accept, reject, cancel, refresh }
}