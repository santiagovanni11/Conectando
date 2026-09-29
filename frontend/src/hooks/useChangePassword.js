import { useCallback, useState } from 'react'
import { accountService } from '../services/accountService'
import { validateChangePassword } from '../utils/accountForms'

/**
 * Cambio de contraseña.
 *
 * El formulario no manda nada hasta que los tres campos están completos y
 * la repetida coincide: es la validación del servidor, pero sin gastar un
 * viaje ni dejar al usuario esperando por un error que ya se sabía.
 *
 * Cuando sale bien llama a `onChanged` y no se queda mostrando nada: el
 * token de esta sesión ya no sirve —cambiar la contraseña lo mata—, así que
 * quien decide a dónde va el usuario de ahora en más es el componente.
 */
export function useChangePassword(onChanged) {
  const [form, setForm] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' })
  const [fieldErrors, setFieldErrors] = useState({})
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const handleChange = useCallback((event) => {
    const { name, value } = event.target
    setForm((prev) => ({ ...prev, [name]: value }))
    setFieldErrors((prev) => ({ ...prev, [name]: undefined }))
    setError('')
  }, [])

  async function submit() {
    const { valid, fieldErrors: errors } = validateChangePassword(form)
    setFieldErrors(errors)
    if (!valid) return false

    setSubmitting(true)
    setError('')
    try {
      await accountService.changePassword({
        currentPassword: form.currentPassword,
        newPassword: form.newPassword,
        confirmPassword: form.confirmPassword,
      })

      // Se vacía el formulario: la contraseña nueva ya no debe quedar a la
      // vista en un input, ni en el historial de lo que el navegador sugiere.
      setForm({ currentPassword: '', newPassword: '', confirmPassword: '' })
      setFieldErrors({})
      onChanged()
      return true
    } catch (cause) {
      setError(cause.message || 'No se pudo cambiar la contraseña.')
      return false
    } finally {
      setSubmitting(false)
    }
  }

  return { form, fieldErrors, error, submitting, handleChange, submit }
}
