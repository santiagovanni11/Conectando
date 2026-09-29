import { apiRequest } from './api'

export const authService = {
  register(credentials) {
    return apiRequest('/api/auth/register', { method: 'POST', body: credentials })
  },

  login(credentials) {
    return apiRequest('/api/auth/login', { method: 'POST', body: credentials })
  },

  me() {
    return apiRequest('/api/auth/me')
  },
}