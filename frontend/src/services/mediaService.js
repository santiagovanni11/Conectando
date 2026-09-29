import { apiRequest } from './api'

export const mediaService = {
  /** Sube una imagen y devuelve la URL pública. */
  async uploadProfileImage(file) {
    const body = new FormData()
    body.append('file', file)
    const profile = await apiRequest('/api/users/me/profile-image', { method: 'POST', body })
    return profile.profileImageUrl
  },
}