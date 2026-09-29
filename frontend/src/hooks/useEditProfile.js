import { useState } from 'react'
import { userService } from '../services/userService'
import { isDisplayNameValid, isBioValid } from '../utils/validators'

export function useEditProfile(profile) {
  const [form, setForm] = useState({
    displayName: profile?.displayName ?? '',
    bio: profile?.bio ?? '',
    profileImageUrl: profile?.profileImageUrl ?? '',
    isPrivate: profile?.isPrivate ?? false,
  })
  const [fieldErrors, setFieldErrors] = useState({})
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [saved, setSaved] = useState(false)

  function validate(fields) {
    const errors = {}

    if (!isDisplayNameValid(fields.displayName)) {
      errors.displayName = 'El nombre visible debe tener entre 2 y 50 caracteres.'
    }
    if (!isBioValid(fields.bio)) {
      errors.bio = 'La biografía no puede superar los 160 caracteres.'
    }

    return errors
  }

  function handleChange(event) {
    const { name, type, value, checked } = event.target
    const next = type === 'checkbox' ? checked : value

    setForm((prev) => ({ ...prev, [name]: next }))
    setFieldErrors((prev) => (prev[name] ? { ...prev, [name]: '' } : prev))
  }

  async function save() {
    const errors = validate(form)

    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors)
      return null
    }

    setSaving(true)
    setError('')
    setSaved(false)

    try {
      const updated = await userService.updateProfile({
        displayName: form.displayName,
        bio: form.bio,
        // La foto se sube por separado; se manda solo si hay una cargada.
        profileImageUrl: form.profileImageUrl || null,
        isPrivate: form.isPrivate,
      })
      setSaved(true)
      return updated
    } catch (err) {
      setError(err.message)
      return null
    } finally {
      setSaving(false)
    }
  }

  return { form, fieldErrors, saving, saved, error, handleChange, save }
}