import { useNavigate } from 'react-router-dom'
import Button from '../ui/Button'
import Icon from '../ui/Icon/Icon'
import { ROUTES } from '../../constants/routes'

export default function RegisterSuccess() {
  const navigate = useNavigate()

  return (
    <div className="register-success" role="status">
      <span className="register-success__icon" aria-hidden="true">
        <Icon name="check" size="xl" />
      </span>
      <h2 className="register-success__title">Cuenta creada</h2>
      <p className="register-success__text">
        Tu cuenta ya está lista. Ahora iniciá sesión con tu email y contraseña.
      </p>
      <Button type="button" variant="primary" size="lg" block onClick={() => navigate(ROUTES.login)}>
        Ir a iniciar sesión
      </Button>
    </div>
  )
}