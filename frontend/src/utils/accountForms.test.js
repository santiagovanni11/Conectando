import { describe, it, expect } from 'vitest'
import { validateChangePassword, validateDeleteAccount } from './accountForms'

/**
 * Reglas de los formularios de cuenta.
 *
 * Son funciones puras, así que se prueban sin montar nada: si una regla se
 * rompe, el error tiene que verse en el campo, no en un toast.
 */
describe('validateChangePassword', () => {
  const buena = {
    currentPassword: 'Vieja1',
    newPassword: 'NuevaClave9',
    confirmPassword: 'NuevaClave9',
  }

  it('acepta un formulario completo y coherente', () => {
    expect(validateChangePassword(buena)).toEqual({ valid: true, fieldErrors: {} })
  })

  it('pide la contraseña actual', () => {
    const { valid, fieldErrors } = validateChangePassword({ ...buena, currentPassword: '' })

    expect(valid).toBe(false)
    expect(fieldErrors.currentPassword).toBeTruthy()
  })

  it('pide la nueva y su repetición', () => {
    const { fieldErrors } = validateChangePassword({
      ...buena,
      newPassword: '',
      confirmPassword: '',
    })

    expect(fieldErrors.newPassword).toBeTruthy()
    expect(fieldErrors.confirmPassword).toBeTruthy()
  })

  it('avisa cuando la repetición no coincide', () => {
    const { valid, fieldErrors } = validateChangePassword({
      ...buena,
      confirmPassword: 'OtraClave9',
    })

    expect(valid).toBe(false)
    expect(fieldErrors.confirmPassword).toBeTruthy()
  })

  it.each([
    ['corta1', 'menos de 8 caracteres'],
    ['Sololetras', 'sin número'],
    ['1234567890', 'sin letra'],
  ])('rechaza una contraseña nueva débil: %s (%s)', (debil) => {
    const { valid, fieldErrors } = validateChangePassword({
      ...buena,
      newPassword: debil,
      confirmPassword: debil,
    })

    expect(valid).toBe(false)
    expect(fieldErrors.newPassword).toBeTruthy()
  })

  it('no se queja de la repetición si la nueva está vacía', () => {
    // Si no hay nueva, el error es de la nueva: marcar también la
    // repetición confunde al usuario sobre qué corregir.
    const { fieldErrors } = validateChangePassword({ ...buena, newPassword: '' })

    expect(fieldErrors.newPassword).toBeTruthy()
    expect(fieldErrors.confirmPassword).toBeUndefined()
  })
})

describe('validateDeleteAccount', () => {
  it('acepta cuando las dos contraseñas coinciden', () => {
    expect(validateDeleteAccount({ currentPassword: 'Clave1', confirmPassword: 'Clave1' }))
      .toEqual({ valid: true, fieldErrors: {} })
  })

  it('rechaza cuando la repetición no coincide', () => {
    const { valid, fieldErrors } = validateDeleteAccount({
      currentPassword: 'Clave1',
      confirmPassword: 'Clave2',
    })

    expect(valid).toBe(false)
    expect(fieldErrors.confirmPassword).toBeTruthy()
  })

  it('pide las dos casillas si están vacías', () => {
    const { valid, fieldErrors } = validateDeleteAccount({
      currentPassword: '',
      confirmPassword: '',
    })

    expect(valid).toBe(false)
    expect(fieldErrors.currentPassword).toBeTruthy()
    expect(fieldErrors.confirmPassword).toBeTruthy()
  })
})
