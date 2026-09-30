import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, act, waitFor } from '@testing-library/react'
import { usePasswordReset } from './usePasswordReset'
import { passwordResetService } from '../services/passwordResetService'

vi.mock('../services/passwordResetService', () => ({
  passwordResetService: { requestCode: vi.fn(), confirm: vi.fn() },
}))

beforeEach(() => {
  vi.clearAllMocks()
  passwordResetService.requestCode.mockResolvedValue(undefined)
  passwordResetService.confirm.mockResolvedValue(undefined)
})

describe('usePasswordReset', () => {
  it('arranca pidiendo el correo', () => {
    const { result } = renderHook(() => usePasswordReset())

    expect(result.current.step).toBe('correo')
    expect(result.current.email).toBe('')
  })

  it('pide el código y pasa al canje', async () => {
    const { result } = renderHook(() => usePasswordReset())

    await act(async () => {
      await result.current.requestCode('ana@ejemplo.com')
    })

    expect(passwordResetService.requestCode).toHaveBeenCalledWith('ana@ejemplo.com')
    expect(result.current.step).toBe('canje')
    // El correo queda guardado: no se vuelve a escribir en el paso siguiente.
    expect(result.current.email).toBe('ana@ejemplo.com')
  })

  it('manda el código junto con la contraseña nueva', async () => {
    const { result } = renderHook(() => usePasswordReset())

    await act(async () => {
      await result.current.requestCode('ana@ejemplo.com')
    })
    await act(async () => {
      await result.current.confirm({
        code: '123456',
        newPassword: 'NuevaClave9',
        confirmPassword: 'NuevaClave9',
      })
    })

    expect(passwordResetService.confirm).toHaveBeenCalledWith({
      email: 'ana@ejemplo.com',
      code: '123456',
      newPassword: 'NuevaClave9',
      confirmPassword: 'NuevaClave9',
    })
  })

  it('un código rechazado deja al usuario en la misma pantalla', async () => {
    // Regresión de diseño: si el error sacara al paso del correo, el usuario
    // tendría que volver a escribir el email para reintentar.
    const { result } = renderHook(() => usePasswordReset())

    await act(async () => {
      await result.current.requestCode('ana@ejemplo.com')
    })

    passwordResetService.confirm.mockRejectedValueOnce(new Error('El código no es válido o venció.'))

    let ok
    await act(async () => {
      ok = await result.current.confirm({ code: '000000', newPassword: 'NuevaClave9', confirmPassword: 'NuevaClave9' })
    })

    expect(ok).toBe(false)
    expect(result.current.step).toBe('canje')
    await waitFor(() => expect(result.current.error).toContain('no es válido'))
  })

  it('si falla el pedido, no avanza y lo dice', async () => {
    passwordResetService.requestCode.mockRejectedValueOnce(new Error('Demasiadas peticiones.'))

    const { result } = renderHook(() => usePasswordReset())
    let ok
    await act(async () => {
      ok = await result.current.requestCode('ana@ejemplo.com')
    })

    expect(ok).toBe(false)
    expect(result.current.step).toBe('correo')
    expect(result.current.error).toBe('Demasiadas peticiones.')
  })

  it('volver atrás limpia el error y no borra el correo', async () => {
    // El correo conservado permite volver al canje con un clic, sin retipear.
    const { result } = renderHook(() => usePasswordReset())

    await act(async () => {
      await result.current.requestCode('ana@ejemplo.com')
    })
    passwordResetService.confirm.mockRejectedValueOnce(new Error('algo'))
    await act(async () => {
      await result.current.confirm({ code: '000000', newPassword: 'a', confirmPassword: 'a' })
    })

    act(() => result.current.back())

    expect(result.current.step).toBe('correo')
    expect(result.current.error).toBe('')
    expect(result.current.email).toBe('ana@ejemplo.com')
  })

  it('reiniciar deja todo como al principio', async () => {
    const { result } = renderHook(() => usePasswordReset())

    await act(async () => {
      await result.current.requestCode('ana@ejemplo.com')
    })
    act(() => result.current.restart())

    expect(result.current.step).toBe('correo')
    expect(result.current.email).toBe('')
  })
})
