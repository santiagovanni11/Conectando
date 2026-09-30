import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { usePasswordReset } from '../hooks/usePasswordReset'
import { ROUTES } from '../constants/routes'
import RequestCodeStep from '../components/auth/RequestCodeStep'
import ConfirmCodeStep from '../components/auth/ConfirmCodeStep'
import PasswordResetDone from '../components/auth/PasswordResetDone'

/**
 * Recuperar el acceso a la cuenta: /recuperar
 *
 * Es pública a propósito, como la ayuda: si lo que se rompió es justamente
 * no poder entrar, esconder la pantalla detrás del login sería dejar la
 * mitad del problema sin resolver.
 */
export default function ForgotPasswordPage() {
  const navigate = useNavigate()
  const { step, email, busy, error, requestCode, confirm, back, restart } = usePasswordReset()
  const [listo, setListo] = useState(false)

  if (listo) {
    return <PasswordResetDone onRestart={restart} />
  }

  return (
    <section className="password-reset">
      {/* Sin encabezado propio: el título lo pone el AuthLayout que envuelve
          a esta página, como en el login y el registro. Ponerlo acá también
          lo duplicaba en pantalla. */}
      {step === 'correo' ? (
        <RequestCodeStep
          busy={busy}
          error={error}
          onSubmit={requestCode}
          onBack={() => navigate(ROUTES.login)}
        />
      ) : (
        <ConfirmCodeStep
          email={email}
          busy={busy}
          error={error}
          onSubmit={async (form) => {
            if (await confirm(form)) setListo(true)
          }}
          onBack={back}
        />
      )}
    </section>
  )
}
