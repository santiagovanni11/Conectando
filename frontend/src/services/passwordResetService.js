import { apiRequest } from './api'

/**
 * Recuperar el acceso a la cuenta.
 *
 * Son dos llamadas porque el usuario tiene que pasar por el correo en el
 * medio: primero pide el código y después lo canjea. Pedirlo todo de una
 * vez en un solo paso dejaría al servidor esperando un viaje de ida y vuelta
 * por el correo, con el formulario abierto y a la espera.
 */
export const passwordResetService = {
  requestCode(email) {
    return apiRequest('/api/auth/password-reset/request', {
      method: 'POST',
      body: { email },
    })
  },

  confirm({ email, code, newPassword, confirmPassword }) {
    return apiRequest('/api/auth/password-reset/confirm', {
      method: 'POST',
      body: { email, code, newPassword, confirmPassword },
    })
  },
}
