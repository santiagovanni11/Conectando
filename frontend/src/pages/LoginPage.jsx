import { useState } from 'react'
import { useLocation, Link } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { authService } from '../services/authService'
import { ROUTES } from '../constants/routes'
import Button from '../components/ui/Button'
import Input from '../components/ui/Input'
import PasswordInput from '../components/ui/PasswordInput'

export default function LoginPage() {
  const { login } = useAuth()
  const location = useLocation()
  const [form, setForm] = useState({ email: '', password: '' })
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  // Aviso que deja otra pantalla al venir, como "tu contraseña cambió, entrá
  // de nuevo". Sin esto el usuario aterriza acá sin entender qué pasó.
  const notice = location.state?.notice

  function handleChange(event) {
    const { name, value } = event.target
    setForm((prev) => ({ ...prev, [name]: value }))
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setSubmitting(true)

    try {
      const session = await authService.login(form)
      login(session)
    } catch (err) {
      setError(err.message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} noValidate>
      {notice && (
        <p className="form-alert form-alert--success" role="status">
          {notice}
        </p>
      )}
      <Input
        name="email"
        label="Email"
        type="email"
        autoComplete="email"
        placeholder="tunombre@email.com"
        value={form.email}
        onChange={handleChange}
        required
      />
      <PasswordInput
        name="password"
        label="Contraseña"
        autoComplete="current-password"
        placeholder="••••••••••"
        value={form.password}
        onChange={handleChange}
        required
      />

      {/* Va debajo del campo y no arriba del formulario: es lo que uno busca
          cuando ya se equivocó, no lo que necesita para entrar. */}
      <Link className="login-forgot" to={ROUTES.forgotPassword}>
        ¿Olvidaste tu contraseña?
      </Link>
      {error && (
        <p className="form-alert" role="alert">
          {error}
        </p>
      )}
      <Button type="submit" block size="lg" loading={submitting}>
        {submitting ? 'Ingresando…' : 'Iniciar sesión'}
      </Button>
    </form>
  )
}