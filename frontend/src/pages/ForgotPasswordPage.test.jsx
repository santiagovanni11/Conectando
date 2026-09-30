import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import ForgotPasswordPage from './ForgotPasswordPage'
import AuthLayout from '../layouts/AuthLayout'
import { passwordResetService } from '../services/passwordResetService'

vi.mock('../services/passwordResetService', () => ({
  passwordResetService: { requestCode: vi.fn(), confirm: vi.fn() },
}))

/**
 * El flujo completo, de punta a punta.
 *
 * Va aparte del test del hook porque acá se prueba lo que el usuario ve: que
 * las dos pantallas se encadenen y que el formulario deje mandar antes de
 * que haya código y contraseña. Un hook puede estar perfecto y que la
 * pantalla no llegue a la segunda etapa.
 */
const renderPage = () =>
  render(
    <MemoryRouter>
      <ForgotPasswordPage />
    </MemoryRouter>,
  )

/**
 * La label lleva un " *" de required, así que se busca por patrón. Es el
 * mismo criterio que usan los tests de los formularios de cuenta.
 */
const patron = (nombre) => new RegExp(`^${nombre}`)
const campo = (nombre) => screen.getByLabelText(patron(nombre))

/**
 * Variante para después de un cambio de pantalla. El paso siguiente aparece
 * recién cuando el servidor responde, así que hay que esperarlo: con la
 * versión sincrónica se consulta antes de que exista y falla siempre.
 */
const esperarCampo = (nombre) => screen.findByLabelText(patron(nombre))

beforeEach(() => {
  vi.clearAllMocks()
  passwordResetService.requestCode.mockResolvedValue(undefined)
  passwordResetService.confirm.mockResolvedValue(undefined)
})

describe('ForgotPasswordPage', () => {
  it('no repite el título que ya pone el layout', () => {
    // Se monta con el layout porque es como está en la app: el título lo
    // pone AuthLayout, como en el login y el registro. Montada sola, la
    // página no tenía el título repetido y el test no lo veía.
    render(
      <MemoryRouter>
        <AuthLayout title="Recuperar contraseña">
          <ForgotPasswordPage />
        </AuthLayout>
      </MemoryRouter>,
    )

    expect(screen.getAllByRole('heading', { level: 1 })).toHaveLength(1)
    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent(
      'Recuperar contraseña',
    )
  })

  it('empieza pidiendo el correo', () => {
    renderPage()

    expect(campo('Email')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /enviar código/i })).toBeInTheDocument()
  })

  it('pide el código y pasa a poner la contraseña nueva', async () => {
    const user = userEvent.setup()
    renderPage()

    await user.type(campo('Email'), 'ana@ejemplo.com')
    await user.click(screen.getByRole('button', { name: /enviar código/i }))

    expect(await esperarCampo('Código')).toBeInTheDocument()
    expect(campo('Contraseña nueva')).toBeInTheDocument()
    // El correo se ve, así que el usuario sabe a dónde lo mandaron.
    expect(screen.getByText('ana@ejemplo.com')).toBeInTheDocument()
  })

  it('no deja cambiar la contraseña sin código', async () => {
    const user = userEvent.setup()
    renderPage()

    await user.type(campo('Email'), 'ana@ejemplo.com')
    await user.click(screen.getByRole('button', { name: /enviar código/i }))
    await esperarCampo('Código')

    await user.type(campo('Contraseña nueva'), 'NuevaClave9')

    expect(screen.getByRole('button', { name: /cambiar contraseña/i })).toBeDisabled()
  })

  it('cambia la contraseña y avisa que se hizo', async () => {
    const user = userEvent.setup()
    renderPage()

    await user.type(campo('Email'), 'ana@ejemplo.com')
    await user.click(screen.getByRole('button', { name: /enviar código/i }))

    await user.type(await esperarCampo('Código'), '123456')
    await user.type(campo('Contraseña nueva'), 'NuevaClave9')
    await user.type(campo('Repetí la contraseña nueva'), 'NuevaClave9')
    await user.click(screen.getByRole('button', { name: /cambiar contraseña/i }))

    expect(await screen.findByText('Contraseña cambiada')).toBeInTheDocument()
    // Y se dice que las sesiones abiertas se cerraron: es lo que el usuario
    // va a notar en el otro dispositivo y conviene que no lo sorprenda.
    expect(screen.getByText(/sesiones/i)).toBeInTheDocument()
  })

  it('un código vencido deja seguir en la misma pantalla', async () => {
    passwordResetService.confirm.mockRejectedValueOnce(
      new Error('El código no es válido o venció. Pedí uno nuevo.'),
    )

    const user = userEvent.setup()
    renderPage()

    await user.type(campo('Email'), 'ana@ejemplo.com')
    await user.click(screen.getByRole('button', { name: /enviar código/i }))

    await user.type(await esperarCampo('Código'), '000000')
    await user.type(campo('Contraseña nueva'), 'NuevaClave9')
    await user.type(campo('Repetí la contraseña nueva'), 'NuevaClave9')
    await user.click(screen.getByRole('button', { name: /cambiar contraseña/i }))

    expect(await screen.findByRole('alert')).toHaveTextContent(/no es válido/)
    expect(campo('Código')).toBeInTheDocument()
  })
})
