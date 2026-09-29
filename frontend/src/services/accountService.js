import { apiRequest } from './api'

/**
 * Gestión de la cuenta propia.
 *
 * Vive aparte de authService porque no es entrar ni salir: es cambiar la
 * contraseña y dar de baja la cuenta, que solo puede hacer su dueño.
 */
export const accountService = {
  changePassword(credentials) {
    return apiRequest('/api/account/change-password', {
      method: 'POST',
      body: credentials,
    })
  },

  deleteAccount(credentials) {
    return apiRequest('/api/account', {
      method: 'DELETE',
      body: credentials,
    })
  },
}
