import { useCallback, useState } from 'react'
import { passwordResetService } from '../services/passwordResetService'

/** Dos pantallas: se pide el correo, después se canjea el código. */
const PASO = { CORREO: 'correo', CANJE: 'canje' }

/**
 * Recuperación de contraseña.
 *
 * El código y la contraseña nueva van en la misma pantalla a propósito. Si
 * fueran dos pasos, el usuario tendría que escribir su contraseña nueva para
 * recién enterarse de que el código estaba mal: se tira el trabajo hecho y
 * hay que volver atrás. Con una sola pantalla, el error aparece al confirmar
 * y el código queda a la vista para corregir.
 *
 * El correo se guarda al pasar al segundo paso para no obligar a escribirlo
 * dos veces.
 */
export function usePasswordReset() {
  const [step, setStep] = useState(PASO.CORREO)
  const [email, setEmail] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  const back = useCallback(() => {
    setStep(PASO.CORREO)
    setError('')
  }, [])

  const restart = useCallback(() => {
    setStep(PASO.CORREO)
    setEmail('')
    setError('')
  }, [])

  /**
   * Pide el código y pasa a la segunda pantalla.
   *
   * Avanza siempre, exista o no la cuenta. El servidor responde lo mismo en
   * los dos casos a propósito, así que no hay forma de que el usuario sepa
   * por qué no llegó nada; quedarse esperando le diría más.
   */
  const requestCode = useCallback(async (correo) => {
    setBusy(true)
    setError('')
    try {
      await passwordResetService.requestCode(correo)
      setEmail(correo)
      setStep(PASO.CANJE)
      return true
    } catch (cause) {
      setError(cause.message || 'No se pudo enviar el código.')
      return false
    } finally {
      setBusy(false)
    }
  }, [])

  /** Canjea el código y deja la contraseña nueva puesta. */
  const confirm = useCallback(
    async ({ code, newPassword, confirmPassword }) => {
      setBusy(true)
      setError('')
      try {
        await passwordResetService.confirm({ email, code, newPassword, confirmPassword })
        return true
      } catch (cause) {
        setError(cause.message || 'No se pudo cambiar la contraseña.')
        return false
      } finally {
        setBusy(false)
      }
    },
    [email],
  )

  return { step, email, busy, error, requestCode, confirm, back, restart, setEmail }
}
