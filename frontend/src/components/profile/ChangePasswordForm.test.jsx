import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import ChangePasswordForm from './ChangePasswordForm'
import { useAuth } from '../../hooks/useAuth'
import { accountService } from '../../services/accountService'
import { campo, llenar, enviarCambioValido } from './profileTestHelpers'

vi.mock('../../hooks/useAuth', () => ({ useAuth: vi.fn() }))
vi.mock('../../services/accountService', () => ({
  accountService: { changePassword: vi.fn() },
}))

// El router se mockea entero: este test es del formulario, no de la
// navegación, y un MemoryRouter real solo agregaría ruido.
vi.mock('react-router-dom', () => ({ useNavigate: () => navigate }))

const logout = vi.fn()
const navigate = vi.fn()

beforeEach(() => {
  vi.clearAllMocks()
  useAuth.mockReturnValue({ logout })
  accountService.changePassword.mockResolvedValue(undefined)
  navigate.mockReset()
})

describe('ChangePasswordForm', () => {
  it('no manda nada si la repetición no coincide', async () => {
    const user = userEvent.setup()
    render(<ChangePasswordForm />)

    await llenar(user, {
      'Contraseña actual': 'Vieja1',
      'Contraseña nueva': 'NuevaClave9',
      'Repetí la contraseña nueva': 'OtraClave9',
    })
    await user.click(screen.getByRole('button', { name: /cambiar contraseña/i }))

    expect(await screen.findByText(/no coinciden/i)).toBeTruthy()
    expect(accountService.changePassword).not.toHaveBeenCalled()
  })

  it('manda las tres contraseñas cuando todo está bien', async () => {
    const user = userEvent.setup()
    render(<ChangePasswordForm />)

    await enviarCambioValido(user)

    await waitFor(() =>
      expect(accountService.changePassword).toHaveBeenCalledWith({
        currentPassword: 'Vieja1',
        newPassword: 'NuevaClave9',
        confirmPassword: 'NuevaClave9',
      }),
    )
  })

  it('saca de la cuenta al cambiar la contraseña', async () => {
    // El token de esta sesión murió con el cambio: seguir "conectado" sería
    // una pantalla que parece viva y falla en la primera acción.
    const user = userEvent.setup()
    render(<ChangePasswordForm />)

    await enviarCambioValido(user)

    await waitFor(() => expect(logout).toHaveBeenCalled())
  })

  it('lleva al login, no a otra parte', async () => {
    const user = userEvent.setup()
    render(<ChangePasswordForm />)

    await enviarCambioValido(user)

    await waitFor(() => expect(navigate).toHaveBeenCalled())
    expect(navigate.mock.calls[0][0]).toBe('/login')
  })

  it('avisa en el login que tiene que entrar con la nueva', async () => {
    // Si solo lo echara, el usuario aterrizaría acá sin entender qué pasó
    // y pensaría que la app se rompió.
    const user = userEvent.setup()
    render(<ChangePasswordForm />)

    await enviarCambioValido(user)

    await waitFor(() => expect(navigate).toHaveBeenCalled())
    const estado = navigate.mock.calls[0][1]
    expect(estado.replace).toBe(true)
    expect(estado.state.notice).toMatch(/volvé a iniciar sesión/i)
  })

  it('no saca de la cuenta si el servidor rechaza', async () => {
    // Un error de validación no puede cerrar la sesión: el usuario podría
    // perder su clave si el server se cae justo al guardar.
    accountService.changePassword.mockRejectedValue(new Error('La contraseña actual no es correcta.'))
    const user = userEvent.setup()
    render(<ChangePasswordForm />)

    await enviarCambioValido(user)

    expect(await screen.findByText(/no es correcta/i)).toBeTruthy()
    expect(logout).not.toHaveBeenCalled()
    expect(navigate).not.toHaveBeenCalled()
  })

  it('vacía los campos después de cambiar', async () => {
    // La contraseña nueva no debe quedar a la vista en el input.
    const user = userEvent.setup()
    render(<ChangePasswordForm />)

    await enviarCambioValido(user)

    await waitFor(() => expect(campo('Contraseña nueva')).toHaveValue(''))
    expect(campo('Contraseña actual')).toHaveValue('')
  })

  it('muestra lo que responde el servidor', async () => {
    accountService.changePassword.mockRejectedValue(new Error('La contraseña actual no es correcta.'))
    const user = userEvent.setup()
    render(<ChangePasswordForm />)

    await llenar(user, {
      'Contraseña actual': 'Malaclave9',
      'Contraseña nueva': 'NuevaClave9',
      'Repetí la contraseña nueva': 'NuevaClave9',
    })
    await user.click(screen.getByRole('button', { name: /cambiar contraseña/i }))

    expect(await screen.findByText(/no es correcta/i)).toBeTruthy()
  })

})
