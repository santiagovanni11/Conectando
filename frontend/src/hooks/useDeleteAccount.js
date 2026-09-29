import { useCallback, useState } from 'react'
import { accountService } from '../services/accountService'
import { validateDeleteAccount } from '../utils/accountForms'

/**
 * Baja de la cuenta.
 *
 * Pide la contraseña dos veces y, encima, que el botón se vuelva a abrir:
 * con la cuenta entera adentro, un doble clic que pasa la validación
 * borra algo que no se puede recuperar.
 */
export function useDeleteAccount(onDeleted) {
  const [form, setForm] = useState({ currentPassword: '', confirmPassword: '' })
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
    const { valid, fieldErrors: errors } = validateDeleteAccount(form)
    setFieldErrors(errors)
    if (!valid) return false

    setSubmitting(true)
    setError('')
    try {
      await accountService.deleteAccount({
        currentPassword: form.currentPassword,
        confirmPassword: form.confirmPassword,
      })

      setForm({ currentPassword: '', confirmPassword: '' })
      setSubmitting(false)
      onDeleted()
      return true
    } catch (cause) {
      setError(cause.message || 'No se pudo eliminar la cuenta.')
      return false
    } finally {
      setSubmitting(false)
    }
  }

  return { form, fieldErrors, error, submitting, handleChange, submit }
}
