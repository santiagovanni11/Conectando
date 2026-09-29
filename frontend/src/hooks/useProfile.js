import { useEffect, useState } from 'react'
import { userService } from '../services/userService'

export function useProfile(userId) {
  const [attempt, setAttempt] = useState(0)
  const [profile, setProfile] = useState(null)
  const [error, setError] = useState('')
  const [notFound, setNotFound] = useState(false)

  useEffect(() => {
    let active = true

    async function fetchProfile() {
      try {
        const result = userId
          ? await userService.getPublicProfile(userId)
          : await userService.getOwnProfile()

        if (active) {
          setProfile(result)
          setError('')
          setNotFound(false)
        }
      } catch (err) {
        if (active) {
          setProfile(null)
          setError(err.message)
          setNotFound(err.status === 404)
        }
      }
    }

    fetchProfile()

    return () => {
      active = false
    }
  }, [attempt, userId])

  const status = error ? 'error' : profile ? 'loaded' : 'loading'

  function reload() {
    setProfile(null)
    setError('')
    setNotFound(false)
    setAttempt((current) => current + 1)
  }

  return { status, profile, error, notFound, reload }
}