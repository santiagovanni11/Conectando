import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import NavItem from './NavItem'

beforeEach(() => {
  vi.clearAllMocks()
})

function renderItem(props) {
  return render(
    <MemoryRouter>
      <NavItem to="/messages" icon="chat" label="Mensajes" {...props} />
    </MemoryRouter>,
  )
}

describe('NavItem', () => {
  it('no muestra badge cuando el contador es cero', () => {
    renderItem({ badge: 0 })

    expect(screen.queryByTestId('badge-Mensajes')).toBeNull()
  })

  it('muestra el número de pendientes', () => {
    renderItem({ badge: 3 })

    expect(screen.getByTestId('badge-Mensajes')).toHaveTextContent('3')
  })

  it('acota el número muy alto para que no rompe el ícono', () => {
    renderItem({ badge: 150 })

    expect(screen.getByTestId('badge-Mensajes')).toHaveTextContent('99+')
  })

  it('anuncia la cantidad a los lectores de pantalla', () => {
    renderItem({ badge: 2 })

    expect(screen.getByRole('link', { name: 'Mensajes, 2 sin leer' })).toBeInTheDocument()
  })

  it('sin contador usa el nombre normal en el aria-label', () => {
    renderItem({ badge: 0 })

    expect(screen.getByRole('link', { name: 'Mensajes' })).toBeInTheDocument()
  })
})