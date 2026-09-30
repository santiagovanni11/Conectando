import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import LoginPage from './LoginPage'
import { useAuth } from '../hooks/useAuth'

vi.mock('../hooks/useAuth', () => ({ useAuth: vi.fn() }))

const login = vi.fn()

/**
 * El aviso que deja el cambio de contraseña al redirigir acá.
 *
 * Sin él, el usuario que acaba de cambiar su clave aterriza en el login sin
 * entender por qué lo echaron, y prueba la contraseña vieja otra vez.
 */
const montarConEstado = (state) =>
  render(
    <MemoryRouter initialEntries={[{ pathname: '/login', state }]}>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
      </Routes>
    </MemoryRouter>,
  )

beforeEach(() => {
  vi.clearAllMocks()
  useAuth.mockReturnValue({ login })
})

describe('LoginPage', () => {
  it('muestra el aviso que le dejaron', () => {
    montarConEstado({ notice: 'Tu contraseña fue actualizada. Volvé a iniciar sesión con la nueva.' })

    expect(screen.getByText(/volvé a iniciar sesión con la nueva/i)).toBeTruthy()
  })

  it('no muestra aviso si entraron sin estado', () => {
    montarConEstado(undefined)

    expect(screen.queryByRole('status')).toBeNull()
  })

  it('el aviso no tapa el formulario', () => {
    montarConEstado({ notice: 'Tu contraseña fue actualizada.' })

    // El aviso y el formulario conviven: si el usuario escribe mal, tiene que
    // ver el motivo sin que desaparezca el contexto de por qué está acá.
    expect(screen.getByText(/tu contraseña fue actualizada/i)).toBeTruthy()
    expect(screen.getByRole('button', { name: /iniciar sesión/i })).toBeTruthy()
    expect(screen.getByLabelText(/^Email/)).toBeTruthy()
  })

  it('ofrece la vía para recuperar la contraseña debajo del botón', () => {
    // Va después del botón de entrar, no junto al campo de contraseña: el
    // formulario termina donde termina la acción, y abajo queda la salida
    // para el que no pudo.
    //
    // Se asserta el orden en el DOM y no solo que exista: es un detalle de
    // composición que se pierde en el primer refactor.
    const { container } = montarConEstado(undefined)

    const enlace = screen.getByRole('link', { name: /olvid/i })
    expect(enlace).toHaveAttribute('href', '/recuperar')

    const hijos = [...container.querySelectorAll('form > *')]
    const boton = hijos.findIndex((n) => n.tagName === 'BUTTON')

    expect(hijos.indexOf(enlace)).toBeGreaterThan(boton)
  })
})
