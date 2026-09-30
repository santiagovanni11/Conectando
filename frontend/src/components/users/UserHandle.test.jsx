import { describe, it, expect } from 'vitest'

import { render, screen } from '@testing-library/react'
import { MemoryRouter, Link } from 'react-router-dom'
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

  const montar = (user, children) =>
    render(
      <MemoryRouter>
        <UserHandle user={user}>{children}</UserHandle>
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

  it('sin arroba usa el nombre de pila', () => {
    // Un enlace sin texto no se puede leer con lector de pantalla ni
    // distinguir de un adorno. Si no hay arroba, el nombre de pila sirve.
    montar({ id: 'u3', displayName: 'Ana' })

    expect(screen.getByRole('link', { name: 'Ana' })).toBeInTheDocument()
    expect(screen.queryByText('@')).not.toBeInTheDocument()
  })

  it('con hijos muestra lo que se le pase y no el arroba', () => {
    // Los comentarios y los chats muestran el nombre de pila. El mismo
    // componente cubre los dos casos para que el enlace al perfil viva en un
    // solo lugar y no haya dos que se queden atrás.
    montar({ id: 'u1', userName: 'ana', displayName: 'Ana López' }, 'Ana López')

    expect(screen.getByRole('link', { name: 'Ana López' })).toHaveAttribute(
      'href',
      '/users/u1',
    )
  })

  it('sin usuario no rompe', () => {
    const { container } = montar(null)

    expect(container).toBeEmptyDOMElement()
  })

  it('con linked={false} no anida un enlace dentro de otro', () => {
    // La fila de la lista de amigos ya es un enlace al perfil. Si el arroba
    // se convirtiendo en otro enlace, el HTML queda inválido y el lector de
    // pantalla anuncia el destino dos veces.
    render(
      <MemoryRouter>
        <Link to="/users/u1">
          <UserHandle user={vivo} linked={false} />
        </Link>
      </MemoryRouter>,
    )

    expect(screen.getAllByRole('link')).toHaveLength(1)
    expect(screen.getByText('@ana')).toBeInTheDocument()
  })

  it('con linked={false} una cuenta eliminada tampoco enlaza', () => {
    render(
      <MemoryRouter>
        <Link to="/users/u2">
          <UserHandle user={eliminado} linked={false} />
        </Link>
      </MemoryRouter>,
    )

    expect(screen.getAllByRole('link')).toHaveLength(1)
    expect(screen.getByText('Cuenta eliminada')).toBeInTheDocument()
  })
})