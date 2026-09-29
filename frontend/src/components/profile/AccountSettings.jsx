import ChangePasswordForm from './ChangePasswordForm'
import DeleteAccountSection from './DeleteAccountSection'

/**
 * La pestaña "Cuenta": cambiar la contraseña y dar de baja el usuario.
 *
 * Son dos formularios y están juntos a propósito. Los dos piden lo mismo —
 * la contraseña actual — porque los dos son decisiones que exigen saber
 * quién sos: uno por seguridad, el otro porque no tiene vuelta atrás.
 */
export default function AccountSettings() {
  return (
    <div className="account-settings">
      <ChangePasswordForm />
      <DeleteAccountSection />
    </div>
  )
}
