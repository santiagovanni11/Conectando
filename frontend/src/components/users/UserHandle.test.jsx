import { describe, it, expect } from 'vitest'

import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import UserHandle from './UserHandle'

/**
 * Cómo se ve una cuenta que ya no existe.
 *
 * El caso que importa es el identificador: al darse de baja, la base guarda
 * "eliminado-<id>" como nombre de usuario porque lo usa como clave, y eso
 * nunca puede llegar a pantalla. Se vería como un error y, de paso,
 * confirmaría que ahí hubo una cuenta.
 */
describe('UserHandle', () => {
  const vivo = { id: 'u1', userName: 'ana', displayName: 'Ana' }
  const eliminado = { id: 'u2', displayName: 'Cuenta eliminada', isDeleted: true }

  const montar = (user) =>
    render(
      <MemoryRouter>
        <UserHandle user={user} />
      </MemoryRouter>,
    )

  it('muestra el nombre de una cuenta viva', () => {
    montar(vivo)

    expect(screen.getByRole('link', { name: '@ana' })).toBeInTheDocument()
  })

  it('la cuenta viva lleva a su perfil', () => {
    montar(vivo)

    expect(screen.getByRole('link', { name: '@ana' })).toHaveAttribute(
      'href',
      '/users/u1',
    )
  })

  it('tampoco muestra el identificador si el servidor lo manda crudo', () => {
    // Es lo que se veía antes: una cadena de hexadecimales en pantalla.
    montar({ id: 'u2', userName: 'eliminado-01a0e6f75a5072ef98b3880f2ee6e9d9' })

    expect(screen.getByText('Cuenta eliminada')).toBeInTheDocument()
    expect(screen.queryByText(/eliminado-/)).not.toBeInTheDocument()
  })

  it('una cuenta eliminada no se puede visitar', () => {
    montar(eliminado)

    // El perfil ya no existe: el enlace llevaría a una pantalla de error.
    expect(screen.queryByRole('link')).not.toBeInTheDocument()
  })

  it('sin nombre de usuario tampoco se imprime un arroba suelto', () => {
    montar({ id: 'u3', displayName: 'Cuenta eliminada' })

    expect(screen.getByText('Cuenta eliminada')).toBeInTheDocument()
    expect(screen.queryByText('@')).not.toBeInTheDocument()
  })

  it('sin usuario no rompe', () => {
    const { container } = montar(null)

    expect(container).toBeEmptyDOMElement()
  })
})