import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'
import { useEditProfile } from '../../hooks/useEditProfile'
import { mediaService } from '../../services/mediaService'
import { PROFILE_LIMITS } from '../../constants/profile'
import { ROUTES } from '../../constants/routes'
import Input from '../ui/Input'
import Textarea from '../ui/Textarea'
import Button from '../ui/Button'
import AvatarPicker from './AvatarPicker'

export default function EditProfileForm({ profile }) {
  const { form, fieldErrors, saving, error: saveError, handleChange, save } = useEditProfile(profile)
  const { updateUser } = useAuth()
  const navigate = useNavigate()
  const [uploading, setUploading] = useState(false)
  const [imageError, setImageError] = useState('')

  async function handleSubmit(event) {
    event.preventDefault()

    const updated = await save()
    if (!updated) return

    updateUser(updated)
    navigate(ROUTES.profile)
  }

  async function handleImageSelect(file) {
    if (!file) return
    setUploading(true)
    setImageError('')
    try {
      const url = await mediaService.uploadProfileImage(file)
      handleChange({ target: { name: 'profileImageUrl', value: url } })
    } catch (err) {
      setImageError(err.message)
    } finally {
      setUploading(false)
    }
  }

  return (
    <form className="profile-card profile-edit" onSubmit={handleSubmit} noValidate>
      <div className="profile-edit__heading">
        <h1 className="profile-edit__title">Editar perfil</h1>
        <AvatarPicker
          name={form.displayName}
          src={form.profileImageUrl || null}
          size="lg"
          uploading={uploading}
          onSelect={handleImageSelect}
        />
        <p className="avatar-picker__hint">Tocá el avatar para cambiar tu foto.</p>
      </div>

      <Input
        name="displayName"
        label="Nombre visible"
        value={form.displayName}
        onChange={handleChange}
        error={fieldErrors.displayName}
        hint={`Entre ${PROFILE_LIMITS.displayNameMin} y ${PROFILE_LIMITS.displayNameMax} caracteres. Es lo que ven los demás.`}
        required
      />

      <Textarea
        name="bio"
        label="Biografía"
        rows={4}
        maxLength={PROFILE_LIMITS.bioMax}
        placeholder="Contá quién sos en pocas palabras."
        value={form.bio}
        onChange={handleChange}
        error={fieldErrors.bio}
        hint={`${form.bio.length}/${PROFILE_LIMITS.bioMax}`}
      />

      {imageError && <p className="form-alert" role="alert">{imageError}</p>}

      <label className="profile-privacy">
        <input
          type="checkbox"
          name="isPrivate"
          checked={form.isPrivate}
          onChange={handleChange}
        />
        <span>
          <span className="profile-privacy__title">Cuenta privada</span>
          <span className="profile-privacy__hint">
            Solo tus amigos pueden ver tus publicaciones, tus amigos y tus seguidores.
          </span>
        </span>
      </label>

      {saveError && (
        <p className="form-alert" role="alert">
          {saveError}
        </p>
      )}

      <div className="profile-edit__actions">
        <Button type="submit" loading={saving}>
          {saving ? 'Guardando…' : 'Guardar cambios'}
        </Button>
        <Button type="button" variant="ghost" onClick={() => navigate(ROUTES.profile)}>
          Cancelar
        </Button>
      </div>
    </form>
  )
}