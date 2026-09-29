import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

export default function GuestRoute() {
  const { user, loading } = useAuth()

  if (loading) {
    return <div className="loading">Cargando…</div>
  }

  if (user) {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}