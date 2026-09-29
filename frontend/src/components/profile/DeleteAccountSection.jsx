import { useState } from 'react'
import { useDeleteAccount } from '../../hooks/useDeleteAccount'
import { useAuth } from '../../hooks/useAuth'
import PasswordInput from '../ui/PasswordInput'
import Button from '../ui/Button'

/**
 * Baja de la cuenta.
 *
 * Está escondida detrás de un botón: no es algo que uno haga de paso. Cuando
 * se abre, pide la contraseña y que se la escriba otra vez. Son dos controles
 * para lo mismo, y aun así el servidor los repite: acá se evita el error,
 * allá está la garantía.
 */
export default function DeleteAccountSection() {
  const { logout } = useAuth()
  const [open, setOpen] = useState(false)
  const { form, fieldErrors, error, submitting, handleChange, submit } = useDeleteAccount(logout)

  async function handleSubmit(event) {
    event.preventDefault()
    if (await submit()) setOpen(false)
  }

  if (!open) {
    return (
      <section className="account-danger">
        <div className="account-danger__head">
          <h3 className="account-danger__title">Eliminar cuenta</h3>
          <p className="account-danger__text">
            Se borran tu perfil y tu acceso. Tus conversaciones quedan, pero
            tus mensajes se muestran como “Usuario eliminado”.
          </p>
        </div>
        <Button variant="destructive" onClick={() => setOpen(true)}>
          Eliminar mi cuenta
        </Button>
      </section>
    )
  }

  return (
    <form className="account-form account-form--danger" onSubmit={handleSubmit} noValidate>
      <h3 className="account-form__title">Eliminar cuenta</h3>
      <p className="account-form__text">
        Esta acción no se puede deshacer. Confirmá con tu contraseña para
        seguir.
      </p>

      <PasswordInput
        label="Contraseña actual"
        name="currentPassword"
        autoComplete="current-password"
        value={form.currentPassword}
        error={fieldErrors.currentPassword}
        onChange={handleChange}
        required
      />

      <PasswordInput
        label="Repetí la contraseña"
        name="confirmPassword"
        autoComplete="current-password"
        value={form.confirmPassword}
        error={fieldErrors.confirmPassword}
        onChange={handleChange}
        required
      />

      {error && <p className="account-form__error">{error}</p>}

      <div className="account-form__actions">
        <Button variant="ghost" onClick={() => setOpen(false)} disabled={submitting}>
          Cancelar
        </Button>
        <Button variant="destructive" type="submit" loading={submitting}>
          Eliminar cuenta definitivamente
        </Button>
      </div>
    </form>
  )
}
