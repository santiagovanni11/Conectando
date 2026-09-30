import { useState } from 'react'
import Button from '../ui/Button'
import Input from '../ui/Input'
import PasswordInput from '../ui/PasswordInput'

/**
 * Paso 2: canjear el código y poner la contraseña nueva.
 *
 * El código va en un campo de texto normal y no en uno numérico a propósito:
 * muchos móviles no traen teclado numérico, y con `inputMode="numeric"` el
 * sistema lo ofrece pero el campo sigue siendo un número. Acá se fuerza el
 * teclado numérico por la pista visual sin perder el texto.
 */
export default function ConfirmCodeStep({ email, busy, error, onSubmit, onBack }) {
  const [form, setForm] = useState({ code: '', newPassword: '', confirmPassword: '' })

  function handleChange(event) {
    const { name, value } = event.target
    setForm((prev) => ({ ...prev, [name]: value }))
  }

  const completo = form.code.trim().length > 0 && form.newPassword.length > 0

  return (
    <form
      className="password-reset__form"
      noValidate
      onSubmit={(event) => {
        event.preventDefault()
        onSubmit(form)
      }}
    >
      <p className="password-reset__lead">
        Mandamos el código a <strong>{email}</strong>. Ponelo acá junto con la
        contraseña nueva.
      </p>

      <Input
        name="code"
        label="Código"
        inputMode="numeric"
        autoComplete="one-time-code"
        maxLength={6}
        placeholder="123456"
        className="password-reset__code"
        value={form.code}
        onChange={handleChange}
        required
        autoFocus
      />

      <PasswordInput
        name="newPassword"
        label="Contraseña nueva"
        autoComplete="new-password"
        value={form.newPassword}
        onChange={handleChange}
        required
      />

      <PasswordInput
        name="confirmPassword"
        label="Repetí la contraseña nueva"
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

      <Button type="submit" block size="lg" loading={busy} disabled={!completo}>
        {busy ? 'Cambiando…' : 'Cambiar contraseña'}
      </Button>

      <Button type="button" variant="ghost" block onClick={onBack} disabled={busy}>
        Usar otro correo
      </Button>
    </form>
  )
}
