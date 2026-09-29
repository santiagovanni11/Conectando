import { Navigate, Route, Routes } from 'react-router-dom'
import AppErrorBoundary from './components/AppErrorBoundary'
import { AuthProvider } from './context/AuthContext'
import AppLayout from './layouts/AppLayout'
import AuthLayout from './layouts/AuthLayout'
import LoginVisual from './components/auth/LoginVisual'
import ProtectedRoute from './components/ProtectedRoute'
import GuestRoute from './components/GuestRoute'
import FeedPage from './pages/FeedPage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import ProfilePage from './pages/ProfilePage'
import EditProfilePage from './pages/EditProfilePage'
import PostDetailPage from './pages/PostDetailPage'
import FriendsPage from './pages/FriendsPage'
import HelpPage from './pages/HelpPage'
import NotificationsPage from './pages/NotificationsPage'
import ConversationsPage from './pages/ConversationsPage'
import ConversationPage from './pages/ConversationPage'
import { ROUTE_PATTERNS, ROUTES } from './constants/routes'

function App() {
  return (
    <AppErrorBoundary>
      <AuthProvider>
        <Routes>
          <Route element={<GuestRoute />}>
            <Route
              path="/login"
              element={
                <AuthLayout title="Iniciar sesión" visual={<LoginVisual />}>
                  <LoginPage />
                </AuthLayout>
              }
            />
            <Route
              path="/register"
              element={
                <AuthLayout title="Crear cuenta">
                  <RegisterPage />
                </AuthLayout>
              }
            />
          </Route>

          {/* Ayuda es pública a propósito: si a alguien le pasa algo justo
              al iniciar sesión, tiene que poder escribirnos igual. */}
          <Route
            path={ROUTES.help}
            element={
              <AppLayout>
                <HelpPage />
              </AppLayout>
            }
          />

          <Route element={<ProtectedRoute />}>
            <Route
              path="/"
              element={
                <AppLayout>
                  <FeedPage />
                </AppLayout>
              }
            />
            <Route
              path="/profile"
              element={
                <AppLayout>
                  <ProfilePage />
                </AppLayout>
              }
            />
            <Route
              path="/profile/edit"
              element={
                <AppLayout>
                  <EditProfilePage />
                </AppLayout>
              }
            />
            <Route
              path={ROUTE_PATTERNS.post}
              element={
                <AppLayout>
                  <PostDetailPage />
                </AppLayout>
              }
            />
            <Route
              path={ROUTE_PATTERNS.user}
              element={
                <AppLayout>
                  <ProfilePage />
                </AppLayout>
              }
            />
            <Route
              path={ROUTES.notifications}
              element={
                <AppLayout>
                  <NotificationsPage />
                </AppLayout>
              }
            />
            <Route
              path={ROUTES.messages}
              element={
                <AppLayout>
                  <ConversationsPage />
                </AppLayout>
              }
            />
            <Route
              path={ROUTE_PATTERNS.conversation}
              element={
                <AppLayout>
                  <ConversationPage />
                </AppLayout>
              }
            />
            <Route
              path={ROUTES.friends}
              element={
                <AppLayout>
                  <FriendsPage />
                </AppLayout>
              }
            />
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </AppErrorBoundary>
  )
}

export default App
