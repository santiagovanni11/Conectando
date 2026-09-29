import { useState } from 'react'
import { authService } from '../services/authService'
import { isEmailValid, isValidPassword, isValidUserName } from '../utils/validators'
import Button from '../components/ui/Button'
import Input from '../components/ui/Input'
import PasswordInput from '../components/ui/PasswordInput'
import RegisterSuccess from '../components/auth/RegisterSuccess'

const EMPTY_STATE = { userName: '', email: '', password: '', confirmPassword: '' }

export default function RegisterPage() {
  const [form, setForm] = useState(EMPTY_STATE)
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [created, setCreated] = useState(false)

  function handleChange(event) {
    const { name, value } = event.target
    setForm((prev) => ({ ...prev, [name]: value }))
  }

  function validate() {
    if (!isValidUserName(form.userName)) {
      return 'El nombre de usuario debe tener entre 3 y 30 caracteres (letras, números o guión bajo).'
    }
    if (!isEmailValid(form.email)) return 'Ingresá un email válido.'
    if (!isValidPassword(form.password)) return 'La contraseña debe tener al menos 8 caracteres, una letra y un número.'
    if (form.password !== form.confirmPassword) return 'Las contraseñas no coinciden.'
    return ''
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')

    const validationError = validate()
    if (validationError) {
      setError(validationError)
      return
    }

    setSubmitting(true)
    try {
      await authService.register({
        userName: form.userName,
        email: form.email,
        password: form.password,
      })
      setCreated(true)
      setForm(EMPTY_STATE)
    } catch (err) {
      setError(err.message)
    } finally {
      setSubmitting(false)
    }
  }

  if (created) return <RegisterSuccess />

  return (
    <form onSubmit={handleSubmit} noValidate>
      <Input
        name="userName"
        label="Nombre de usuario"
        autoComplete="nickname"
        placeholder="Cómo te llaman los demás"
        value={form.userName}
        onChange={handleChange}
        required
      />
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
        autoComplete="new-password"
        hint="Mínimo 8 caracteres, con una letra y un número."
        value={form.password}
        onChange={handleChange}
        required
      />
      <PasswordInput
        name="confirmPassword"
        label="Confirmar contraseña"
        autoComplete="new-password"
        value={form.confirmPassword}
        onChange={handleChange}
        required
      />
      {error && (
        <p className="form-alert" role="alert">
          {error}
        </p>
      )}
      <Button type="submit" block size="lg" loading={submitting}>
        {submitting ? 'Creando cuenta…' : 'Crear cuenta'}
      </Button>
    </form>
  )
}