import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import DeleteAccountSection from './DeleteAccountSection'
import { useAuth } from '../../hooks/useAuth'
import { accountService } from '../../services/accountService'

vi.mock('../../hooks/useAuth', () => ({ useAuth: vi.fn() }))
vi.mock('../../services/accountService', () => ({
  accountService: { deleteAccount: vi.fn() },
}))

const campo = (nombre) => screen.getByLabelText(new RegExp(`^${nombre}`))

const llenar = async (user, current, confirm) => {
  await user.clear(campo('Contraseña actual'))
  await user.type(campo('Contraseña actual'), current)
  await user.clear(campo('Repetí la contraseña'))
  await user.type(campo('Repetí la contraseña'), confirm)
}

const abrir = async (user) => {
  await user.click(screen.getByRole('button', { name: /eliminar mi cuenta/i }))
}

beforeEach(() => {
  vi.clearAllMocks()
  useAuth.mockReturnValue({ logout: vi.fn() })
  accountService.deleteAccount.mockResolvedValue(undefined)
})

describe('DeleteAccountSection', () => {
  it('no muestra el formulario hasta que se lo pide', () => {
    render(<DeleteAccountSection />)

    expect(screen.queryByLabelText(/^Contraseña actual/)).toBeNull()
    expect(screen.getByRole('button', { name: /eliminar mi cuenta/i })).toBeTruthy()
  })

  it('no borra si la repetición no coincide', async () => {
    const user = userEvent.setup()
    render(<DeleteAccountSection />)
    await abrir(user)

    await llenar(user, 'Clave1', 'Clave2')
    await user.click(screen.getByRole('button', { name: /eliminar cuenta definitivamente/i }))

    expect(await screen.findByText(/no coinciden/i)).toBeTruthy()
    expect(accountService.deleteAccount).not.toHaveBeenCalled()
  })

  it('borra y cierra sesión cuando las dos coinciden', async () => {
    const logout = vi.fn()
    useAuth.mockReturnValue({ logout })
    const user = userEvent.setup()
    render(<DeleteAccountSection />)
    await abrir(user)

    await llenar(user, 'Clave1', 'Clave1')
    await user.click(screen.getByRole('button', { name: /eliminar cuenta definitivamente/i }))

    await waitFor(() =>
      expect(accountService.deleteAccount).toHaveBeenCalledWith({
        currentPassword: 'Clave1',
        confirmPassword: 'Clave1',
      }),
    )
    await waitFor(() => expect(logout).toHaveBeenCalled())
  })

  it('el botón de cancelar cierra sin borrar nada', async () => {
    const user = userEvent.setup()
    render(<DeleteAccountSection />)
    await abrir(user)

    await user.click(screen.getByRole('button', { name: /cancelar/i }))

    expect(screen.queryByLabelText(/^Contraseña actual/)).toBeNull()
    expect(accountService.deleteAccount).not.toHaveBeenCalled()
  })

  it('vuelve a mostrar el botón si el servidor rechaza', async () => {
    accountService.deleteAccount.mockRejectedValue(new Error('La contraseña actual no es correcta.'))
    const logout = vi.fn()
    useAuth.mockReturnValue({ logout })
    const user = userEvent.setup()
    render(<DeleteAccountSection />)
    await abrir(user)

    await llenar(user, 'MalaClave9', 'MalaClave9')
    await user.click(screen.getByRole('button', { name: /eliminar cuenta definitivamente/i }))

    expect(await screen.findByText(/no es correcta/i)).toBeTruthy()
    expect(logout).not.toHaveBeenCalled()
  })

  it('muestra la contraseña con el botón del ojo', async () => {
    const user = userEvent.setup()
    render(<DeleteAccountSection />)
    await abrir(user)

    const actual = campo('Contraseña actual')
    expect(actual).toHaveAttribute('type', 'password')

    await user.click(screen.getAllByRole('button', { name: /mostrar contraseña/i })[0])

    expect(actual).toHaveAttribute('type', 'text')
  })

  it('el ojo no vacía el campo', async () => {
    const user = userEvent.setup()
    render(<DeleteAccountSection />)
    await abrir(user)

    await user.type(campo('Contraseña actual'), 'Clave1')
    await user.click(screen.getAllByRole('button', { name: /mostrar contraseña/i })[0])

    expect(campo('Contraseña actual')).toHaveValue('Clave1')
  })
})
