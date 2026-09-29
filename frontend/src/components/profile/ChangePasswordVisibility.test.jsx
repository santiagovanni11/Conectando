import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import ChangePasswordForm from './ChangePasswordForm'
import { useAuth } from '../../hooks/useAuth'
import { campo } from './profileTestHelpers'

vi.mock('../../hooks/useAuth', () => ({ useAuth: vi.fn() }))
vi.mock('../../services/accountService', () => ({ accountService: { changePassword: vi.fn() } }))
vi.mock('react-router-dom', () => ({ useNavigate: () => vi.fn() }))

/**
 * El botón del ojo.
 *
 * Va aparte porque es una preocupación distinta a la del envío: acá no se
 * manda nada, solo se comprueba que ver la contraseña no rompa el campo.
 * Escribir una clave a ciegas es fácil de errar, y una errata deja la
 * cuenta sin acceso.
 */
const ojo = (indice = 0) =>
  screen.getAllByRole('button', { name: /mostrar contraseña/i })[indice]

const cerrar = (indice = 0) =>
  screen.getAllByRole('button', { name: /ocultar contraseña/i })[indice]

// El índice 1 es el de "Contraseña nueva": los botones van en el mismo orden
// que los campos, actual, nueva, repetida.
const NUEVA = 1

beforeEach(() => {
  vi.clearAllMocks()
  useAuth.mockReturnValue({ logout: vi.fn() })
})

describe('ChangePasswordForm - botón del ojo', () => {
  it('muestra la contraseña', async () => {
    const user = userEvent.setup()
    render(<ChangePasswordForm />)

    expect(campo('Contraseña nueva')).toHaveAttribute('type', 'password')
    await user.click(ojo(NUEVA))
    expect(campo('Contraseña nueva')).toHaveAttribute('type', 'text')
  })

  it('vuelve a ocultarla con el mismo botón', async () => {
    const user = userEvent.setup()
    render(<ChangePasswordForm />)

    await user.click(ojo(NUEVA))
    await user.click(cerrar(NUEVA))
    expect(campo('Contraseña nueva')).toHaveAttribute('type', 'password')
  })

  it('no vacía el campo ni lo pisa', async () => {
    const user = userEvent.setup()
    render(<ChangePasswordForm />)

    await user.type(campo('Contraseña nueva'), 'NuevaClave9')
    await user.click(ojo(NUEVA))

    expect(campo('Contraseña nueva')).toHaveValue('NuevaClave9')
  })

  it('cada campo tiene su propio botón', async () => {
    // Ver una no debería mostrar las otras: son tres claves distintas.
    const user = userEvent.setup()
    render(<ChangePasswordForm />)

    expect(screen.getAllByRole('button', { name: /mostrar contraseña/i })).toHaveLength(3)

    await user.click(ojo(0))

    expect(campo('Contraseña actual')).toHaveAttribute('type', 'text')
    expect(campo('Contraseña nueva')).toHaveAttribute('type', 'password')
    expect(campo('Repetí la contraseña nueva')).toHaveAttribute('type', 'password')
  })
})
