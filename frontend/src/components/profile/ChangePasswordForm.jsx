import { useNavigate } from 'react-router-dom'
import { useChangePassword } from '../../hooks/useChangePassword'
import { useAuth } from '../../hooks/useAuth'
import { ROUTES } from '../../constants/routes'
import PasswordInput from '../ui/PasswordInput'
import Button from '../ui/Button'

/**
 * Formulario de cambio de contraseña.
 *
 * Pide la actual porque sin ella cualquiera con la sesión abierta —o con un
 * token copiado— dejaría la cuenta con una contraseña que el dueño no
 * conoce. La nueva se escribe dos veces: los asteriscos no avisan de una
 * errata, y una contraseña mal escrita es una cuenta perdida.
 *
 * Al cambiarla se sale de la cuenta y se vuelve al login. No es capricho:
 * el token de esta sesión murió con el cambio —por eso las demás sesiones
 * también—, así que seguir "conectado" sería una pantalla que parece viva
 * pero falla en la primera acción.
 */
export default function ChangePasswordForm() {
  const { logout } = useAuth()
  const navigate = useNavigate()
  const { form, fieldErrors, error, submitting, handleChange, submit } = useChangePassword(() => {
    logout()
    navigate(ROUTES.login, {
      replace: true,
      state: { notice: 'Tu contraseña fue actualizada. Volvé a iniciar sesión con la nueva.' },
    })
  })

  async function handleSubmit(event) {
    event.preventDefault()
    await submit()
  }

  return (
    <form className="account-form" onSubmit={handleSubmit} noValidate>
      <h3 className="account-form__title">Cambiar contraseña</h3>
      <p className="account-form__text">
        Al cambiarla se cierran las sesiones abiertas en otros dispositivos.
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
        label="Contraseña nueva"
        name="newPassword"
        autoComplete="new-password"
        hint="Mínimo 8 caracteres, con una letra y un número."
        value={form.newPassword}
        error={fieldErrors.newPassword}
        onChange={handleChange}
        required
      />

      <PasswordInput
        label="Repetí la contraseña nueva"
        name="confirmPassword"
        autoComplete="new-password"
        value={form.confirmPassword}
        error={fieldErrors.confirmPassword}
        onChange={handleChange}
        required
      />

      {error && <p className="account-form__error">{error}</p>}

      <Button type="submit" loading={submitting}>
        Cambiar contraseña
      </Button>
    </form>
  )
}
