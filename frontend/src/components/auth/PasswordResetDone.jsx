import Button from '../ui/Button'
import Icon from '../ui/Icon/Icon'
import { ROUTES } from '../../constants/routes'

/**
 * Confirmación de que la contraseña cambió.
 *
 * Es una pantalla y no un cartel encima del formulario porque el código ya
 * se consumió: si el usuario recarga, vuelve al principio en vez de quedar
 * en un formulario que no va a volver a funcionar. El botón lo manda al
 * login con un aviso, que es lo único que le queda por hacer.
 */
export default function PasswordResetDone({ onRestart }) {
  return (
    <div className="password-reset__done" role="status">
      <span className="password-reset__icon" aria-hidden="true">
        <Icon name="check" size="xl" />
      </span>

      <h2 className="password-reset__done-title">Contraseña cambiada</h2>

      <p className="password-reset__done-text">
        Ya podés entrar con la nueva. Por seguridad, cerramos las sesiones
        que tuvieras abiertas en otros dispositivos.
      </p>

      <Button
        type="button"
        variant="primary"
        size="lg"
        block
        onClick={() => window.location.assign(ROUTES.login)}
      >
        Ir a iniciar sesión
      </Button>

      <Button type="button" variant="ghost" block onClick={onRestart}>
        Recuperar otra cuenta
      </Button>
    </div>
  )
}
