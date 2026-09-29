import { createContext, useEffect, useState } from 'react'
import { TOKEN_STORAGE_KEY } from '../constants/auth'
import { authService } from '../services/authService'
import { resetHubConnection } from '../services/messagingHub'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [token, setToken] = useState(() => localStorage.getItem(TOKEN_STORAGE_KEY))
  const [loading, setLoading] = useState(Boolean(localStorage.getItem(TOKEN_STORAGE_KEY)))

  useEffect(() => {
    if (!token) return

    async function loadUser() {
      try {
        const me = await authService.me()
        setUser(me)
      } catch {
        localStorage.removeItem(TOKEN_STORAGE_KEY)
        setToken(null)
      } finally {
        setLoading(false)
      }
    }

    loadUser()
  }, [token])

  function login(session) {
    // El WebSocket anterior sigue autenticado como el usuario previo.
    resetHubConnection()
    localStorage.setItem(TOKEN_STORAGE_KEY, session.token)
    setToken(session.token)
    setUser(session.user)
    setLoading(false)
  }

  function logout() {
    resetHubConnection()
    localStorage.removeItem(TOKEN_STORAGE_KEY)
    setToken(null)
    setUser(null)
    setLoading(false)
  }

  function updateUser(patch) {
    setUser((prev) => (prev ? { ...prev, ...patch } : prev))
  }

  return (
    <AuthContext.Provider value={{ user, token, loading, login, logout, updateUser }}>{children}</AuthContext.Provider>
  )
}

export default AuthContext