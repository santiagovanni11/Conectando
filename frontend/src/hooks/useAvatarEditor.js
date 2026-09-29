import { useState } from 'react'
import { mediaService } from '../services/mediaService'
import { userService } from '../services/userService'

/**
 * Cambia la foto de perfil o su encuadre, sin pasar por el formulario de edición.
 *
 * Va aparte de `useEditProfile` porque son dos cosas distintas: ese hook valida
 * y guarda un formulario con errores por campo, y acá no hay formulario, hay dos
 * acciones sueltas. Si compartieran estado, cambiar el avatar en la cabecera
 * tendría que arrastrar también los campos de texto del formulario.
 *
 * El backend espera el perfil entero, así que ambas acciones reenvían los
 * campos que ya tiene: si no, un cambio de foto borraría la biografía.
 */
export function useAvatarEditor(profile, onUpdated) {
  const [uploading, setUploading] = useState(false)
  const [savingFraming, setSavingFraming] = useState(false)
  const [error, setError] = useState('')

  const perfilActual = () => ({
    displayName: profile?.displayName ?? '',
    bio: profile?.bio ?? null,
    isPrivate: profile?.isPrivate ?? false,
  })

  async function replacePhoto(file) {
    setUploading(true)
    setError('')
    try {
      const url = await mediaService.uploadProfileImage(file)
      const updated = await userService.updateProfile({
        ...perfilActual(),
        profileImageUrl: url,
      })
      onUpdated?.(updated)
      return true
    } catch (err) {
      setError(err.message)
      return false
    } finally {
      setUploading(false)
    }
  }

  /**
   * Guarda el encuadre. Solo manda los tres campos del recorte: los offsets
   * van en píxeles de la original y el backend los valida contra su rango.
   */
  async function saveFraming(framing) {
    setSavingFraming(true)
    setError('')
    try {
      const updated = await userService.updateProfile({
        ...perfilActual(),
        profileImageZoom: framing.zoom,
        profileImageOffsetX: framing.offsetX,
        profileImageOffsetY: framing.offsetY,
      })
      onUpdated?.(updated)
      return true
    } catch (err) {
      setError(err.message)
      return false
    } finally {
      setSavingFraming(false)
    }
  }

  return { uploading, savingFraming, error, replacePhoto, saveFraming }
}
