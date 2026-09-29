import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

export default function ProtectedRoute() {
  const { token, user, loading } = useAuth()

  if (loading) {
    return <div className="loading">Cargando…</div>
  }

  if (!token || !user) {
    return <Navigate to="/login" replace />
  }

  return <Outlet />
}