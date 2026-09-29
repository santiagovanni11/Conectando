import { isValidPassword } from './validators'

/**
 * Reglas de los dos formularios de cuenta.
 *
 * Son funciones puras y sin React a propósito: los dos formularios repiten
 * las mismas tres preguntas (¿está la actual? ¿coincide la repetida? ¿la
 * nueva sirve?) y mantenerlas en un lado solo evita que una se actualice y la
 * otra no.
 *
 * Ninguna reemplaza la validación del servidor: acá se evita mandar lo que
 * ya se sabe que va a fallar, allá está la que manda.
 */

const REQUIRED_CURRENT = 'Ingresá tu contraseña actual.'
const REQUIRED_CONFIRM = 'Repetila para confirmar.'
const MISMATCH = 'Las contraseñas no coinciden.'
const WEAK = 'Usá al menos 8 caracteres, con una letra y un número.'

export function validateChangePassword({ currentPassword, newPassword, confirmPassword }) {
  const fieldErrors = {}

  if (!currentPassword) fieldErrors.currentPassword = REQUIRED_CURRENT
  if (!newPassword) fieldErrors.newPassword = 'Ingresá una contraseña nueva.'
  else if (!isValidPassword(newPassword)) fieldErrors.newPassword = WEAK

  if (!confirmPassword) fieldErrors.confirmPassword = REQUIRED_CONFIRM
  else if (newPassword && newPassword !== confirmPassword) {
    fieldErrors.confirmPassword = MISMATCH
  }

  return { valid: Object.keys(fieldErrors).length === 0, fieldErrors }
}

export function validateDeleteAccount({ currentPassword, confirmPassword }) {
  const fieldErrors = {}

  if (!currentPassword) fieldErrors.currentPassword = REQUIRED_CURRENT
  if (!confirmPassword) fieldErrors.confirmPassword = REQUIRED_CONFIRM
  else if (currentPassword && currentPassword !== confirmPassword) {
    fieldErrors.confirmPassword = MISMATCH
  }

  return { valid: Object.keys(fieldErrors).length === 0, fieldErrors }
}
